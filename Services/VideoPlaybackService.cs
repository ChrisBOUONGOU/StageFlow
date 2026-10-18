using LibVLCSharp.Shared;
using StageFlow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace StageFlow.Services;

/// <summary>
/// Gère une session vidéo commune à l'éditeur et à la présentation.
/// Chaque fenêtre possède son propre lecteur LibVLC, synchronisé avec l'autre.
/// </summary>
public static class VideoPlaybackService
{
    private sealed class Session
    {
        public required string Path { get; init; }
        public required MediaPlayer EditorPlayer { get; init; }
        public required MediaPlayer PresentationPlayer { get; init; }
        public bool Loop { get; set; }
        public double Volume { get; set; }
        public bool PlaybackHeld { get; set; }
        public bool EditorSurfaceReady { get; set; }
        public bool PresentationSurfaceReady { get; set; }
    }

    private static readonly object Gate = new();
    private static readonly LibVLC LibVlc = new();
    private static readonly Dictionary<Guid, Session> Sessions = new();
    private static readonly Timer SyncTimer = new(SynchronizePlayers, null, 250, 250);

    public static MediaPlayer GetPlayer(
        VideoElement video,
        string path,
        bool presentation,
        double volume,
        bool loop)
    {
        lock (Gate)
        {
            if (!Sessions.TryGetValue(video.Id, out var session) ||
                !string.Equals(session.Path, path, StringComparison.OrdinalIgnoreCase))
            {
                if (session is not null)
                    DisposeSession(session);

                session = new Session
                {
                    Path = path,
                    EditorPlayer = CreatePlayer(path),
                    PresentationPlayer = CreatePlayer(path),
                    Loop = loop,
                    Volume = Math.Clamp(volume, 0, 1)
                };

                Sessions[video.Id] = session;
                session.EditorPlayer.EndReached += (_, _) => HandleEnd(video.Id);
                session.PresentationPlayer.EndReached += (_, _) => HandleEnd(video.Id);
            }

            session.Loop = loop;
            session.Volume = Math.Clamp(volume, 0, 1);
            ApplyVolume(session);

            return presentation ? session.PresentationPlayer : session.EditorPlayer;
        }
    }

    /// <summary>Signale l'attachement ou le détachement d'une surface vidéo native.</summary>
    public static void NotifySurfaceReady(
        Guid videoId,
        bool presentation,
        bool ready,
        bool autoPlay = false)
    {
        lock (Gate)
        {
            if (!Sessions.TryGetValue(videoId, out var session))
                return;

            if (presentation)
                session.PresentationSurfaceReady = ready;
            else
                session.EditorSurfaceReady = ready;

            if (!ready || session.PlaybackHeld)
                return;

            var player = presentation ? session.PresentationPlayer : session.EditorPlayer;
            var other = presentation ? session.EditorPlayer : session.PresentationPlayer;

            try
            {
                if (other.IsPlaying)
                {
                    player.Time = Math.Max(0, other.Time);
                    TryPlay(player);
                }
                else if (autoPlay)
                {
                    PlayReadyPlayers(session);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Video surface initialization error: {ex}");
            }
        }
    }

    public static void Start(Guid videoId)
    {
        lock (Gate)
        {
            if (!Sessions.TryGetValue(videoId, out var session) || session.PlaybackHeld)
                return;

            PlayReadyPlayers(session);
        }
    }

    public static bool IsPlaying(Guid videoId)
    {
        lock (Gate)
        {
            return Sessions.TryGetValue(videoId, out var session) &&
                   (session.EditorPlayer.IsPlaying || session.PresentationPlayer.IsPlaying);
        }
    }

    public static void Toggle(Guid videoId)
    {
        if (IsPlaying(videoId))
            Pause(videoId);
        else
            Resume(videoId);
    }

    public static void Pause(Guid videoId)
    {
        lock (Gate)
        {
            if (!Sessions.TryGetValue(videoId, out var session))
                return;

            session.PlaybackHeld = true;
            try
            {
                session.EditorPlayer.Pause();
                session.PresentationPlayer.Pause();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Video pause error: {ex}");
            }
        }
    }

    public static void Resume(Guid videoId)
    {
        lock (Gate)
        {
            if (!Sessions.TryGetValue(videoId, out var session))
                return;

            session.PlaybackHeld = false;
            PlayReadyPlayers(session);
        }
    }

    public static void Restart(Guid videoId)
    {
        lock (Gate)
        {
            if (!Sessions.TryGetValue(videoId, out var session))
                return;

            session.PlaybackHeld = false;
            try
            {
                session.EditorPlayer.Time = 0;
                session.PresentationPlayer.Time = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Video restart error: {ex}");
            }

            PlayReadyPlayers(session);
        }
    }

    public static void Stop(Guid videoId, bool resetPosition = false)
    {
        lock (Gate)
        {
            if (!Sessions.TryGetValue(videoId, out var session))
                return;

            session.PlaybackHeld = true;
            try
            {
                session.EditorPlayer.Stop();
                session.PresentationPlayer.Stop();

                if (resetPosition)
                {
                    session.EditorPlayer.Time = 0;
                    session.PresentationPlayer.Time = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Video stop error: {ex}");
            }
        }
    }

    public static void Seek(Guid videoId, long milliseconds)
    {
        lock (Gate)
        {
            if (!Sessions.TryGetValue(videoId, out var session))
                return;

            long position = Math.Max(0, milliseconds);
            try
            {
                session.EditorPlayer.Time = position;
                session.PresentationPlayer.Time = position;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Video seek error: {ex}");
            }
        }
    }

    public static void SetPresentationActive(bool active)
    {
        // Le lecteur de l'éditeur conserve la sortie audio. On ne change pas
        // de lecteur audio à l'ouverture de la présentation, ce qui évite
        // les coupures provoquées par le basculement de la sortie sonore.
    }

    private static void ApplyVolume(Session session)
    {
        int volume = (int)Math.Round(session.Volume * 100);
        try
        {
            // Une seule sortie audio : le lecteur de l'éditeur.
            session.EditorPlayer.Volume = volume;
            session.PresentationPlayer.Volume = 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Video volume error: {ex}");
        }
    }

    public static void SetLoop(Guid videoId, bool loop)
    {
        lock (Gate)
        {
            if (Sessions.TryGetValue(videoId, out var session))
                session.Loop = loop;
        }
    }

    public static void SetVolume(Guid videoId, double volume)
    {
        lock (Gate)
        {
            if (!Sessions.TryGetValue(videoId, out var session))
                return;

            session.Volume = Math.Clamp(volume, 0, 1);
            ApplyVolume(session);
        }
    }

    public static void KeepPlaybackAlive()
    {
        // Les lecteurs appartiennent au service, pas à la durée de vie d'une vue.
    }

    public static void StopExcept(IEnumerable<Guid> activeVideoIds)
    {
        var active = new HashSet<Guid>(activeVideoIds);

        lock (Gate)
        {
            foreach (var entry in Sessions)
            {
                if (active.Contains(entry.Key))
                    continue;

                var session = entry.Value;
                session.PlaybackHeld = false;

                try
                {
                    session.EditorPlayer.Stop();
                    session.PresentationPlayer.Stop();
                    session.EditorPlayer.Time = 0;
                    session.PresentationPlayer.Time = 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Inactive video stop error: {ex}");
                }
            }
        }
    }

    public static void StopAll(bool resetPosition = false)
    {
        lock (Gate)
        {
            foreach (var session in Sessions.Values)
            {
                try
                {
                    session.EditorPlayer.Stop();
                    session.PresentationPlayer.Stop();

                    if (resetPosition)
                    {
                        session.EditorPlayer.Time = 0;
                        session.PresentationPlayer.Time = 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Video stop error: {ex}");
                }
            }
        }
    }

    private static void PlayReadyPlayers(Session session)
    {
        if (session.EditorSurfaceReady)
            TryPlay(session.EditorPlayer);

        if (session.PresentationSurfaceReady)
            TryPlay(session.PresentationPlayer);
    }

    private static MediaPlayer CreatePlayer(string path)
    {
        var player = new MediaPlayer(LibVlc)
        {
            Media = new Media(LibVlc, path, FromType.FromPath)
        };

        return player;
    }

    private static void TryPlay(MediaPlayer player)
    {
        try
        {
            if (!player.IsPlaying)
                player.Play();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Video playback error: {ex}");
        }
    }

    private static void HandleEnd(Guid videoId)
    {
        lock (Gate)
        {
            if (!Sessions.TryGetValue(videoId, out var session))
                return;

            if (!session.Loop)
            {
                session.PlaybackHeld = true;
                try
                {
                    session.EditorPlayer.Pause();
                    session.PresentationPlayer.Pause();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Video end handling error: {ex}");
                }

                return;
            }

            try
            {
                session.PlaybackHeld = false;
                session.EditorPlayer.Time = 0;
                session.PresentationPlayer.Time = 0;
                PlayReadyPlayers(session);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Video loop error: {ex}");
            }
        }
    }

    private static void SynchronizePlayers(object? state)
    {
        lock (Gate)
        {
            foreach (var session in Sessions.Values)
            {
                if (session.PlaybackHeld)
                    continue;

                try
                {
                    var editor = session.EditorPlayer;
                    var presentation = session.PresentationPlayer;
                    bool editorReady = session.EditorSurfaceReady;
                    bool presentationReady = session.PresentationSurfaceReady;

                    MediaPlayer? master = editorReady && editor.IsPlaying
                        ? editor
                        : presentationReady && presentation.IsPlaying
                            ? presentation
                            : null;

                    if (master is null)
                        continue;

                    MediaPlayer follower = ReferenceEquals(master, editor)
                        ? presentation
                        : editor;

                    bool followerReady = ReferenceEquals(follower, editor)
                        ? editorReady
                        : presentationReady;

                    if (!followerReady)
                        continue;

                    long delta = master.Time - follower.Time;
                    // Éviter les repositionnements fréquents qui peuvent provoquer
                    // des saccades ou des coupures audio.
                    if (Math.Abs(delta) > 900)
                        follower.Time = Math.Max(0, master.Time);

                    if (!follower.IsPlaying)
                        TryPlay(follower);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Video synchronization error: {ex}");
                }
            }
        }
    }

    private static void DisposeSession(Session session)
    {
        try { session.EditorPlayer.Stop(); } catch { }
        try { session.PresentationPlayer.Stop(); } catch { }
        session.EditorPlayer.Dispose();
        session.PresentationPlayer.Dispose();
    }
}
