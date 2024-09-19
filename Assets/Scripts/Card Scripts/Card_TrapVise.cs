using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Trap - Vise", fileName = "card_trapVise", order = 5)]
public class Card_TrapVise : Card
{
    void Reset()
    {
        cardName = "Trap - Vise";
        cardDetails_field = "Sets a trap that inflicts Disable Leg";
    }

    public override void ActivateCard_Field(Hunter user)
    {
        base.ActivateCard_Field(user);
    }

}
