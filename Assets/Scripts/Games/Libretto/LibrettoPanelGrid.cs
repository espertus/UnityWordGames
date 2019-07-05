using System.Collections;
using System.Collections.Generic;

public class LibrettoPanelGrid : LibrettoGrid
{
    private static int ROWS = 2;
    private static int COLUMNS = 12;
    private static float Y_OFFSET = -3.5f;

    public LibrettoPanelGrid() : base(ROWS, COLUMNS, Y_OFFSET, GRID_TYPE.PANEL_GRID) { }

    // Show the given characters (missing letters) on the tiles for this panel grid.
    public void ShowRowChars(List<char> chars)
    {
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