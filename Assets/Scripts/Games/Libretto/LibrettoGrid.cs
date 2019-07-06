using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class LibrettoGrid : MonoBehaviour {
    public enum GRID_TYPE
    {
        WORD_GRID,
        PANEL_GRID
    }

    public GameObject gridTileGO;

    [HideInInspector]
    public float gridTileSize;

    protected int rows;
    protected int columns; 
    private float offsetY;
    private GRID_TYPE gridType;

    // All rows*columns tiles, in a one-dimensional list
    protected List<LibrettoTile> tiles;

    // All rows*columns tiles, in a two-dimensional list
    protected List<List<LibrettoTile>> gridTiles;

    public LibrettoGrid(int rows, int columns, float offsetY, GRID_TYPE gridType)
    {
        this.rows = rows;
        this.columns = columns;
        this.offsetY = offsetY;
        this.gridType = gridType;
    }

    public LibrettoTile TileCloseToPoint (Vector2 point, bool mustTouch = true) {
        int c = Mathf.FloorToInt ((point.x - gridTiles[0][0].transform.position.x + ( gridTileSize * 0.5f )) / gridTileSize);

        if (c < 0)
            return null;
        if (c >= columns)
            return null;

        int r =  Mathf.FloorToInt ((gridTiles[0][0].transform.position.y + ( gridTileSize * 0.5f ) - point.y ) /  gridTileSize);

        if (r < 0) return null;

        if (r >= rows) return null;

        if (gridTiles.Count <= r)
            return null;

        if (gridTiles[r].Count <= c)
            return null;

        if (!gridTiles [r] [c].touched && mustTouch)
            return null;

        return gridTiles[r][c]; 

    }

    public void BuildGrid ()
    {
        if (tiles != null && tiles.Count != 0) {
            foreach (var t in tiles)
                Destroy (t.gameObject);

            transform.localScale = Vector2.one;
            transform.localPosition = Vector2.zero;
        }

        tiles = new List<LibrettoTile> ();
        gridTiles = new List<List<LibrettoTile>> ();


        for (int row = 0; row < rows; row++) {

            var rowsTiles = new List<LibrettoTile>();

            for (int column = 0; column < columns; column++) {

                var item = Instantiate (gridTileGO) as GameObject;

                var tile = item.GetComponent<LibrettoTile>();
                Assert.IsNotNull(tile);
                tile.SetTilePosition(row, column);
                tile.transform.parent = gameObject.transform;
                tile.gridType = gridType;

                tiles.Add (tile);
                tile.gameObject.SetActive (false);

                rowsTiles.Add (tile);
            }
            gridTiles.Add(rowsTiles);
        }

        ScaleGrid ( Mathf.Abs (gridTiles [0] [0].transform.localPosition.y - gridTiles [1] [0].transform.localPosition.y));

    }

    private void ScaleGrid ( float tileSize) {

        gridTileSize = tileSize;

        var stageWidth = 5.0f;
        var stageHeight = 4.8f;

        var gridWidth = (columns - 1) * gridTileSize;
        var gridHeight = (rows - 1) * gridTileSize;

        var scale = 1.0f;


        if (gridWidth > stageWidth || gridHeight > stageHeight) {

            if (gridWidth >= gridHeight) {
                scale = stageWidth / gridWidth;
            } else {
                scale = stageHeight / gridHeight;
            }
            transform.localScale = new Vector2(scale, scale);
        }

        gridTileSize *= scale;
        var x = (gridWidth * scale) * -0.5f;
        var y = (3.5f - 0.5f * (gridHeight * scale)) + offsetY;
        Debug.Log("localPosition = (" + x + ", " + y + ")");
        transform.localPosition = new Vector2 ((gridWidth * scale) * -0.5f , (3.5f - 0.5f * (gridHeight * scale))  + offsetY);
    }

}
