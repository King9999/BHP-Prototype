using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

/* Visual representation of Card scriptable object. */
public class CardObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Card cardData;       //handles card activation also.
    public Sprite cardSprite;

    public void ShowCard(bool toggle)
    {
        gameObject.SetActive(toggle);
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        if (cardData != null)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.SelectCard);
        }
    }

    //displays detailed info about card and plays a short animation
    public void OnPointerEnter(PointerEventData pointer)
    {
        if (cardData != null)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.CardDetails);
            GetDetails(cardData);
        }
    }

    void GetDetails(Card cardData)
    {
        HunterManager hm = Singleton.instance.HunterManager;
        hm.ui.cardNameText.text = cardData.cardName;

        //details are different if player is in combat or on the field
        GameManager gm = Singleton.instance.GameManager;
        if (gm.gameState == GameManager.GameState.Combat)
            hm.ui.cardDetailsText.text = cardData.cardDetails_combat;
        else
            hm.ui.cardDetailsText.text = cardData.cardDetails_field;
    }

}
