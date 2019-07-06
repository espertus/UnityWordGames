using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LibrettoWordGrid : LibrettoGrid
{
    private static int ROWS = Libretto.MYSTERY_WORD_LENGTH + Libretto.TOP_WORD_ROW;
    private static int COLUMNS = System.Math.Max(Libretto.TOP_WORD_LENGTH, Libretto.BOTTOM_WORD_LENGTH);
    private static float Y_OFFSET = 2.2f; // number doesn't seem to matter

    private static int MAX_ABOVE_LETTERS = 0;
    private static int MAX_BELOW_LETTERS = 0;
    private static int MIN_CROSS_LENGTH = 3;

    public LibrettoWordGrid() : base(ROWS, COLUMNS, Y_OFFSET, GRID_TYPE.WORD_GRID) { }

    public List<LibrettoTile> GetRowTiles(int len, int row, int offset)
    {
        var result = new List<LibrettoTile>();
        Debug.Log("In GetRowTiles(" + len + ", " + row + ", " + offset + "), columns = " + columns);
        Assert.IsTrue(len + offset <= columns);
        Assert.IsTrue(row < rows);
        Assert.AreEqual(columns, gridTiles[row].Count);

       for (int i = offset; i < offset + len; i++)
        {
            result.Add(gridTiles[row][i]);
        }
        return result;
    }

    public List<LibrettoTile> GetColumnTiles(int column, int len)
    {
        var result = new List<LibrettoTile>();
        for (int row = 0; row < len; row++)
        {
            result.Add(gridTiles[row][column]);
        }
        /*
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
        */
        return result;
    }

}
