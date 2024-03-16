using System;
using System.Collections.Generic;
using System.Linq;
public class GridGeneration
{
    public List<GridColor[]> GeneratedColors(int stageCount)
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

    public List<GridColor[]> GetCombinedSets(List<GridColor[]> grids, int set, int sn)
    {
        var combinedSet = new List<GridColor[]>();

        var list = grids;
        var prioritySet = getGridPriority.PrioritySet(set, sn);

        do
        {
            var sets = new GridColor[25];

            var grabEveryGrid = list.Count < set ? list : list.Take(set).ToList();

            for (int i = 0; i < grabEveryGrid.Count; i++)
            {
                var getIxes = Enumerable.Range(0, 25).Where(x => grabEveryGrid[prioritySet[i]][x] != null).ToList();

                for (int j = 0; j < getIxes.Count; j++)
                    sets[getIxes[j]] = grabEveryGrid[prioritySet[i]][getIxes[j]];

                var getNulls = Enumerable.Range(0, 25).Where(x => grabEveryGrid[prioritySet[i]][x] == null).ToList();

                for (int j = 0; j < getNulls.Count; j++)
                    sets[getNulls[j]] = new GridColor("[Empty]");
            }

            combinedSet.Add(sets);

            list.RemoveRange(0, list.Count < set ? list.Count : set);
        }
        while (list.Count > 0);



        return combinedSet;
    }
}
