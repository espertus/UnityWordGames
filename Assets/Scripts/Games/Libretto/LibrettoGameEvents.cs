using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// same as CrossingEvents
public class LibrettoGameEvents
{
    public delegate void Event_DictionaryLoaded();

    public static event Event_DictionaryLoaded OnDictionaryLoaded;

    public static void DictionaryLoaded()
    {
        if (OnDictionaryLoaded != null)
            OnDictionaryLoaded();
    }
}
