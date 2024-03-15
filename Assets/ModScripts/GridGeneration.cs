using System;
using System.Collections.Generic;
using System.Linq;
public class GridGeneration
{
    public IEnumerable<GridColor[]> GeneratedColors(int stageCount)
    {
        var colorGrid = new List<GridColor[]>();

        var currentGrid = new GridColor[25];

        for (int i = 0; i < stageCount; i++)
        {
            var generatedNumbers = Enumerable.Range(0, 25).ToList().Shuffle().Take(6).ToArray();

            for (int j = 0; j < generatedNumbers.Length; j++)
                currentGrid[generatedNumbers[j]] = new GridColor(new[] { "Orange", "Lime", "Turquoise", "Magenta" }.PickRandom());

            colorGrid.Add(currentGrid);
        }

        return colorGrid;
    }

    private readonly GridPriority getGridPriority = new GridPriority();

    public IEnumerable<GridColor[]> GetCombinedSets(List<GridColor[]> grids, int set, int sn)
    {
        var combinedSet = new List<GridColor[]>();

        var list = grids;
        var prioritySet = getGridPriority.PrioritySet(set, sn);
        var sets = new GridColor[25];

        while (list.Count > 0)
        {

        }



        return combinedSet;
    }
}
