using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Trap - Damage", fileName = "card_trapDamage", order = 5)]
public class Card_TrapDamage : Card
{
    void Reset()
    {
        cardName = "Trap - Damage";
        cardDetails_field = "Sets a trap that deals 20% HP damage when triggered";
    }
    private void OnEnable()
    {
        weight = 50;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        base.ActivateCard_Field(user);
    }

}
