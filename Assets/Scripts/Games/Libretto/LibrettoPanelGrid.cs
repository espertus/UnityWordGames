using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class LibrettoPanelGrid : LibrettoGrid
{
    private static int ROWS = 4;
    private static int COLUMNS = 12;
    private static float Y_OFFSET = -5.5f;

    public LibrettoPanelGrid() : base(ROWS, COLUMNS, Y_OFFSET, GRID_TYPE.PANEL_GRID) { }

    // Show the given characters (missing letters) on the tiles for this panel grid.
    public void ShowRowChars(List<char> chars)
    {
        Assert.IsTrue(chars.Count < tiles.Count);
        var i = 0;
        foreach (var t in tiles)
        {
            t.SetTileData(chars[i]);
            t.ShowPlaced();
            i++;
            if (i == chars.Count)
                break;
        }
    }

    public void ClearTiles()
    {
        foreach (var t in tiles)
        {
            t.gameObject.SetActive(false);
        }
    }

}