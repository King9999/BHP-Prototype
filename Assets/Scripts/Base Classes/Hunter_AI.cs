using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* CPU Hunters use a scriptable object that control their behaviour. These objects will have toggles that determine a hunter's
 * actions. Behaviours also influence which stats are more likely to rise during hunter generation. 
 * This allows CPU hunters to specialize in a few stats, while being weak in others. */
public abstract class Hunter_AI : ScriptableObject
{
    public enum BehaviourType { Aggro, Bully, Opportunist, Mage, Turtle, Ninja }
    [Header("---AI Behaviours---")]
    public BehaviourType behaviourType;        //internal info only. It tells me what kind of behaviour the hunter has.
    public bool canAttackHunters;
    public bool canAttackMonsters;          //also includes bosses
    public bool canOpenChests;
    public bool canUseTerminals;

    [Header("---Stats Influence---")]
    
    public int rollStr;
    public int rollVit;               /* These are the odds that a stat is chosen during hunter generation.
                                    * The sum of these values must be 100% */
    public int rollSpd;
    public int rollMnt;

    //Some behaviours have special abilities that are triggered on their turn.
    public virtual void ActivateAbility(Hunter hunter) { }

    //choose a card based on behaviour.
    public virtual Card ChooseCard_Field(Hunter hunter) 
    {
        if (hunter.cards.Count <= 0)
            return null;

        Card card = null;
        List<Card> topCards = new List<Card>();

        for (int i = 0; i < hunter.cards.Count; i++)
        {
            //movement cards are higher priority than trap cards, unless hunter is a ninja
            //gather all of the field and versatile cards
            if (hunter.cards[i].cardType == Card.CardType.Field || hunter.cards[i].cardType == Card.CardType.Versatile)
            {
                //behaviours change the weight of cards

                //general conditions
                if (hunter.HasTargetItem() && hunter.cards[i] is Card_Exit exitCard)
                {
                    exitCard.weight = 100;
                }

                if (hunter.super.superMeter >= 0.7f && hunter.cards[i] is Card_SuperCharge superCard)
                {
                    superCard.weight = 90;
                }

                //behaviour-specific conditions
                //ninja
                if (hunter.cpuBehaviour.behaviourType == BehaviourType.Ninja && hunter.cards[i].fieldCardType == Card.FieldCardType.Trap)
                {
                    hunter.cards[i].weight = 90;
                }

                //opportunist
                if (hunter.cpuBehaviour.behaviourType == BehaviourType.Opportunist)
                {
                    if (hunter.HasTargetItem() && hunter.cards[i].fieldCardType == Card.FieldCardType.TrapAvoidance)
                    {
                        hunter.cards[i].weight = 90;
                    }

                    if (hunter.HasTargetItem() && hunter.cards[i].fieldCardType == Card.FieldCardType.Movement)
                    {
                        hunter.cards[i].weight = 90;
                    }
                }
                    

                topCards.Add(hunter.cards[i]);

            }

        }

        //look at the available cards and pick the most suitable one
        if (topCards.Count <= 0)
            return null; //no cards

        int totalWeight = 0;


        for(int i = 0; i < topCards.Count; i++)
        {
            totalWeight += topCards[i].weight;
        }

        
        int j = 0;
        bool cardFound = false;
        topCards = topCards.OrderByDescending(x => x.weight).ToList();  //sort by highest weight
        int randWeight = UnityEngine.Random.Range(0, totalWeight);
        while (!cardFound && j < topCards.Count)
        {
            if (randWeight <= topCards[j].weight)
            {
                cardFound = true;
                card = topCards[j];
                Debug.LogFormat("CPU chose card {0}", card.cardName);
            }
            else
            {
                randWeight -= topCards[j].weight;
                j++;
            }
        }

        return card;
    }


}
