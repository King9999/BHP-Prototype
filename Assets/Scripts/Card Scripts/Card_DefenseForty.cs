using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Defense +40", fileName = "card_defense40", order = 4)]
public class Card_DefenseForty : Card
{
    void Reset()
    {
        cardName = "Defense +40";
        cardDetails_field = "+40% chance to avoid traps";
        cardDetails_combat = "+40% DFP";
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
