using System.Linq;

public class GridPriority
{
    public int[] PrioritySet(int set, int sn)
    {
        switch (set)
        {
            case 2:
                return new[] { 1, 3, 5, 7, 9 }.Contains(sn) ? "10".Select(x => int.Parse(x.ToString())).ToArray() : Enumerable.Range(0, 2).ToArray();
            case 3:
                if (new[] { 1, 4, 7, 0 }.Contains(sn))
                    return "201".Select(x => int.Parse(x.ToString())).ToArray();

                else if (new[] { 2, 5, 8 }.Contains(sn))
                    return "120".Select(x => int.Parse(x.ToString())).ToArray();

                return Enumerable.Range(0, 3).ToArray();
            case 4:
                if (new[] { 1, 5, 9 }.Contains(sn))
                    return "3210".Select(x => int.Parse(x.ToString())).ToArray();

                else if (new[] { 2, 6, 0 }.Contains(sn))
                    return "1302".Select(x => int.Parse(x.ToString())).ToArray();

                else if (new[] { 3, 7 }.Contains(sn))
                    return Enumerable.Range(0, 4).ToArray();

                return "2031".Select(x => int.Parse(x.ToString())).ToArray();
        }

        return null;
    }
}
