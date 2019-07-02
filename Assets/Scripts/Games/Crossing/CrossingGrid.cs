using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CrossingGrid : MonoBehaviour {
    public enum GRID_TYPE
    {
        WORD_GRID,
        PANEL_GRID
    }

	public GameObject gridTileGO;

	[HideInInspector]
	public float GRID_TILE_SIZE;

    private int rows;
    private int columns; 
    private float offsetY;
    private GRID_TYPE gridType;

	private List<CrossingTile> tiles;

	private List<List<CrossingTile>> gridTiles;

    public CrossingGrid(int rows, int columns, float offsetY, GRID_TYPE gridType)
    {
        this.rows = rows;
        this.columns = columns;
        this.offsetY = offsetY;
        this.gridType = gridType;
    }

    public CrossingTile TileCloseToPoint (Vector2 point, bool mustTouch = true) {
		int c = Mathf.FloorToInt ((point.x - gridTiles[0][0].transform.position.x + ( GRID_TILE_SIZE * 0.5f )) / GRID_TILE_SIZE);

		if (c < 0)
			return null;
		if (c >= columns)
			return null;

		int r =  Mathf.FloorToInt ((gridTiles[0][0].transform.position.y + ( GRID_TILE_SIZE * 0.5f ) - point.y ) /  GRID_TILE_SIZE);

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

	public void ClearTiles () {
		foreach (var t in tiles) {
			t.gameObject.SetActive (false);
		}
	}

	public void ShowRowChars (List<char> chars) {
		var i = 0;
		foreach (var t in tiles) {
			t.SetTileData (chars [i]);
			t.ShowPlaced ();
			i++;
			if (i == chars.Count)
				break;
		}
	}

	public List<CrossingTile> GetRowTiles (int len, int row) {

		var result = new List<CrossingTile> ();
		var diff = columns - len;
		var startIndex = Mathf.FloorToInt (diff / 2);

		while (result.Count < len) {
			result.Add (gridTiles [row] [startIndex]);
			startIndex++;
		}

		return result;
	}

	public List<CrossingTile> GetColumnTiles ( int row, int column) {
		var result = new List<CrossingTile> ();
		var startIndex = row - Random.Range(0, 4);
		var bottomHalf = Random.Range (0, 4);
		while (true) {
			if (startIndex >= rows || (startIndex > row && startIndex - row >= bottomHalf && result.Count >= 3)) {
				break;
			}
			var tile = gridTiles [startIndex] [column];
			result.Add (tile);
			startIndex++;
		}
		return result;
	}

	public void BuildGrid ()
	{
		if (tiles != null && tiles.Count != 0) {
			foreach (var t in tiles)
				Destroy (t.gameObject);

			transform.localScale = Vector2.one;
			transform.localPosition = Vector2.zero;
		}

		//a one dimensional list of tiles we can shuffle
		tiles = new List<CrossingTile> ();
		//the two dimensional grid
		gridTiles = new List<List<CrossingTile>> ();


		for (int row = 0; row < rows; row++) {

			var rowsTiles = new List<CrossingTile>();

			for (int column = 0; column < columns; column++) {

				var item = Instantiate (gridTileGO) as GameObject;

				var tile = item.GetComponent<CrossingTile>();
				tile.SetTilePosition(row, column);
				tile.transform.parent = gameObject.transform;
				tile.gridType = gridType;

				tiles.Add (tile);
				tile.gameObject.SetActive (false);

				rowsTiles.Add (tile);
			}
			gridTiles.Add(rowsTiles);
		}

        UnityEngine.Debug.Log("gridTiles.Count = " + gridTiles.Count);
        UnityEngine.Debug.Log("gridTiles[0].Count = " + gridTiles[0].Count);
        UnityEngine.Debug.Log("gridTiles[1].Count = " + gridTiles[1].Count);
		ScaleGrid ( Mathf.Abs (gridTiles [0] [0].transform.localPosition.y - gridTiles [1] [0].transform.localPosition.y));

	}

	private void ScaleGrid ( float tileSize) {

		GRID_TILE_SIZE = tileSize;

		var stageWidth = 5.0f;
		var stageHeight = 4.8f;

		var gridWidth = (columns - 1) * GRID_TILE_SIZE;
		var gridHeight = (rows - 1) * GRID_TILE_SIZE;

		var scale = 1.0f;


		if (gridWidth > stageWidth || gridHeight > stageHeight) {

			if (gridWidth >= gridHeight) {
				scale = stageWidth / gridWidth;
			} else {
				scale = stageHeight / gridHeight;
			}
			transform.localScale = new Vector2(scale, scale);
		}

		GRID_TILE_SIZE *= scale;
		transform.localPosition = new Vector2 ((gridWidth * scale) * -0.5f , (3.5f - 0.5f * (gridHeight * scale))  + offsetY);
	}

}
