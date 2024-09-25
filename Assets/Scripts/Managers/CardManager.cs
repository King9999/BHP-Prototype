using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/* Manages all of the card objects in the game. Includes managing a deck of cards and distributing cards to players. 
 A separate class, CardObject, is defined in this script. It's used to provide visual representation of the cards in game. */
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

    public CardObject cardPrefab;
    GameObject cardContainer;
    public List<CardObject> deck;
    // Start is called before the first frame update
    void Start()
    {
        //add all copies of cards in master list to the deck, then shuffle.
        cardContainer = new GameObject("Deck");
        cardContainer.transform.SetParent(this.transform);
        for (int i = 0; i < masterCardList.Length; i++)
        {
            if (masterCardList[i].card == null)
                continue;

            for (int j = 0; j < masterCardList[i].copies; j++)
            {
                CardObject card = Instantiate(cardPrefab, cardContainer.transform);
                card.cardData = masterCardList[i].card;
                card.cardSprite = masterCardList[i].card.cardSprite;
                card.GetComponent<SpriteRenderer>().sprite = card.cardSprite;
                card.ShowCard(false);   //hide card until it's needed
                deck.Add(card);
            }
        }


        ShuffleDeck(deck);
    }

    /// <summary>
    /// Randomizes Card objects in a list.
    /// </summary>
    /// <param name="deck">The cards whose positions will be randomized.</param>
    public void ShuffleDeck(List<CardObject> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            //for each card, get a card at a random location and swap positions.
            CardObject copiedCard = deck[i];
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

    public void DrawCard(Hunter hunter, List<CardObject> deck)
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