using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CrossingWordGrid : CrossingGrid
{
    private static int ROWS = 7;
    private static int COLUMNS = 8;
    private static float Y_OFFSET = 2.2f;

    private static int MAX_ABOVE_LETTERS = 3;
    private static int MAX_BELOW_LETTERS = 3;
    private static int MIN_CROSS_LENGTH = 3;

    public CrossingWordGrid() : base(ROWS, COLUMNS, Y_OFFSET, GRID_TYPE.WORD_GRID) {}

    public List<CrossingTile> GetRowTiles(int len, int row)
    {

        var result = new List<CrossingTile>();
        var diff = columns - len;
        var startIndex = Mathf.FloorToInt(diff / 2);

        while (result.Count < len)
        {
            result.Add(gridTiles[row][startIndex]);
            startIndex++;
        }

        return result;
    }

    public List<CrossingTile> GetColumnTiles(int row, int column)
    {
        var result = new List<CrossingTile>();
        var startIndex = row - Random.Range(0, MAX_ABOVE_LETTERS + 1);
        var bottomHalf = Random.Range(0, MAX_BELOW_LETTERS + 1);
        while (true)
        {
            if (startIndex >= rows || (startIndex > row && startIndex - row >= bottomHalf && result.Count >= MIN_CROSS_LENGTH))
            {
                break;
            }
            var tile = gridTiles[startIndex][column];
            result.Add(tile);
            startIndex++;
        }
        return result;
    }

}