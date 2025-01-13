using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//negates all damage and debuff attempts during combat. In the field, avoid all traps.
[CreateAssetMenu(menuName = "Cards/Perfect Defense", fileName = "card_PerfectDef", order = 4)]
public class Card_PerfectDefense : Card
{
    void Reset()
    {
        cardName = "Perfect Defense";
        cardDetails_field = "100% chance to avoid traps";
        cardDetails_combat = "Reduce all damage against user to 0. Debuff chance against user is 0%";
    }
    private void OnEnable()
    {
        defaultWeight = 80;
        weight = defaultWeight;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        user.evdMod += 1;
    }

    public override void DeactivateCard_Field(Hunter user)
    {
        user.evdMod -= 1;
        if (user.evdMod < 0)
            user.evdMod = 0;
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        Combat combat = Singleton.instance.Combat;
        combat.perfectDefenseMod = 0;
    }
}
