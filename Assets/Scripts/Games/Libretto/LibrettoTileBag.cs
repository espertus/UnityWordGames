using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LibrettoTileBag
{
    // Distribution of letters from A-Z, not including blanks
    private static int[] DISTRIBUTION = {
        9, 2, 3, 4, 13, 2, 3, 2, 8,
        1, 1, 4, 3, 5, 7, 3, 1, 6,
        6, 5, 4, 1, 1, 1, 2, 1};

    private static int NUM_TILES = 98;
    private static char[] FULL_BAG; // Do not modify after filling
    private static int nextLetter;

    private LibrettoTileBag() { }

    private static void initialize() 
    {
        if (FULL_BAG == null)
        {
            FULL_BAG = new char[NUM_TILES];
            int offset = 0;
            for (int i = 0; i < 26; i++)
            {
                char letter = (char)((int)'a' + i);
                for (int j = 0; j < DISTRIBUTION[i]; j++)
                {
                    FULL_BAG[offset++] = letter;
                }
            }
            Utils.Scramble(FULL_BAG);
            nextLetter = 0;
            Assert.AreEqual(NUM_TILES, offset);
        }
    }

    /// <summary>
    /// Gets a letter from the the one tile bag, which gets replenished when depleted.
    /// </summary>
    /// <returns>The letter.</returns>
    public static char GetLetter()
    {
        initialize();
        if (nextLetter >= NUM_TILES)
        {
            nextLetter = 0;
            Utils.Scramble(FULL_BAG);
        }
        return FULL_BAG[nextLetter++];
    }
}
