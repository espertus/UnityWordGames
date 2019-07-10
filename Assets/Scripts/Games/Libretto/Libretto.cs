using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;


public class Libretto : MonoBehaviour, IInputHandler
{
    public static int MYSTERY_WORD_LENGTH = 8;
    public static int TOP_WORD_LENGTH = 7;
    public static int BOTTOM_WORD_LENGTH = 7;
    public static int TOP_WORD_ROW = 0;
    private static int NUM_DISTRACTERS = 2;
    private static int MAX_ABOVE_LETTERS = 2;
    private static int MAX_BELOW_LETTERS = 2;
    public static bool TAP_ENABLED = true;

    // Other game objects
    public LibrettoWordGrid wordGrid;
    public LibrettoPanelGrid panelGrid;
    public Text textLabel;

    // State for touching and dragging
    private Vector3 touchPosition;
    private LibrettoTile selectedTile;
    private LibrettoTile tempTileOrigin;
    // The following variable enables detection of when a tile is selected
    // from the panel, moved around, and returned. It should not be counted
    // as a tap.
    private bool leftArea;

    // State for this round
    private string mysteryWord;
    private string topWord;
    private string bottomWord;
    private int topIntercept;
    private int bottomIntercept;
    private PuzzleWord puzzleWord;
    private List<char> buttonChars;
    private int topWordRow;
    private int bottomWordRow;
    private List<char> distractorChars;

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

        // Place horizontal words and select tiles for vertical word.
        Assert.AreEqual(topIntercept, bottomIntercept); // currently constrained to be at some intercepts
        int diff = topIntercept - bottomIntercept;
        List<LibrettoTile> tiles;
        if (diff >= 0)
        {
            PlaceWord(topWord, topWordRow, 0);
            PlaceWord(bottomWord, bottomWordRow, diff);
            tiles = wordGrid.GetColumnTiles(topIntercept, mysteryWord.Length);
        }
        else
        {
            PlaceWord(topWord, topWordRow, -diff);
            PlaceWord(bottomWord, bottomWordRow, 0);
            tiles = wordGrid.GetColumnTiles(bottomIntercept, mysteryWord.Length);
        }

        // Show gap tiles and build up letters for panel.
        puzzleWord = new PuzzleWord(mysteryWord, tiles);
        buttonChars = new List<char>();
        for (int i = 0; i < mysteryWord.Length; i++)
        {
            LibrettoTile tile = tiles[i];
            if (tile.tileType == LibrettoTile.TILE_TYPE.EMPTY)
            {
                tile.ShowGap();
                buttonChars.Add(mysteryWord[i]);
            }
        }

        // Set up distractors.
        distractorChars = new List<char>();
        for (int i = 0; i < NUM_DISTRACTERS; i++)
        {
            char distractor = LibrettoTileBag.GetLetter();
            distractorChars.Add(distractor);
            buttonChars.Add(distractor);
        }
        Debug.Log("distractorChars: " + new string(distractorChars.ToArray()));
        buttonChars = Utils.Scramble<char>(buttonChars);
        Debug.Log("buttonChars: " + new string(buttonChars.ToArray()));
        panelGrid.ShowRowChars(buttonChars);
    }

    void SelectWords()
    {
        mysteryWord = LibrettoDictionary.Instance.getRandomWord(MYSTERY_WORD_LENGTH);
        Assert.IsNotNull(mysteryWord);
        UnityEngine.Debug.Log("mysteryWord: " + mysteryWord);
        topWordRow = UnityEngine.Random.Range(0, MAX_ABOVE_LETTERS);
        topWord = LibrettoDictionary.Instance.getRandomWord(TOP_WORD_LENGTH, mysteryWord[topWordRow], 0);
        topIntercept = topWord.IndexOf(mysteryWord[topWordRow]);
        bottomWordRow = mysteryWord.Length - UnityEngine.Random.Range(0, MAX_BELOW_LETTERS) - 1;
        bottomWord = LibrettoDictionary.Instance.getRandomWord(BOTTOM_WORD_LENGTH, mysteryWord[bottomWordRow], 0);
        bottomIntercept = bottomWord.IndexOf(mysteryWord[bottomWordRow]);
    }

    void PlaceWord(string word, int row, int offset)
    {
        //UnityEngine.Debug.Log("PlaceWord(word = " + word + ", row = " + row + ", offset = " + offset + ")");
        List<LibrettoTile> wordTiles = wordGrid.GetRowTiles(word.Length, row, offset);
        var chars = word.ToCharArray();
        for (int i = 0; i < word.Length; i++)
        {
            LibrettoTile tile = wordTiles[i];
            tile.SetTileData(chars[i]);
            tile.ShowFixed();
            tile.SetColor(Color.white, Color.black);
        }
    }

    // Replace panel tiles with placed letters in puzzle words.
    public void Refresh()
    {
        foreach (LibrettoTile tile in wordGrid.tiles)
        {
            if (tile.tileType == LibrettoTile.TILE_TYPE.PLACED)
            {
                tile.ShowGap();

            }
        }

        panelGrid.ClearTiles();
        panelGrid.ShowRowChars(Utils.Scramble<char>(buttonChars));
    }

    public void GiveHint()
    {
        // First, remove a distractor if any remain.
        if (distractorChars.Count > 0)
        {
            char distractor = distractorChars[0];
            distractorChars.RemoveAt(0);

            //  This removes a tile with the distracting letter from the panel
            // but not from the word grid if it has been placed.
            foreach (LibrettoTile tile in panelGrid.tiles)
            {
                if (tile.TypeChar == distractor)
                {
                    // Remove from tile panel.
                    Debug.Log("Distractor tile found: " + tile);
                    //panelGrid.tiles.Remove(tile); Not sure if needed
                    buttonChars.Remove(distractor); // so it won't reappear on reset
                    tile.tileType = LibrettoTile.TILE_TYPE.EMPTY;
                    bool placed = !tile.gameObject.activeSelf;
                    tile.gameObject.SetActive(false);

                    // Remove from word grid if placed there.
                    if (placed)
                    {
                        foreach (LibrettoTile wordTile in puzzleWord.wordTiles)
                        {
                            Debug.Log("wordTile: " + wordTile);
                            if (wordTile.TypeChar == distractor &&
                            wordTile.tileType == LibrettoTile.TILE_TYPE.PLACED)
                            {
                                wordTile.tileType = LibrettoTile.TILE_TYPE.GAP;
                                wordTile.gameObject.SetActive(false);
                                wordTile.ShowGap();
                                return;
                            }
                        }
                        Assert.IsTrue(false);
                    }
                    return;
                }
            }
            Assert.IsTrue(false); // The tile was not found in panel
        }
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

                selectedTile = tempTile.GetComponent<LibrettoTile>();
                selectedTile.transform.localScale = panelGrid.transform.localScale;
                selectedTile.transform.parent = wordGrid.transform;
       
                selectedTile.transform.localPosition = tile.transform.localPosition;
                selectedTile.gridType = LibrettoGrid.GRID_TYPE.WORD_GRID;
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

        // Is it a tap?
        if (TAP_ENABLED)
        {
            var targetTile = panelGrid.TileCloseToPoint(touchPosition);
            if (selectedTile == targetTile && !leftArea)
            {
                // A tap was detected.
                if (selectedTile.gridType == LibrettoGrid.GRID_TYPE.PANEL_GRID)
                {
                    // Place the tapped tile in the highest gap.
                    foreach (LibrettoTile tile in puzzleWord.wordTiles)
                    {
                        // Drop the tapped tile into the first gap (left to right, top to bottom)
                        if (tile.tileType == LibrettoTile.TILE_TYPE.GAP)
                        {
                            // TODO: Refactor
                            tile.SetTileData(selectedTile.TypeChar);
                            tile.ShowPlaced();
                            selectedTile.ResetPosition();
                            selectedTile.gameObject.SetActive(false);

                            CheckSolution();
                            ClearSelection();
                            return;
                        }
                        // Fall through if no gaps left.
                    }

                }
                // Either a tap failed due to no gap or the tapped tile wasn't in the panel.
                ClearSelection();
                return;
            }
        }

        // Check if moving from the word grid...
        if (selectedTile.tileType == LibrettoTile.TILE_TYPE.TEMPORARY)
        {
            var target = wordGrid.TileCloseToPoint(touchPosition, false);
            if (target != null && target.gameObject.activeSelf && target.tileType == LibrettoTile.TILE_TYPE.GAP)
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

        // Check if moving from the panel grid...
        else if (selectedTile.gridType == LibrettoGrid.GRID_TYPE.PANEL_GRID)
        {
            var target = wordGrid.TileCloseToPoint(touchPosition, false);
            if (target != null && target.gameObject.activeSelf && target.tileType == LibrettoTile.TILE_TYPE.GAP)
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
            if (target != null && target.gameObject.activeSelf && target.tileType == LibrettoTile.TILE_TYPE.GAP)
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
            leftArea = false;
        }
    }


    private void ClearErrors()
    {
        foreach (var t in puzzleWord.wordTiles)
        {
            if (t.tileType == LibrettoTile.TILE_TYPE.PLACED)
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
                Utils.DelayAndCall(this, 2, () =>
                {
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
            if (TAP_ENABLED && panelGrid.TileCloseToPoint(touchPosition) != selectedTile)
            {
                leftArea = true;
            }
            selectedTile.transform.position = touchPosition;
        }
    }



    private struct PuzzleWord
    {
        public List<LibrettoTile> wordTiles;
        public string word;

        public PuzzleWord(string word, List<LibrettoTile> tiles)
        {
            this.word = word;
            this.wordTiles = tiles;
        }

        // This assumes there are exactly 2 CLUE tiles in each word.
        public bool IsCompleted()
        {
            /*
            int BEFORE = 0;
            int WITHIN = 1;
            int AFTER = 2;
            int state = BEFORE;

            foreach (var t in wordTiles)
            {
                if (!t.gameObject.activeSelf)
                {
                    return false;
                }
                switch(t.tileType)
                {
                    case LibrettoTile.TILE_TYPE.GAP:
                        return state == AFTER;
                    case LibrettoTile.TILE_TYPE.CLUE:
                        state++;
                        break;
                    case LibrettoTile.TILE_TYPE.PLACED:
                        break;
                    default:
                        Assert.IsEqual(-1, t.tileType);

                }
            }
            return false;
            */

            foreach (var t in wordTiles)
            {
                if (!t.gameObject.activeSelf)
                    return false;
                if (t.tileType == LibrettoTile.TILE_TYPE.GAP)
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