using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using StageFlow.Models;

namespace StageFlow.Views.Controls;

/// <summary>
/// Fenêtre permettant d'appliquer plusieurs styles à différentes portions d'un texte.
/// </summary>
internal static class TextFormattingDialog
{
    public static async Task<bool> ShowAsync(TextElement textElement, Window owner)
    {
        var editor = new TextBox
        {
            Text = textElement.Text,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 100,
            Height = 150,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var workingRuns = textElement.TextRuns.Count > 0 &&
                          string.Concat(textElement.TextRuns.Select(run => run.Text)) == textElement.Text
            ? textElement.TextRuns.Select(run => run.Clone()).ToList()
            : new List<TextRun>
            {
                new()
                {
                    Text = textElement.Text,
                    Color = textElement.Color,
                    FontSize = textElement.FontSize,
                    IsBold = textElement.IsBold,
                    IsItalic = textElement.IsItalic
                }
            };

        Color selectedColor;
        try
        {
            selectedColor = Color.Parse(workingRuns.FirstOrDefault()?.Color ?? textElement.Color);
        }
        catch
        {
            selectedColor = Colors.White;
        }

        var sizeBox = new TextBox
        {
            Width = 72,
            Text = (workingRuns.FirstOrDefault()?.FontSize ?? textElement.FontSize)
                .ToString("0", CultureInfo.InvariantCulture)
        };

        var status = new TextBlock
        {
            Text = "Sélectionne une portion du texte, puis choisis une mise en forme.",
            TextWrapping = TextWrapping.Wrap
        };

        var colorPicker = new ColorPicker
        {
            Color = selectedColor,
            Width = 220,
            Height = 250
        };
        colorPicker.ColorChanged += (_, e) => selectedColor = e.NewColor;

        int rememberedSelectionStart = 0;
        int rememberedSelectionLength = 0;

        void RememberSelection()
        {
            int start = Math.Min(editor.SelectionStart, editor.SelectionEnd);
            int length = Math.Abs(editor.SelectionEnd - editor.SelectionStart);

            if (length > 0)
            {
                rememberedSelectionStart = start;
                rememberedSelectionLength = length;
            }
        }

        // Capture après les événements d'entrée : Avalonia peut mettre à jour la
        // sélection après PointerReleased/KeyUp.
        editor.PointerMoved += (_, _) => Dispatcher.UIThread.Post(RememberSelection);
        editor.PointerReleased += (_, _) => Dispatcher.UIThread.Post(RememberSelection);
        editor.KeyUp += (_, _) => Dispatcher.UIThread.Post(RememberSelection);
        editor.LostFocus += (_, _) => RememberSelection();
        editor.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.SelectionStartProperty ||
                e.Property == TextBox.SelectionEndProperty)
            {
                RememberSelection();
            }
        };

        void ApplyToSelection(Action<TextRun> apply)
        {
            RememberSelection();

            int start = Math.Min(editor.SelectionStart, editor.SelectionEnd);
            int length = Math.Abs(editor.SelectionEnd - editor.SelectionStart);

            if (length <= 0)
            {
                start = rememberedSelectionStart;
                length = rememberedSelectionLength;
            }

            if (length <= 0)
            {
                status.Text = "Sélectionne d'abord les caractères à modifier dans le champ de texte.";
                return;
            }

            workingRuns = ApplyRunStyle(
                editor.Text ?? string.Empty,
                workingRuns,
                start,
                length,
                textElement,
                apply);

            status.Text = "Style appliqué. Tu peux sélectionner une autre portion du texte.";
        }

        var boldButton = new Button
        {
            Content = "B",
            FontWeight = FontWeight.Bold,
            Focusable = false
        };
        boldButton.Click += (_, _) => ApplyToSelection(run => run.IsBold = !run.IsBold);

        var italicButton = new Button
        {
            Content = "I",
            FontStyle = FontStyle.Italic,
            Focusable = false
        };
        italicButton.Click += (_, _) => ApplyToSelection(run => run.IsItalic = !run.IsItalic);

        var sizeButton = new Button
        {
            Content = "Appliquer la taille",
            Focusable = false
        };
        sizeButton.Click += (_, _) =>
        {
            if (double.TryParse(sizeBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double size) &&
                size >= 8 && size <= 300)
            {
                ApplyToSelection(run => run.FontSize = size);
            }
            else
            {
                status.Text = "Entre une taille comprise entre 8 et 300.";
            }
        };

        var colorButton = new Button
        {
            Content = "Appliquer la couleur",
            Focusable = false
        };
        colorButton.Click += (_, _) => ApplyToSelection(run => run.Color = selectedColor.ToString());

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { boldButton, italicButton, sizeBox, sizeButton }
        };

        var saveButton = new Button
        {
            Content = "Appliquer et fermer",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        var cancelButton = new Button
        {
            Content = "Annuler",
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var window = new Window
        {
            Title = "Mise en forme du texte",
            Width = 620,
            Height = 560,
            MinWidth = 480,
            MinHeight = 420,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Texte — sélectionne les caractères à mettre en forme :",
                            FontWeight = FontWeight.SemiBold
                        },
                        editor,
                        actions,
                        new TextBlock
                        {
                            Text = "Couleur",
                            FontWeight = FontWeight.SemiBold
                        },
                        colorPicker,
                        colorButton,
                        status,
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Spacing = 8,
                            Children = { cancelButton, saveButton }
                        }
                    }
                }
            }
        };

        saveButton.Click += (_, _) => window.Close(true);
        cancelButton.Click += (_, _) => window.Close(false);

        bool applied = await window.ShowDialog<bool>(owner);
        if (!applied)
            return false;

        textElement.Text = editor.Text ?? string.Empty;
        textElement.TextRuns = NormalizeRuns(textElement.Text, workingRuns, textElement);
        return true;
    }

    private static List<TextRun> ApplyRunStyle(
        string text,
        List<TextRun> runs,
        int start,
        int length,
        TextElement defaults,
        Action<TextRun> apply)
    {
        start = Math.Clamp(start, 0, text.Length);
        length = Math.Clamp(length, 0, text.Length - start);

        if (length == 0)
            return NormalizeRuns(text, runs, defaults);

        var characters = new List<TextRun>(text.Length);

        if (string.Concat(runs.Select(run => run.Text)) == text)
        {
            foreach (var run in runs)
            {
                foreach (char character in run.Text)
                {
                    var characterStyle = run.Clone();
                    characterStyle.Text = character.ToString();
                    characters.Add(characterStyle);
                }
            }
        }
        else
        {
            foreach (char character in text)
            {
                characters.Add(new TextRun
                {
                    Text = character.ToString(),
                    Color = defaults.Color,
                    FontSize = defaults.FontSize,
                    IsBold = defaults.IsBold,
                    IsItalic = defaults.IsItalic
                });
            }
        }

        for (int i = start; i < start + length; i++)
            apply(characters[i]);

        return MergeAdjacentRuns(characters);
    }

    private static List<TextRun> NormalizeRuns(string text, List<TextRun> runs, TextElement defaults)
    {
        if (string.Concat(runs.Select(run => run.Text)) == text)
            return MergeAdjacentRuns(runs.Select(run => run.Clone()).ToList());

        return new List<TextRun>
        {
            new()
            {
                Text = text,
                Color = defaults.Color,
                FontSize = defaults.FontSize,
                IsBold = defaults.IsBold,
                IsItalic = defaults.IsItalic
            }
        };
    }

    private static List<TextRun> MergeAdjacentRuns(List<TextRun> runs)
    {
        var merged = new List<TextRun>();

        foreach (var run in runs)
        {
            if (run.Text.Length == 0)
                continue;

            var previous = merged.LastOrDefault();
            if (previous != null &&
                previous.Color == run.Color &&
                previous.FontSize == run.FontSize &&
                previous.IsBold == run.IsBold &&
                previous.IsItalic == run.IsItalic)
            {
                previous.Text += run.Text;
            }
            else
            {
                merged.Add(run.Clone());
            }
        }

        return merged;
    }
}
