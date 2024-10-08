using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Defense +60", fileName = "card_defense60", order = 4)]
public class Card_DefenseSixty : Card
{
    void Reset()
    {
        cardName = "Defense +60";
        cardDetails_field = "+60% chance to avoid traps";
        cardDetails_combat = "+60% DFP";
    }
    private void OnEnable()
    {
        weight = 70;
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
