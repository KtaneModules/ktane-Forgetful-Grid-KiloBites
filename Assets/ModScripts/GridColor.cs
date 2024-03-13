using System;

public class GridColor
{
    private static readonly string[] colorNames = { "Orange", "Lime", "Turquoise", "Magenta", "[Empty]" };

    public string ColorName { get; private set; }
    public int ColorIndex;

    private int GetIx(string color) => Array.IndexOf(colorNames, color);

    public GridColor(string colorName)
    {
        ColorName = colorName;
        ColorIndex = GetIx(colorName);
    }
}