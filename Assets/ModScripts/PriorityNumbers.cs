using System.Linq;

public class PriorityNumbers
{
    public int[] Sets;

    private int[] PrioritySet(int set, int lastSnDigit)
    {
        switch (set)
        {
            case 0:
                return new[] { 1, 3, 5, 7, 9 }.Contains(lastSnDigit) ? Enumerable.Range(0, 2).Reverse().ToArray() : Enumerable.Range(0, 2).ToArray();
            case 1:
                return new[] { 1, 4, 7, 0 }.Contains(lastSnDigit) ? new[] { 2, 0, 1, } : new[] { 2, 5, 8 }.Contains(lastSnDigit) ? new[] { 1, 2, 0 } : Enumerable.Range(0, 3).ToArray();
            case 2:
                return new[] { 1, 5, 9 }.Contains(lastSnDigit) ? Enumerable.Range(0, 4).Reverse().ToArray() : new[] { 2, 6, 0 }.Contains(lastSnDigit) ? new[] { 1, 3, 0, 2 } :
                    new[] { 3, 7 }.Contains(lastSnDigit) ? Enumerable.Range(0, 4).ToArray() : new[] { 2, 0, 3, 1 };
        }

        return null;
    }

    public PriorityNumbers(int set, int lastDigit)
    {
        Sets = PrioritySet(set, lastDigit);
    }
}
