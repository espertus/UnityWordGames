using System.Collections;
using System.Collections.Generic;
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

    public Color wrongColor = Color.red;
    public Color placedColor = Color.white;
    public Color gapColor = Color.blue;
    private Vector2 localPosition;

    void Start()
    {
        localPosition = transform.localPosition;
    }

    public override string ToString()
    {
        return "[LiberttoTile '" + this.TypeChar + "' at (" + this.column + ", " + this.row + ")]";
    }

    public bool IsMovable()
    {
        return tileType == TILE_TYPE.PLACED || tileType == TILE_TYPE.BUTTON;
    }

    public void ShowTemporary()
    {
        gameObject.SetActive(true);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(true);
        SetColor(gapColor, Color.white);
        tileType = TILE_TYPE.TEMPORARY;
    }

    public void ShowGap()
    {
        gameObject.SetActive(true);
        outline.gameObject.SetActive(true);
        tileBg.SetActive(false);
        foreach (var c in charsGO)
            c.SetActive(false);
        outline.color = Color.blue;
        tileType = TILE_TYPE.GAP;
    }

    public void ShowFixed()
    {
        gameObject.SetActive(true);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(true);
        SetColor(placedColor, Color.black);
        tileType = TILE_TYPE.CLUE;
    }

    public void ShowPlaced()
    {
        gameObject.SetActive(true);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(true);
        SetColor(gapColor, Color.white);
        tileType = TILE_TYPE.PLACED;
    }

    public void ShowButton()
    {
        gameObject.SetActive(true);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(true);
        SetColor(gapColor, Color.white);
        tileType = TILE_TYPE.BUTTON;
    }


    public void ShowWrong()
    {

        if (tileType != TILE_TYPE.PLACED)
            return;

        gameObject.SetActive(true);
        outline.gameObject.SetActive(false);
        tileBg.SetActive(true);
        SetColor(wrongColor, Color.white);
    }

    public void ResetPosition()
    {
        Debug.Log("In ResetPosition(), changing transform.localPosition for " + this + " from " + transform.localPosition + " to " + localPosition);
        transform.localPosition = localPosition;
    }

}
