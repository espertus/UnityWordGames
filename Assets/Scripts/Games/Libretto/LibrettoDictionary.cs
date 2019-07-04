using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Assertions;

public class LibrettoDictionary : MonoBehaviour
{
    private static int MIN_LENGTH = 3;
    private static int MAX_LENGTH = 10; // This limit does not apply to allWords.
    private static int MAX_GROUP = 4;
    private static LibrettoDictionary _instance = null;
    private static System.Random random = new System.Random();

    private HashSet<string> allWords;
    // Array of common words whose length is the index.
    private string[][] wordsOfLength;


    public static LibrettoDictionary Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject instanceGo = new GameObject("GameDictionary");
                _instance = instanceGo.AddComponent<LibrettoDictionary>();
            }

            return _instance;
        }
    }

    public void Initialize()
    {
        StartCoroutine("LoadWordData");
    }

    private bool IsGroupSeparator(string word)
    {
        return word.IndexOf('#') != -1;
    }

    // was IEnumerator
    void LoadWordData()
    {
        // Read in all words from file.
        string dictionaryPath = System.IO.Path.Combine(Application.streamingAssetsPath, "wordsByFrequency.txt");
        string result = System.IO.File.ReadAllText(dictionaryPath);
        var words = result.Split('\n');

        // Count words of each length.
        int group = 0;
        int[] countsByLength = new int[MAX_LENGTH + 1];
        foreach (var w in words)
        {
            if (IsGroupSeparator(w))
            {
                if (++group > MAX_GROUP)
                {
                    break;
                }

            }
            else
            {
                if (w.Length >= MIN_LENGTH && w.Length <= MAX_LENGTH)
                {
                    countsByLength[w.Length]++;
                }
            }
        }

        // Allocate data structures for words.
        allWords = new HashSet<string>();
        wordsOfLength = new string[MAX_LENGTH + 1][];
        for (int i = 0; i < MAX_LENGTH + 1; i++)
        {
            wordsOfLength[i] = new string[countsByLength[i]];
        }

        // Populate data structures.
        int[] counts = new int[MAX_LENGTH + 1]; // Current length
        group = 0;
        foreach (var w in words)
        {
            if (string.IsNullOrEmpty(w) || w.Length < MIN_LENGTH)
                continue;

            var word = w.TrimEnd();

            // A line beginning with # indicates that we are getting to
            // words of lower frequency. 
            if (IsGroupSeparator(word)) {
                group++;
            } else {
                if (group <= MAX_GROUP && word.Length <= MAX_LENGTH)
                {
                    int len = w.Length;
                    Assert.IsTrue(counts[len] < countsByLength[len]);
                    wordsOfLength[len][counts[len]++] = word;
                }
                allWords.Add(word);
            }
        }

        LibrettoGameEvents.DictionaryLoaded();
        // LibrettoPuzzleData.Instance.LoadData ();
    }

    /// <summary>
    /// Checks whether the given word is in the full dictionary.
    /// </summary>
    /// <returns><c>true</c>, if word is valid, <c>false</c> otherwise.</returns>
    /// <param name="word">the word to test</param>
    public bool IsValidWord(string word)
    {
        return allWords.Contains(word);
    }


    /// <summary>
    /// Gets a random word of the given length, optionally containing the
    /// given letter. There is no guarantee that the word has not been 
    /// given earlier in this game. This fails an assertion if no word could
    /// be found.
    /// </summary>
    /// <returns>The random word.</returns>
    /// <param name="len">the length of the word</param>
    /// <param name="containing">a letter that the word must contain, or '-'</param>
    public string getRandomWord(int len, char containing = '-')
    {
        UnityEngine.Assertions.Assert.IsTrue(len <= MAX_LENGTH);
        string word;
        //Debug.Log("Count of words of length " + len + ": " + wordsOfLength[len].Length);
        //Debug.Log("containing: " + containing);
        while (true)
        {
            word = wordsOfLength[len][random.Next(0, wordsOfLength[len].Length)];
            Assert.IsNotNull(word);
            if (containing == '-' || word.IndexOf(containing) != -1)
            {
                return word;
            }
        }
        // This will be reached only if there are no words of the requested
        // length with the specified letter. 
        Assert.IsTrue(false);
        return null;
    }
}
