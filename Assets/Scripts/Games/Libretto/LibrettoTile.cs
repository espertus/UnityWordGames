using UnityEngine;

public class LibrettoTile : GridTile
{
    public enum TILE_TYPE
    {
        EMPTY,      // for invisible tiles
        GAP,        // for target tiles which must be filled in with letters
        PLACED,     // for tiles which were gaps but now have letters
        CLUE,       // tiles containing letters that cannot be moved
        TEMPORARY,  // special state for placed tiles being moved
        BUTTON      // state of tiles in panel
    }

    public SpriteRenderer outline;

    [HideInInspector]
    public LibrettoGrid.GRID_TYPE gridType;

    [HideInInspector]
    public TILE_TYPE tileType;

    private Vector2 localPosition;

    void Start()
    {
        localPosition = transform.localPosition;
    }

    public override string ToString()
    {
        return "[LibrettoTile '" + TypeChar + "' at (" + column + ", " + row + "), active: " + gameObject.activeSelf + "]";
    }

    public bool IsMovable()
    {
        return tileType == TILE_TYPE.PLACED || tileType == TILE_TYPE.BUTTON;
    }

    public bool IsActive()
    {
        return gameObject.activeSelf;
    }
    
    public void ShowTemporary()
    {
        gameObject.SetActive(true);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(true);
        SetColor(LibrettoPalette.GAP_BG_COLOR, LibrettoPalette.GAP_FG_COLOR);
        tileType = TILE_TYPE.TEMPORARY;
    }

    public void ShowGap()
    {
        gameObject.SetActive(true); // Why not false?
        outline.gameObject.SetActive(true);
        tileBg.SetActive(false);
        foreach (var c in charsGO)
            c.SetActive(false);
        outline.color = LibrettoPalette.GAP_BG_COLOR;
        tileType = TILE_TYPE.GAP;
    }

    public void ShowFixed()
    {
        gameObject.SetActive(true);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(true);
        SetColor(LibrettoPalette.FIXED_BG_COLOR, LibrettoPalette.FIXED_FG_COLOR);
        tileType = TILE_TYPE.CLUE;
    }

    // This is not fully tested and probably doesn't work in all cases.
    public void ShowEmpty()
    {
        gameObject.SetActive(false);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(false);
        tileType = LibrettoTile.TILE_TYPE.EMPTY;
    }

    public void Place(char c)
    {
        SetTileData(c);
        ShowPlaced();
    }
    
    public void ShowPlaced()
    {
        gameObject.SetActive(true);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(true);
        SetColor(LibrettoPalette.PLACED_BG_COLOR, LibrettoPalette.PLACED_FG_COLOR);
        tileType = TILE_TYPE.PLACED;
    }

    public void ShowButton()
    {
        gameObject.SetActive(true);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(true);
        SetColor(LibrettoPalette.GAP_BG_COLOR, LibrettoPalette.GAP_FG_COLOR);
        tileType = TILE_TYPE.BUTTON;
    }


    public void ShowWrong()
    {
        if (tileType != TILE_TYPE.PLACED)
            return;

        gameObject.SetActive(true);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(true);
        SetColor(LibrettoPalette.WRONG_BG_COLOR, LibrettoPalette.WRONG_FG_COLOR);
    }

    // call this when a letter is moved from the panel (the tile still remains but hides)
    public void HideInPanel()
    {
        ResetPosition();
        gameObject.SetActive(false);
    }
    
    public void ResetPosition()
    {
        //Debug.Log("In ResetPosition(), changing transform.localPosition for " + this + " from " + transform.localPosition + " to " + localPosition);
        transform.localPosition = localPosition;
    }

}
