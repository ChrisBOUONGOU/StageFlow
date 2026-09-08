using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Animation;

using Avalonia.Styling;
using Avalonia.Threading;
using StageFlow.Models;
using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;
using Avalonia.VisualTree;

namespace StageFlow.Services
{
    public sealed class TransitionService
    {
        public async Task PlayAsync(
        Control outgoing,
        Control incoming,
        TransitionSettings settings)
        {
            if (settings.Type == TransitionType.Cut)
            {
                outgoing.IsVisible = false;
                incoming.IsVisible = true;
                return;
            }

            double duration =
                Math.Max(
                    0.05,
                    settings.Duration);

            TimeSpan time =
                TimeSpan.FromSeconds(duration);

            incoming.IsVisible = true;

            switch (settings.Type)
            {
                case TransitionType.Fade:
                case TransitionType.Dissolve:
                    await FadeAsync(
                        outgoing,
                        incoming,
                        time);
                    break;

                case TransitionType.SlideLeft:
                    await SlideAsync(
                        outgoing,
                        incoming,
                        time,
                        -1,
                        0);
                    break;

                case TransitionType.SlideRight:
                    await SlideAsync(
                        outgoing,
                        incoming,
                        time,
                        1,
                        0);
                    break;

                case TransitionType.SlideUp:
                    await SlideAsync(
                        outgoing,
                        incoming,
                        time,
                        0,
                        -1);
                    break;

                case TransitionType.SlideDown:
                    await SlideAsync(
                        outgoing,
                        incoming,
                        time,
                        0,
                        1);
                    break;

                default:
                    outgoing.IsVisible = false;
                    incoming.IsVisible = true;
                    break;
            }

            outgoing.IsVisible = false;
            incoming.IsVisible = true;
        }

        private static async Task FadeAsync(
            Control outgoing,
            Control incoming,
            TimeSpan duration)
        {
            incoming.Opacity = 0;

            var incomingAnimation =
                new Animation
                {
                    Duration = duration,

                    Children =
                    {
                    new KeyFrame
                    {
                        Cue = new Cue(0),
                        Setters =
                        {
                            new Setter(
                                Visual.OpacityProperty,
                                0d)
                        }
                    },

                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters =
                        {
                            new Setter(
                                Visual.OpacityProperty,
                                1d)
                        }
                    }
                    }
                };

            var outgoingAnimation =
                new Animation
                {
                    Duration = duration,

                    Children =
                    {
                    new KeyFrame
                    {
                        Cue = new Cue(0),
                        Setters =
                        {
                            new Setter(
                                Visual.OpacityProperty,
                                1d)
                        }
                    },

                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters =
                        {
                            new Setter(
                                Visual.OpacityProperty,
                                0d)
                        }
                    }
                    }
                };

            incoming.IsVisible = true;

            await Task.WhenAll(
                outgoingAnimation.RunAsync(outgoing),
                incomingAnimation.RunAsync(incoming));
        }

        private static async Task SlideAsync(
            Control outgoing,
            Control incoming,
            TimeSpan duration,
            double directionX,
            double directionY)
        {
            if (incoming.Parent is not Panel panel)
            {
                outgoing.IsVisible = false;
                incoming.IsVisible = true;
                return;
            }

            double width =
                panel.Bounds.Width;

            double height =
                panel.Bounds.Height;

            if (width <= 0)
                width = 1920;

            if (height <= 0)
                height = 1080;

            double startX =
                directionX * width;

            double startY =
                directionY * height;

            var transform =
                new TranslateTransform
                {
                    X = startX,
                    Y = startY
                };

            incoming.RenderTransform =
                transform;

            incoming.IsVisible = true;

            var animation =
                new Animation
                {
                    Duration = duration,

                    Children =
                    {
                    new KeyFrame
                    {
                        Cue = new Cue(0),
                        Setters =
                        {
                            new Setter(
                                TranslateTransform.XProperty,
                                startX),

                            new Setter(
                                TranslateTransform.YProperty,
                                startY)
                        }
                    },

                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters =
                        {
                            new Setter(
                                TranslateTransform.XProperty,
                                0d),

                            new Setter(
                                TranslateTransform.YProperty,
                                0d)
                        }
                    }
                    }
                };

            await animation.RunAsync(incoming);

            incoming.RenderTransform = null;
        }
    }
}
