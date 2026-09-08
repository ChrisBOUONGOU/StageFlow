using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using StageFlow.Models;
using StageFlow.ViewModels;

namespace StageFlow.Services.Remote
{
    public sealed class RemoteServer : IDisposable
    {
        private readonly MainWindowViewModel _mainViewModel;

        private HttpListener? _listener;

        private CancellationTokenSource? _cancellationTokenSource;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private const int Port = 5180;

        private bool _isRunning;

        private bool _isBlackout;

        public bool IsRunning => _isRunning;

        public int ListeningPort => Port;

        public string LocalIpAddress { get; private set; } = "127.0.0.1";

        public string ConnectionUrl =>
            $"http://{LocalIpAddress}:{Port}";

        public RemoteServer(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public void Start()
        {
            if (_isRunning)
                return;

            LocalIpAddress = GetLocalIpAddress();

            _listener = new HttpListener();

            _listener.Prefixes.Add(
                $"http://+:{Port}/");

            try
            {
                _listener.Start();
            }
            catch
            {
                _listener.Close();
                _listener = null;

                throw;
            }

            _isRunning = true;

            _cancellationTokenSource =
                new CancellationTokenSource();

            _ = Task.Run(
                () => ListenLoopAsync(
                    _cancellationTokenSource.Token));
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;

            try
            {
                _cancellationTokenSource?.Cancel();
            }
            catch
            {
            }

            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch
            {
            }

            _listener = null;

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private async Task ListenLoopAsync(
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext? context = null;

                try
                {
                    if (_listener == null)
                        break;

                    context = await _listener
                        .GetContextAsync()
                        .WaitAsync(cancellationToken);

                    _ = Task.Run(
                        () => HandleRequestAsync(context),
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    if (!_isRunning)
                        break;
                }
            }
        }

        private async Task HandleRequestAsync(
            HttpListenerContext context)
        {
            try
            {
                string path =
                    context.Request.Url?.AbsolutePath
                    ?? "/";

                string method =
                    context.Request.HttpMethod;

                AddCorsHeaders(context.Response);

                if (method == "OPTIONS")
                {
                    context.Response.StatusCode = 204;
                    context.Response.Close();
                    return;
                }

                switch (path)
                {
                    case "/api/remote/state":
                        await SendStateAsync(context);
                        break;

                    case "/api/remote/command":
                        await HandleCommandAsync(context);
                        break;

                    case "/api/remote/ping":
                        await SendJsonAsync(
                            context,
                            new
                            {
                                success = true,
                                application = "StageFlow",
                                version = "1.0"
                            });
                        break;

                    default:
                        context.Response.StatusCode = 404;

                        await SendJsonAsync(
                            context,
                            new
                            {
                                success = false,
                                error = "Endpoint not found."
                            });

                        break;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    context.Response.StatusCode = 500;

                    await SendJsonAsync(
                        context,
                        new
                        {
                            success = false,
                            error = ex.Message
                        });
                }
                catch
                {
                    try
                    {
                        context.Response.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        private async Task SendStateAsync(
            HttpListenerContext context)
        {
            RemoteState state =
                BuildState();

            await SendJsonAsync(
                context,
                state);
        }

        private async Task HandleCommandAsync(
            HttpListenerContext context)
        {
            if (context.Request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 405;

                await SendJsonAsync(
                    context,
                    new
                    {
                        success = false,
                        error = "POST required."
                    });

                return;
            }

            using StreamReader reader =
                new(context.Request.InputStream);

            string body =
                await reader.ReadToEndAsync();

            RemoteCommand? command =
                JsonSerializer.Deserialize<RemoteCommand>(
                    body,
                    _jsonOptions);

            if (command == null)
            {
                context.Response.StatusCode = 400;

                await SendJsonAsync(
                    context,
                    new
                    {
                        success = false,
                        error = "Invalid command."
                    });

                return;
            }

            bool success =
                ExecuteCommand(command);

            if (!success)
            {
                context.Response.StatusCode = 400;
            }

            await SendJsonAsync(
                context,
                new
                {
                    success,
                    state = BuildState()
                });
        }

        private bool ExecuteCommand(
            RemoteCommand command)
        {
            switch (command.Command.ToUpperInvariant())
            {
                case "NEXT":
                    return GoToNextSlide();

                case "PREVIOUS":
                    return GoToPreviousSlide();

                case "GOTO":
                    if (!command.SlideIndex.HasValue)
                        return false;

                    return GoToSlide(
                        command.SlideIndex.Value);

                case "BLACKOUT":
                    _isBlackout = !_isBlackout;
                    return true;

                case "CLEAR_BLACKOUT":
                    _isBlackout = false;
                    return true;

                default:
                    return false;
            }
        }

        private bool GoToNextSlide()
        {
            if (_mainViewModel.Slides.Count == 0)
                return false;

            int currentIndex =
                GetCurrentSlideIndex();

            int nextIndex =
                currentIndex + 1;

            if (nextIndex >=
                _mainViewModel.Slides.Count)
            {
                return false;
            }

            _mainViewModel.SelectedSlide =
                _mainViewModel.Slides[nextIndex];

            return true;
        }

        private bool GoToPreviousSlide()
        {
            if (_mainViewModel.Slides.Count == 0)
                return false;

            int currentIndex =
                GetCurrentSlideIndex();

            int previousIndex =
                currentIndex - 1;

            if (previousIndex < 0)
                return false;

            _mainViewModel.SelectedSlide =
                _mainViewModel.Slides[previousIndex];

            return true;
        }

        private bool GoToSlide(int index)
        {
            if (index < 0 ||
                index >= _mainViewModel.Slides.Count)
            {
                return false;
            }

            _mainViewModel.SelectedSlide =
                _mainViewModel.Slides[index];

            return true;
        }

        private int GetCurrentSlideIndex()
        {
            Slide? selected =
                _mainViewModel.SelectedSlide;

            if (selected == null)
                return 0;

            for (int i = 0;
                 i < _mainViewModel.Slides.Count;
                 i++)
            {
                if (_mainViewModel.Slides[i].Id ==
                    selected.Id)
                {
                    return i;
                }
            }

            return 0;
        }

        private RemoteState BuildState()
        {
            int index =
                GetCurrentSlideIndex();

            Slide? slide =
                _mainViewModel.SelectedSlide;

            return new RemoteState
            {
                Connected = true,

                CurrentSlide =
                    _mainViewModel.Slides.Count == 0
                        ? 0
                        : index + 1,

                TotalSlides =
                    _mainViewModel.Slides.Count,

                SlideTitle =
                    slide?.Title ?? string.Empty,

                IsPresentationRunning = true,

                IsBlackout = _isBlackout
            };
        }

        private static async Task SendJsonAsync(
            HttpListenerContext context,
            object data)
        {
            string json =
                JsonSerializer.Serialize(data);

            byte[] bytes =
                Encoding.UTF8.GetBytes(json);

            context.Response.ContentType =
                "application/json";

            context.Response.ContentEncoding =
                Encoding.UTF8;

            context.Response.ContentLength64 =
                bytes.Length;

            await context.Response.OutputStream
                .WriteAsync(bytes);

            context.Response.Close();
        }

        private static void AddCorsHeaders(
            HttpListenerResponse response)
        {
            response.Headers["Access-Control-Allow-Origin"] =
                "*";

            response.Headers["Access-Control-Allow-Methods"] =
                "GET, POST, OPTIONS";

            response.Headers["Access-Control-Allow-Headers"] =
                "Content-Type";
        }

        private static string GetLocalIpAddress()
        {
            try
            {
                foreach (
                    NetworkInterface networkInterface
                    in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.OperationalStatus !=
                        OperationalStatus.Up)
                    {
                        continue;
                    }

                    if (networkInterface.NetworkInterfaceType ==
                        NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }

                    IPInterfaceProperties properties =
                        networkInterface.GetIPProperties();

                    foreach (
                        UnicastIPAddressInformation address
                        in properties.UnicastAddresses)
                    {
                        if (address.Address.AddressFamily ==
                            AddressFamily.InterNetwork)
                        {
                            return address.Address.ToString();
                        }
                    }
                }
            }
            catch
            {
            }

            return "127.0.0.1";
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
