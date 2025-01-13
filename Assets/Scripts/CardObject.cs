using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

/* Visual representation of Card scriptable object. */
public class CardObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Card cardData;       //handles card activation also.
    public Sprite cardSprite, invalidCardSprite;
    bool revertCardOn, animateCardOn, coroutineActive, origPosCaptured;
    public bool mouseOnCard;       //used to check if mouse is hovering over a card.
    public bool cardSelected, cardInvalid;      //cardInvalid = cannot select card.
    Vector3 originalPos;

    /*void OnEnable()
    {
        originalPos = this.transform.position;
        Debug.Log("Original card Pos: " + originalPos);
    }*/

    void Update()
    {
        //need to do this in update instead of Start because the card position isn't correct until after the first frame update apparently.
        if (!origPosCaptured)
        {
            origPosCaptured = true;
            originalPos = transform.position;
        }

        //TODO: cards are not animating correctly, so I'm disabling the coroutines for now.
        /*if (revertCardOn && !coroutineActive)
        {
            StartCoroutine(RevertCard());
        }

        if (animateCardOn && !coroutineActive)
        {
            StartCoroutine(AnimateCard());
        }*/

        /*if (mouseOnCard && !cardInvalid && Input.GetMouseButtonDown(0))
        {
            GameManager gm = Singleton.instance.GameManager;
            if (gm.gameState == GameManager.GameState.Combat)
            {
                if (gm.combatManager.combatState == Combat.CombatState.AttackerTurn)
                    gm.combatManager.attackersCard = cardData;
                else
                    gm.combatManager.defendersCard = cardData;
            }
            else
            {
                CardManager cm = Singleton.instance.CardManager;
                cm.selectedCard = cardData;
                Debug.Log(cm.selectedCard + " selected");
            }

            //change the sort layer
            //GetComponent<SortingGroup>().sortingOrder = 2;

            //HunterManager hm = Singleton.instance.HunterManager;
            //hm.ui.ShowCardCursor(true, this.transform.position);
            CardMenu cardMenu = Singleton.instance.CardMenu;
            cardMenu.ShowCursor(true, this.transform.position);
        }*/
    }

    public void ShowCard(bool toggle)
    {
        gameObject.SetActive(toggle);
    }

    public void UpdateCardSprite(Sprite sprite)
    {
        GetComponent<Image>().sprite = sprite;
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        revertCardOn = true;
        animateCardOn = false;
        mouseOnCard = false;
        Debug.Log("Mouse is on card: " + mouseOnCard);
        //HunterManager hm = Singleton.instance.HunterManager;
        //hm.ui.ShowCardDetails(false);
        CardMenu cm = Singleton.instance.CardMenu;
        cm.ShowCardDetails(false);
        //hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.SelectCard);

        
    }

    //displays detailed info about card and plays a short animation
    public void OnPointerEnter(PointerEventData pointer)
    {
        animateCardOn = true;
        revertCardOn = false;
        mouseOnCard = true;
        Debug.Log("Mouse is on card: " + mouseOnCard);
        CardMenu cm = Singleton.instance.CardMenu;
        cm.ShowCardDetails(true);
        //HunterManager hm = Singleton.instance.HunterManager;
        //hm.ui.ShowCardDetails(true);
            //hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.CardDetails);
         GetDetails(cardData);
        
    }

    public void OnPointerClick(PointerEventData pointer)
    {
        if (cardInvalid)
            return;

        //if (/*mouseOnCard &&*/ !cardInvalid )
        //{
        GameManager gm = Singleton.instance.GameManager;
        Combat combat = Singleton.instance.Combat;
        if (gm.gameState == GameManager.GameState.Combat)
        {
            Hunter attacker = combat.attackerRoom.character as Hunter;
            Hunter defender = combat.defenderRoom.character as Hunter;
            if (combat.combatState == Combat.CombatState.AttackerTurn)
                attacker.combatCard = cardData;
                //combat.attackersCard = cardData;
            else
                //combat.defendersCard = cardData;
                defender.combatCard = cardData;
        }
        else
        {
            CardManager cm = Singleton.instance.CardManager;
            cm.selectedCard = cardData;
            Debug.LogFormat("{0} selected", cm.selectedCard);
        }

            //change the sort layer
            //GetComponent<SortingGroup>().sortingOrder = 2;

            //HunterManager hm = Singleton.instance.HunterManager;
            //hm.ui.ShowCardCursor(true, this.transform.position);
            CardMenu cardMenu = Singleton.instance.CardMenu;
            cardMenu.ShowCursor(true, this.transform.position);
        //}
    }

    void GetDetails(Card cardData)
    {
        //HunterManager hm = Singleton.instance.HunterManager;
       // hm.ui.cardNameText.text = cardData.cardName;

        CardMenu cm = Singleton.instance.CardMenu;
        cm.cardNameText.text = cardData.cardName;

        //details are different if player is in combat or on the field
        GameManager gm = Singleton.instance.GameManager;
        if (gm.gameState == GameManager.GameState.Combat)
        {
            if (cardData.cardType == Card.CardType.Combat || cardData.cardType == Card.CardType.Versatile)
                cm.cardDetailsText.text = cardData.cardDetails_combat;
            else
                //cannot use this card in combat
                cm.cardDetailsText.text = "Can't use this card in combat.";
        }
        else
        {
            if (cardData.cardType == Card.CardType.Field || cardData.cardType == Card.CardType.Versatile)
                cm.cardDetailsText.text = cardData.cardDetails_field;
            else
                //can't use this card in the field
                cm.cardDetailsText.text = "Can't use this card in the field.";
        }
    }

    IEnumerator AnimateCard()
    {
        //StopCoroutine(RevertCard());
        coroutineActive = true;
        //originalPos = transform.position;
        Vector3 newPos = new Vector3(originalPos.x, originalPos.y + 15, originalPos.z);
        float moveSpeed = 200;
        //HunterManager hm = Singleton.instance.HunterManager;
        CardMenu cm = Singleton.instance.CardMenu;

        while (transform.position.y < newPos.y)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + moveSpeed * Time.deltaTime, transform.position.z);
            if (cm.cardCursor.activeSelf && cardSelected)
                cm.cardCursor.transform.position = transform.position;
            yield return null;
        }
        coroutineActive = false;
        animateCardOn = false;
    }

    IEnumerator RevertCard()
    {
        //StopCoroutine(AnimateCard());
        coroutineActive = true;
        float moveSpeed = 200;
        //Vector3 originalPos = transform.position;
        //Vector3 newPos = new Vector3(transform.position.x, transform.position.y - 30, transform.position.z);
        //HunterManager hm = Singleton.instance.HunterManager;
        CardMenu cm = Singleton.instance.CardMenu;

        while (transform.position.y > originalPos.y)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - moveSpeed * Time.deltaTime, transform.position.z);
            if (cm.cardCursor.activeSelf && cardSelected)
                cm.cardCursor.transform.position = transform.position;
            yield return null;
        }
        coroutineActive = false;
        revertCardOn = false;
    }

}
