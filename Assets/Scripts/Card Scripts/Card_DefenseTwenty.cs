using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Defense +20", fileName = "card_defense20", order = 4)]
public class Card_DefenseTwenty : Card
{
    private const float dfpMod = 0.2f;
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
        user.evdMod += dfpMod;
    }

    public override void DeactivateCard_Field(Hunter user)
    {
        user.evdMod -= dfpMod;
        if (user.evdMod < 0)
            user.evdMod = 0;
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        user.dfpMod += dfpMod;
    }

    public override void DeactivateCard_Combat(Hunter user)
    {
        user.dfpMod -= dfpMod;
        if (user.dfpMod < 0)
            user.dfpMod = 0;
    }
}
