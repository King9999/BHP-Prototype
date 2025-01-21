using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Combat;

/*AGGRO BEHAVIOUR
-----
* Always attacks hunters and monsters, whichever is closer
* Ignores chests and terminals
* At the start of each turn, there's a 30% chance that the hunter gains Berserk debuff.
*/

[CreateAssetMenu(menuName = "AI Behaviour/Hunter/Aggro", fileName = "ai_aggro")]
public class Hunter_AI_Aggro : Hunter_AI
{
    // Start is called before the first frame update
    void Reset()
    {
        behaviourType = BehaviourType.Aggro;        //internal info only. It tells me what kind of behaviour the hunter has.
        canAttackHunters = true;
        canAttackMonsters = true;          //also includes bosses
        canOpenChests = false;
        canUseTerminals = false;
        rollStr = 85;
        rollVit = 5;
        rollSpd = 10;
        rollMnt = 0;
    }

    //Hunter has a chance to gain berserk debuff
    public override void ActivateAbility(Hunter hunter)
    {
        if (Random.value <= 0.3f)
        {
                EffectManager em = Singleton.instance.EffectManager;
                em.AddEffect(StatusEffect.Effect.Berserk, hunter);
                Debug.LogFormat("Berserk triggered for {0}", hunter.characterName);
                //add berserk
                //StatusEffect_Berserk berserk = new();     //want to check if this code works
                //if (hunter.debuffs.Contains(berserk))
                /*bool berserkFound = false;
            int i = 0;
            while (!berserkFound && i < hunter.debuffs.Count)
            {
                if (hunter.debuffs[i].effect == StatusEffect.Effect.Berserk)
                    berserkFound = true;
                else
                    i++;
            }

            if (!berserkFound)
            {
                EffectManager em = Singleton.instance.EffectManager;
                em.AddEffect(StatusEffect.Effect.Berserk, hunter);
                Debug.LogFormat("Berserk triggered for {0}", hunter.characterName);
            }
            else
            {
                Debug.LogFormat(" Berserk already active for {0}", hunter.characterName);
            }*/

        }
    }

    public override Card ChooseCard_Combat(Hunter hunter)
    {
        if (hunter.cards.Count <= 0)
            return null;

        Card card = null;
        List<Card> topCards = new List<Card>();

        //Aggro hunters only want attack cards.
        switch (hunter.characterState)
        {
            case Character.CharacterState.Attacking:
                //look for attack and defense cards, attack cards are higher priority
                for (int i = 0; i < hunter.cards.Count; i++)
                {
                    Card currentCard = hunter.cards[i];
                    if (currentCard.cardType == Card.CardType.Combat)
                    {
                        if (currentCard.cardID == Card.CardID.Attack20 || currentCard.cardID == Card.CardID.Attack40 ||
                            currentCard.cardID == Card.CardID.Attack60 || currentCard.cardID == Card.CardID.Pierce)
                        {
                            topCards.Add(currentCard);
                        }

                    }
                }
                break;

        }

        //look at the available cards and pick the most suitable one
        if (topCards.Count <= 0)
            return null; //no cards

        int totalWeight = 0;

        for (int i = 0; i < topCards.Count; i++)
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

    public override void MakeDefenderChoice(Hunter hunter)
    {
        /* Aggro hunters never run. They attack until someone dies. */
        Combat combat = Singleton.instance.Combat;
        hunter.ChangeCharacterState(hunter.characterState = Character.CharacterState.Attacking);
        combat.defenderAction = DefenderAction.CounterAttack;
    }

}
