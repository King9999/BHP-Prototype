using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Trap - Dizzy", fileName = "card_trapDizzy", order = 5)]
public class Card_TrapDizzy : Card
{
    void Reset()
    {
        cardName = "Trap - Dizzy";
        cardDetails_field = "Sets a trap that inflicts Dizzy";
    }

    public override void ActivateCard_Field(Hunter user)
    {
        base.ActivateCard_Field(user);
    }

}
