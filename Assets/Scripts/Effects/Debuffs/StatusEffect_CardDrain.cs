using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Removes all cards in hunter's possession and prevents them from receiving cards for 1 turn.
//Monsters can be inflicted with this debuff, but it does nothing (The boss that has their own deck is the exception).
//Note that the card is still drawn from the deck, but is discarded so nobody gets the card.
[CreateAssetMenu(menuName = "Effects/Status Effects/Card Drain", fileName = "statEffect_CardDrain")]
public class StatusEffect_CardDrain : StatusEffect
{
   void Reset()
   {
        effectName = "Card Drain";
        effectDetails = "All cards in hand are discarded. Cannot receive cards for 1 turn";
        effectType = EffectType.Debuff;
        effect = Effect.CardDrain;
        hasDuration = true;
        totalDuration = 2;      //must be 2 turns, otherwise the debuff is removed before the draw phase occurs. Technically it's 1 turn.
   }

    public override void ApplyEffect(Character user)
    {
        Debug.LogFormat("{0}'s cards discarded!", user.characterName);
        if (user is Hunter hunter)
        {
            for (int i = 0; i < hunter.cards.Count; i++)
            {
                Destroy(hunter.cards[i]);
            }
            hunter.cards.Clear();

            //cannot draw card for 1 turn.
            hunter.canDrawCard = false;
        }

        //TODO: Add special condition for boss who has their own deck
    }

    //remove disabled leg
    public override void CleanupEffect(Character user)
    {
        Debug.LogFormat("Removing 'Card Drain' debuff from {0}", user.characterName);
        if (user is Hunter hunter)
        {
            hunter.canDrawCard = true;
        }
        base.CleanupEffect(user);   
    }
}
