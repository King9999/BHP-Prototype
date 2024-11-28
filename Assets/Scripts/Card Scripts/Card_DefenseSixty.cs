using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Defense +60", fileName = "card_defense60", order = 4)]
public class Card_DefenseSixty : Card
{
    private const float dfpMod = 0.6f;
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
