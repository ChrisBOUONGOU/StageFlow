using System;
using System.Collections.Generic;
using System.Text;

using LibVLCSharp.Shared;

namespace StageFlow.Services
{
    public sealed class VideoPlayerService : IDisposable
    {
        private LibVLC? _libVlc;
        private MediaPlayer? _mediaPlayer;
        private Media? _media;

        public MediaPlayer? Player => _mediaPlayer;

        public bool IsInitialized =>
            _libVlc != null &&
            _mediaPlayer != null;

        public void Initialize()
        {
            if (IsInitialized)
                return;

            Core.Initialize();

            _libVlc = new LibVLC();

            _mediaPlayer =
                new MediaPlayer(_libVlc);
        }

        public bool Load(string filePath)
        {
            if (!IsInitialized)
                Initialize();

            if (!File.Exists(filePath))
                return false;

            _media?.Dispose();

            _media =
                new Media(
                    _libVlc!,
                    new Uri(filePath));

            _mediaPlayer!.Media = _media;

            return true;
        }

        public void Play()
        {
            _mediaPlayer?.Play();
        }

        public void Pause()
        {
            _mediaPlayer?.Pause();
        }

        public void Stop()
        {
            _mediaPlayer?.Stop();
        }

        public void Restart()
        {
            if (_mediaPlayer == null)
                return;

            _mediaPlayer.Time = 0;
            _mediaPlayer.Play();
        }

        public void SetVolume(int volume)
        {
            if (_mediaPlayer == null)
                return;

            _mediaPlayer.Volume =
                Math.Clamp(volume, 0, 100);
        }

        public void Seek(long milliseconds)
        {
            if (_mediaPlayer == null)
                return;

            _mediaPlayer.Time =
                Math.Max(0, milliseconds);
        }

        public long CurrentTime =>
            _mediaPlayer?.Time ?? 0;

        public long Duration =>
            _mediaPlayer?.Length ?? 0;

        public bool IsPlaying =>
            _mediaPlayer?.IsPlaying ?? false;

        public void Dispose()
        {
            _mediaPlayer?.Stop();

            _media?.Dispose();
            _mediaPlayer?.Dispose();
            _libVlc?.Dispose();

            _media = null;
            _mediaPlayer = null;
            _libVlc = null;
        }
    }
}
