using System;
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
                    panelGrid.tiles.Remove(tile); // remove so it won't be used later for hint
                    buttonChars.Remove(distractor); // so it won't reappear on reset
                    bool placed = !tile.IsActive();
                    tile.ShowEmpty();

                    // Remove from word grid if placed there.
                    if (placed)
                    {
                        foreach (LibrettoTile wordTile in puzzleWord.wordTiles)
                        {
                            if (wordTile.TypeChar == distractor &&
                            wordTile.tileType == LibrettoTile.TILE_TYPE.PLACED)
                            {
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
        
        // If no distractors are available, show any misplaced letters.
        if (puzzleWord.Has(LibrettoTile.TILE_TYPE.PLACED))
        {
            bool foundWrong = false;
            for (var i = 0; i < puzzleWord.word.Length; i++)
            {
                LibrettoTile wordTile = puzzleWord.wordTiles[i];
                if (wordTile.tileType == LibrettoTile.TILE_TYPE.PLACED && wordTile.TypeChar != puzzleWord.word[i])
                {
                    wordTile.ShowWrong();
                    foundWrong = true;
                }
            }

            if (foundWrong)
            {
                return;
            }
        }

        // fill in a letter from the panel.
        var index = 0;
        foreach (LibrettoTile wordTile in puzzleWord.wordTiles)
        {
            if (wordTile.tileType == LibrettoTile.TILE_TYPE.GAP)
            {
                char c = puzzleWord.word[index];
                foreach (LibrettoTile panelTile in panelGrid.tiles)
                {
                    if (panelTile.TypeChar == c)
                    {
                        // Also make sure this works with dead distractor tiles.
                        if (panelTile.IsActive())
                        {
                            wordTile.Place(c);
                            panelTile.HideInPanel();
                            CheckSolution();
                            return;
                        }
                        // We can't use this panel tile, since it's already been played, but
                        // maybe there's another panel tile with the same letter, so keep looking.
                    }
                }
            }
            index++;
        }
    }

    public void CheckWord()
    {
        string word = puzzleWord.GetCompletedWord();
        if (word != null)
        {
            if (LibrettoDictionary.Instance.IsValidWord(word))
            {
                CheckSolution(); // TODO: Make sure CheckSolution doesn't get called twice.
            }
            else
            {
                puzzleWord.ShowErrors();
            }
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
                            tile.Place(selectedTile.TypeChar);
                            selectedTile.HideInPanel();

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
            // ...to gap in word grid.
            if (target != null && target.gameObject.activeSelf && target.tileType == LibrettoTile.TILE_TYPE.GAP)
            {
                target.Place(selectedTile.TypeChar);
            }
            else
            {
                target = panelGrid.TileCloseToPoint(touchPosition, false);
                // back to panel grid.
                if (target != null && !target.gameObject.activeSelf)
                {
                    target.SetTileData(selectedTile.TypeChar);
                    target.ShowButton();
                    target.Select(true);
                }
                else
                {
                    tempTileOrigin.Place(selectedTile.TypeChar);
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
                target.Place(selectedTile.TypeChar);
                selectedTile.HideInPanel();
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


    public void CheckSolution()
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
        
        /// <summary>
        /// Checks if a word has been completed, i.e., whether all of clue/placed
        /// tiles are contiguous. This assumes there are exactly 2 clue tiles.
        /// </summary>
        /// <returns>the word, or null if a word has not been completed</returns>
        /// <exception cref="Exception">if an invariant is violated</exception>
        public string GetCompletedWord()
        {
            int BEFORE = 0;
            int AFTER_1ST_PLACEMENT = 1;
            int AFTER_1ST_CLUE = 2;
            int AFTER_2ND_CLUE = 3;
            int AFTER_GAP = 4;
            int state = BEFORE;
            List<char> word = new List<char>();

            foreach (var t in wordTiles)
            {
                switch(t.tileType)
                {
                    case LibrettoTile.TILE_TYPE.GAP:
                        if (state == AFTER_1ST_PLACEMENT || state == AFTER_1ST_CLUE)
                        {
                            return null; // non-contiguous word
                        }
                        else if (state == AFTER_2ND_CLUE) {
                            state = AFTER_GAP;
                        }
                        break;
                    case LibrettoTile.TILE_TYPE.CLUE:
                        word.Add(t.TypeChar);
                        if (state == BEFORE || state == AFTER_1ST_PLACEMENT)
                        {
                            state = AFTER_1ST_CLUE;
                        } else if (state == AFTER_1ST_CLUE)
                        {
                            state = AFTER_2ND_CLUE;
                        }
                        else
                        {
                            throw new Exception("More than 2 clue tiles in word.");
                        }
                        break;
                    case LibrettoTile.TILE_TYPE.PLACED:
                        word.Add(t.TypeChar);
                        if (state == BEFORE)
                        {
                            state = AFTER_1ST_PLACEMENT;
                        } else if (state == AFTER_GAP)
                        {
                            return null;  // non-contiguous word
                        }
                        break;
                    default:
                        throw new Exception("Unexpected tile type: " + t.tileType);

                }
            }

            return string.Concat(word.ToArray());
        }


        public bool IsAWord()
        {
            var word = GetCompletedWord();
            if (word != null)
            {
                return LibrettoDictionary.Instance.IsValidWord(word);
            }
            else
            {
                return false;
            }
        }

        public bool IsCompleted()
        {
            return GetCompletedWord() != null;
        }

        /// <summary>
        /// Color all of the placed tiles in the word (even correctly placed ones) to indicate error.
        /// </summary>
        public void ShowErrors()
        {
            foreach (var t in wordTiles)
            {
                t.ShowWrong();
            }
        }

        public bool Has(LibrettoTile.TILE_TYPE tileType)
        {
            foreach (LibrettoTile tile in wordTiles)
            {
                if (tile.tileType == tileType)
                {
                    return true;
                }
            }
            return false;
        }
    }
}