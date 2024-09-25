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
        
    }

    //displays detailed info about card and plays a short animation
    public void OnPointerEnter(PointerEventData pointer)
    {
        
    }

}
