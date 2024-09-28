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
    bool revertCardOn, animateCardOn, coroutineActive, origPosCaptured;
    bool mouseOnCard;       //used to check if mouse is hovering over a card.
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

        if (revertCardOn && !coroutineActive)
        {
            StartCoroutine(RevertCard());
        }

        if (animateCardOn && !coroutineActive)
        {
            StartCoroutine(AnimateCard());
        }

        if (mouseOnCard && Input.GetMouseButtonDown(0))
        {
            CardManager cm = Singleton.instance.CardManager;
            cm.selectedCard = cardData;
            Debug.Log(cm.selectedCard + " selected");
        }
    }

    public void ShowCard(bool toggle)
    {
        gameObject.SetActive(toggle);
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        revertCardOn = true;
        animateCardOn = false;
        mouseOnCard = false;
        Debug.Log("Mouse is on card: " + mouseOnCard);
            HunterManager hm = Singleton.instance.HunterManager;
            hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.SelectCard);

        
    }

    //displays detailed info about card and plays a short animation
    public void OnPointerEnter(PointerEventData pointer)
    {
        animateCardOn = true;
        revertCardOn = false;
        mouseOnCard = true;
        Debug.Log("Mouse is on card: " + mouseOnCard);
        HunterManager hm = Singleton.instance.HunterManager;
            hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.CardDetails);
            GetDetails(cardData);
        
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

    IEnumerator AnimateCard()
    {
        //StopCoroutine(RevertCard());
        coroutineActive = true;
        //originalPos = transform.position;
        Vector3 newPos = new Vector3(originalPos.x, originalPos.y + 30, originalPos.z);
        float moveSpeed = 200;

        while (transform.position.y < newPos.y)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + moveSpeed * Time.deltaTime, transform.position.z);
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

        while (transform.position.y > originalPos.y)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - moveSpeed * Time.deltaTime, transform.position.z);
            yield return null;
        }
        coroutineActive = false;
        revertCardOn = false;
    }

}
