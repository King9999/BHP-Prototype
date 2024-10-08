using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Defense +20", fileName = "card_defense20", order = 4)]
public class Card_DefenseTwenty : Card
{
    void Reset()
    {
        cardName = "Defense +20";
        cardDetails_field = "+20% chance to avoid traps";
        cardDetails_combat = "+20% DFP";
    }
    private void OnEnable()
    {
        weight = 50;
    }

    public override void ActivateCard_Field(Hunter user)
    {
        base.ActivateCard_Field(user);
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        base.ActivateCard_Combat(user);
    }
}
