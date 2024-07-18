using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/* Manages all of the card objects in the game. Includes managing a deck of cards and distributing cards to players. */
public class CardManager : MonoBehaviour
{
    /* This struct contains every card in the game, and how many copies there are in a deck. */
    [Serializable]
    public struct MasterCardList
    {
        public Card card;
        public int copies;
    }

    public MasterCardList[] masterCardList;

    //public List<Card> masterCardList;
    public List<Card> deck;
    // Start is called before the first frame update
    void Start()
    {
        //add all copies of cards in master list to the deck, then shuffle.
    }


    public void ShuffleDeck()
    {

    }
}
