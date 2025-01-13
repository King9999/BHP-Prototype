using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//card that ignores defense rating. Does not work against perfect defense.
[CreateAssetMenu(menuName = "Cards/Pierce", fileName = "card_pierce", order = 0)]
public class Card_Pierce : Card
{
    void Reset()
    {
        cardName = "Pierce";
        cardDetails_combat = "Ignores target's total defense. No effect vs. Perfect Defense";
    }

    private void OnEnable()
    {
        defaultWeight = 80;
        weight = defaultWeight;
    }
    public override void ActivateCard_Combat(Hunter user)
    {
        base.ActivateCard_Combat(user);
    }
}
