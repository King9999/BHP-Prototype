using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Cards grant various effects to players. Once the deck is empty, a powerful boss appears. Cards can be used in or out of battle, and have different effects
   Depending on when they're used. */
public abstract class Card : ScriptableObject
{
    public string cardName, cardDetails_field, cardDetails_combat;
    public Sprite cardSprite;

    public enum CardID { Move1, Move2, Move3, Exit, Attack20, Attack40, Attack60, Pierce, SuperCharge, Defense20, Defense40, Defense60,
                        PerfectDefense, TrapDamage, TrapDizzy, TrapVise, TrapDrain }
    public CardID cardID;       //used to get specific cards from deck.
    public enum CardType { Versatile, Field, Combat }
    public CardType cardType;
    public enum FieldCardType { None, Trap, TrapAvoidance, Movement }
    public FieldCardType fieldCardType;     //used by AI to idenfity specific cards.
    public int weight;          //used by AI to determine card priority. Value is out of 100. The higher, the better.
                                //card weight varies by behaviour, as different behaviours value some cards more than others.

    //trigger conditions
    [Header("---Field Triggers---")]
    public bool triggerWhenDiceRolled;     //used by MOV cards
    public bool triggerBeforeMoving;        //used by trap cards
    public bool triggerWhenTrapSprung;     //used by Defense cards
    

    //card effects last for the duration of combat or a turn.
    public virtual void ActivateCard_Field(Hunter user) { }
    public virtual void DeactivateCard_Field(Hunter user) { }
    public virtual void ActivateCard_Combat(Hunter user) { }
    public virtual void DeactivateCard_Combat(Hunter user) { }  //used to undo any effects without overwriting effects from other sources.
}
