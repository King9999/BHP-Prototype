using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Cards grant various effects to players. Once the deck is empty, a powerful boss appears. Cards can be used in or out of battle, and have different effects
   Depending on when they're used. */
public abstract class Card : ScriptableObject
{
    public string cardName, cardDetails_field, cardDetails_combat;
    public Sprite cardSprite;
    public enum CardType { Versatile, Field, Combat }
    public CardType cardType;

    //trigger conditions
    [Header("---Field Triggers---")]
    public bool triggerWhenDiceRolled;     //used by MOV cards
    public bool triggerAfterMoving;        //used by trap cards
    public bool triggerWhenTrapSprung;     //used by Defense cards
    

    //card effects last for the duration of combat or a turn.
    public virtual void ActivateCard_Field(Hunter user) { }
    public virtual void ActivateCard_Combat(Hunter user) { }
    public virtual void DeactivateCard_Combat(Hunter user) { }  //used to undo any effects without overwriting effects from other sources.
}
