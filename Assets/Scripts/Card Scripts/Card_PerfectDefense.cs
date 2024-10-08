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
        weight = 80;
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
