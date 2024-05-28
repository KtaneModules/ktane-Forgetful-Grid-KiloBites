using UnityEngine;

public class GridColorOption
{
    public string ColorName { get; set; }
    public Material Color { get; set; }

    public GridColorOption(string colorName, Material color)
    {
        ColorName = colorName;
        Color = color;
    }
}
