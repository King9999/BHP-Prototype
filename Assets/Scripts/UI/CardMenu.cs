using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

//UI for selecting a card to use either in the field or in combat.

public class CardMenu : MonoBehaviour
{
    public GameObject cardDetailsWindow, cardCursor;
    public TextMeshProUGUI cardDetailsText, cardNameText;
    public List<CardObject> hunterCards;
    private bool mouseOnCard, detailsWindowOpen;

    [Header("---Buttons---")]
    public Button backButton;
    public Button selectCardButton;     //button functions change depending on which scene the game is on.
    public Button skipButton;

    // Start is called before the first frame update
    void Start()
    {
        //Singleton.instance.CardMenu = this;
    }

    void Update()
    {
        /*if (!detailsWindowOpen && mouseOnCard)
        {
            detailsWindowOpen = true;
            ShowCardDetails(true);
        }

        if (!mouseOnCard)
        {
            detailsWindowOpen = false;
            ShowCardDetails(false);
        }

        //check if mouse is hovering over a card
        int i = 0;
        while(!mouseOnCard && i < hunterCards.Count) 
        {
            if (hunterCards[i].mouseOnCard)
            {
                mouseOnCard = true;
            }
            else
            {
                mouseOnCard = false;
                i++;
            }
        }
        //if (i >= hunterCards.Count)
           // mouseOnCard = false;*/
    }

    public void ShowMenu(bool toggle, Character character = null, Card.CardType validCardType = Card.CardType.Field)
    {
        gameObject.SetActive(toggle);
        ShowCardDetails(false);
        ShowCursor(false, Vector3.zero);
        if (toggle == true)
        {
            Vector3 menuPos = Vector3.zero;
            GameManager gm = Singleton.instance.GameManager;

            if (gm.gameState == GameManager.GameState.Dungeon)
            {
                menuPos = new Vector3(character.transform.position.x, character.transform.position.y + 6, character.transform.position.z);
            }
            else if (gm.gameState == GameManager.GameState.Combat)
            {
                menuPos = new Vector3(character.transform.position.x - 16, character.transform.position.y - 5.5f, character.transform.position.z);
            }
            transform.position = Camera.main.WorldToScreenPoint(menuPos);

            //get hunter cards
            if (character is Hunter hunter)
            {
                for (int i = 0; i < hunter.cards.Count; i++)
                {
                    hunterCards[i].ShowCard(true);
                    hunterCards[i].cardData = hunter.cards[i];
                    if (hunter.cards[i].cardType == validCardType || hunter.cards[i].cardType == Card.CardType.Versatile)
                    {
                        hunterCards[i].UpdateCardSprite(hunterCards[i].cardSprite);
                        hunterCards[i].cardInvalid = false;
                    }
                    else
                    {
                        //can't use this card
                        hunterCards[i].cardInvalid = true;
                        hunterCards[i].UpdateCardSprite(hunterCards[i].invalidCardSprite);
                    }

                }

                //sort cards NOTE: will this help with sorting the cards consistently?
                /*var sorted = from card in hunterCards
                             orderby card descending
                             select card;
                foreach (var card in sorted)
                {
                    
                }*/
                hunterCards = hunterCards.OrderByDescending(x => !x.cardInvalid).ToList();  //sorting isn't consistent for some reason
            }
        }
        else
        {
            foreach (CardObject card in hunterCards)
            {
                card.ShowCard(false);
            }
        }
    }

    public void ShowCardDetails(bool toggle)
    {
        cardDetailsWindow.SetActive(toggle);
    }

    public void ShowCursor(bool toggle, Vector3 pos)
    {
        cardCursor.SetActive(toggle);
        if (toggle == true)
        {
            //display cursor over the selected card
            cardCursor.transform.position = pos;

            //check each card and find the one that was selected
            foreach (CardObject card in hunterCards)
            {
                if (card.transform.position == pos)
                {
                    card.cardSelected = true;
                }
                else
                {
                    card.cardSelected = false;
                }
            }
        }
    }
}
