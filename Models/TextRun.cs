namespace StageFlow.Models;

/// <summary>Style appliqué à une portion de texte d'un élément texte.</summary>
public sealed class TextRun
{
    public string Text { get; set; } = string.Empty;
    public string Color { get; set; } = "#FFFFFF";
    public double FontSize { get; set; } = 48;
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }

    public TextRun Clone() => new()
    {
        Text = Text,
        Color = Color,
        FontSize = FontSize,
        IsBold = IsBold,
        IsItalic = IsItalic
    };
}
