using System.Linq;
using UnityEngine;
using static UnityEngine.Random;

public class ColorGrid
{

    private static readonly string[] colorNames = { "Orange", "Lime", "Turquoise", "Magenta", "Empty" };

    public GridColorOption[] Grid;

    public ColorGrid(Material[] colors)
    {
        var randomCoords = Enumerable.Range(0, 25).ToArray().Shuffle().Take(6).ToArray();

        var randomColors = Enumerable.Range(0, 25).Select(x => randomCoords.Contains(x) ? Range(0, 4) : 4).ToArray();

        Grid = Enumerable.Range(0, 25).Select(x => new GridColorOption(colorNames[randomColors[x]], colors[randomColors[x]])).ToArray();

    }
}
