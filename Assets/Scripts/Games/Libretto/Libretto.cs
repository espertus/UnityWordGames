using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;


public class Libretto : MonoBehaviour, IInputHandler
{
    private static int MYSTERY_WORD_LENGTH = 8;
    private static int TOP_WORD_LENGTH = 6;
    private static int BOTTOM_WORD_LENGTH = 7;

    // Other game objects
    public LibrettoWordGrid wordGrid;
    public LibrettoPanelGrid panelGrid;
    public Text textLabel;

    // State for touching and dragging
    private Vector3 touchPosition;
    private CrossingTile selectedTile;
    private CrossingTile tempTileOrigin;

    // Words for this round
    private string mysteryWord;
    private string topWord;
    private string bottomWord;
    private PuzzleWord puzzleWord;

    void Start()
    {
        LibrettoGameEvents.OnDictionaryLoaded += LibrettoGameEvents_OnDictionaryLoaded;
        LibrettoDictionary.Instance.Initialize();
    }

    void LibrettoGameEvents_OnDictionaryLoaded()
    {
        NewPuzzle();
    }

    void NewPuzzle()
    {
        wordGrid.BuildGrid();
        panelGrid.BuildGrid();
        SelectWords();
    }

    void SelectWords()
    {
        mysteryWord = LibrettoDictionary.Instance.getRandomWord(MYSTERY_WORD_LENGTH);
        Assert.IsNotNull(mysteryWord); 
        UnityEngine.Debug.Log("mysteryWord: " + mysteryWord);
        topWord = LibrettoDictionary.Instance.getRandomWord(TOP_WORD_LENGTH, mysteryWord[0]);
        UnityEngine.Debug.Log("topWord: " + topWord);
        bottomWord = LibrettoDictionary.Instance.getRandomWord(BOTTOM_WORD_LENGTH, mysteryWord[mysteryWord.Length - 1]);
        UnityEngine.Debug.Log("bottomWord: " + bottomWord);
    }

    public void HandleTouchDown(Vector2 touch)
    {

        ClearSelection();

        touchPosition = Camera.main.ScreenToWorldPoint(touch);
        touchPosition.z = 0;


        //check panel grid
        var tile = panelGrid.TileCloseToPoint(touchPosition);


        if (tile == null || !tile.gameObject.activeSelf)
        {

            //check word grid
            tile = wordGrid.TileCloseToPoint(touchPosition);
            if (tile != null && tile.gameObject.activeSelf && tile.IsMovable())
            {
                //pick tile from panel
                var tempTile = Instantiate(panelGrid.gridTileGO) as GameObject;
                tempTileOrigin = tile;

                selectedTile = tempTile.GetComponent<CrossingTile>();
                selectedTile.transform.localScale = panelGrid.transform.localScale;
                selectedTile.transform.parent = wordGrid.transform;
                selectedTile.transform.localPosition = tile.transform.localPosition;
                selectedTile.gridType = CrossingGrid.GRID_TYPE.WORD_GRID;
                selectedTile.SetTileData(tile.TypeChar);
                selectedTile.ShowTemporary();

                tile.ShowGap();
            }

        }
        else
        {
            selectedTile = tile;
        }

        if (selectedTile != null) selectedTile.Select(true);

    }



    public void HandleTouchUp(Vector2 touch)
    {
        if (selectedTile == null)
        {
            return;
        }


        if (selectedTile.tileType == CrossingTile.TILE_TYPE.TEMPORARY)
        {

            var target = wordGrid.TileCloseToPoint(touchPosition, false);
            if (target != null && target.gameObject.activeSelf && target.tileType == CrossingTile.TILE_TYPE.GAP)
            {
                target.SetTileData(selectedTile.TypeChar);
                target.ShowPlaced();
            }
            else
            {
                target = panelGrid.TileCloseToPoint(touchPosition, false);
                if (target != null && !target.gameObject.activeSelf)
                {
                    target.SetTileData(selectedTile.TypeChar);
                    target.ShowButton();
                }
                else
                {
                    tempTileOrigin.SetTileData(selectedTile.TypeChar);
                    tempTileOrigin.ShowPlaced();
                }
            }
            Destroy(selectedTile.gameObject);

        }
        else if (selectedTile.gridType == CrossingGrid.GRID_TYPE.PANEL_GRID)
        {
            var target = wordGrid.TileCloseToPoint(touchPosition, false);
            if (target != null && target.gameObject.activeSelf && target.tileType == CrossingTile.TILE_TYPE.GAP)
            {
                target.SetTileData(selectedTile.TypeChar);
                target.ShowPlaced();
                selectedTile.ResetPosition();
                selectedTile.gameObject.SetActive(false);
            }
            else
            {
                selectedTile.ResetPosition();
                selectedTile.ShowPlaced();
            }

        }
        else
        {
            var target = wordGrid.TileCloseToPoint(touchPosition, false);
            if (target != null && target.gameObject.activeSelf && target.tileType == CrossingTile.TILE_TYPE.GAP)
            {
                target.SetTileData(selectedTile.TypeChar);
                selectedTile.ShowGap();
            }
            else
            {
                selectedTile.ResetPosition();
            }

        }

        CheckSolution();

        ClearSelection();

    }

    public void HandleTouchMove(Vector2 touch)
    {
        touchPosition = Camera.main.ScreenToWorldPoint(touch);
        touchPosition.z = 0;
    }


    private void ClearSelection()
    {

        if (selectedTile != null)
        {
            selectedTile.selected = false;
            selectedTile = null;
        }
    }


    private void ClearErrors()
    {
            foreach (var t in puzzleWord.wordTiles)
            {
                if (t.tileType == CrossingTile.TILE_TYPE.PLACED)
                {
                    t.ShowPlaced();
                }
            }
    }



    private void CheckSolution()
    {
        ClearErrors();

        bool allCompleted = true;
        bool allWords = true;

            if (!puzzleWord.IsCompleted())
            {
                allCompleted = false;
            }
            else
            {
                if (!puzzleWord.IsAWord())
                {
                    allWords = false;
                    puzzleWord.ShowErrors();
                }
            }
       
        if (allCompleted)
        {
            if (allWords)
            {
                //SUCCESS
                textLabel.gameObject.SetActive(true);
                Utils.DelayAndCall(this, 2, () => {
                    textLabel.gameObject.SetActive(false);
                    NewPuzzle();
                });
            }
        }
    }




    void Update()
    {
        if (selectedTile != null)
        {
            selectedTile.transform.position = touchPosition;
        }
    }



    private struct PuzzleWord
    {
        public List<CrossingTile> wordTiles;
        public string word;

        public PuzzleWord(string word, List<CrossingTile> tiles)
        {
            this.word = word;
            this.wordTiles = tiles;
        }

        public bool IsCompleted()
        {
            foreach (var t in wordTiles)
            {
                if (!t.gameObject.activeSelf)
                    return false;
                if (t.tileType == CrossingTile.TILE_TYPE.GAP)
                    return false;
            }
            return true;
        }

        public bool IsAWord()
        {
            if (IsCompleted())
            {
                char[] c = new char[wordTiles.Count];
                var i = 0;
                foreach (var t in wordTiles)
                {
                    c[i] = t.TypeChar;
                    i++;
                }
                var newWord = new string(c);
                return LibrettoDictionary.Instance.IsValidWord(newWord);
            }
            return false;
        }

        public void ShowErrors()
        {
            foreach (var t in wordTiles)
            {

                t.ShowWrong();
            }
        }

    }

}

