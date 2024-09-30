using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

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
    public List<Card> deck;
    public Card selectedCard;           //reference to card a player picks in the field/during combat.
    private int maxHand { get; } = 5;   //total number of cards a hunter can have in their hand.

    // Start is called before the first frame update
    void Start()
    {
        //add all copies of cards in master list to the deck, then shuffle.
        //cardContainer = new GameObject("Deck");
        //cardContainer.transform.SetParent(this.transform);
        for (int i = 0; i < masterCardList.Length; i++)
        {
            if (masterCardList[i].card == null)
                continue;

            for (int j = 0; j < masterCardList[i].copies; j++)
            {
                deck.Add(Instantiate(masterCardList[i].card));
                /*CardObject card = Instantiate(cardPrefab, cardContainer.transform);
                card.cardData = masterCardList[i].card;
                card.cardSprite = masterCardList[i].card.cardSprite;
                card.GetComponent<Image>().sprite = card.cardSprite;
                card.ShowCard(false);   //hide card until it's needed
                deck.Add(card);*/
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
            //CardObject copiedCard = deck[i];
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

    public void DrawCard(Hunter hunter, List<Card> deck, int amount = 1)
    {
        if (deck.Count <= 0 || amount > deck.Count)
        {
            Debug.Log("No more cards to draw!");
            return;
        }

        int i = 0;
        do
        {
            if (hunter.cards.Count < maxHand)
            {
                hunter.cards.Add(deck[0]);
                Debug.Log("Drew card " + deck[0].cardName);
            }
            else
            {
                Debug.Log(hunter.characterName + "'s hand is full! Discarding " + deck[0].cardName);
            }

            deck.Remove(deck[0]);
            i++;
        }
        while (deck.Count > 0 && i < amount);
        UpdateDeckCount();
    }

    public void UpdateDeckCount()
    {
        HunterManager hm = Singleton.instance.HunterManager;
        hm.ui.deckCountText.text = "Cards in Deck: " + deck.Count;
    }
}