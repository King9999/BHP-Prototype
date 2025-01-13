using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Defense +40", fileName = "card_defense40", order = 4)]
public class Card_DefenseForty : Card
{
    private const float dfpMod = 0.4f;
    void Reset()
    {
        cardName = "Defense +40";
        cardDetails_field = "+40% chance to avoid traps";
        cardDetails_combat = "+40% DFP";
    }
    private void OnEnable()
    {
        defaultWeight = 60;
        weight = defaultWeight;
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
