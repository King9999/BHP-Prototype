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
        for (int i = 0; i < masterCardList.Length; i++)
        {
            if (masterCardList[i].card == null)
                continue;

            for (int j = 0; j < masterCardList[i].copies; j++)
            {
                deck.Add(Instantiate(masterCardList[i].card));
            }
        }


        ShuffleDeck(deck);
    }

    /// <summary>
    /// Randomizes Card objects in a list.
    /// </summary>
    /// <param name="deck">The cards whose positions will be randomized.</param>
    public void ShuffleDeck(List<Card> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            //for each card, get a card at a random location and swap positions.
            Card copiedCard = deck[i];
            int randCard;
            do
            {
                randCard = UnityEngine.Random.Range(0, deck.Count);
            }
            while (randCard == i);

            deck[i] = deck[randCard];
            deck[randCard] = copiedCard;
        }
    }

    public void DrawCard(Hunter hunter, List<Card> deck)
    {
        if (deck.Count <= 0)
        {
            Debug.Log("No more cards to draw!");
            return;
        }

        hunter.cards.Add(deck[0]);
        deck.Remove(deck[0]);
    }
}
