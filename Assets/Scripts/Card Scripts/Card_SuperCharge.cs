using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//user gains 30% super meter. No combat effect
[CreateAssetMenu(menuName = "Cards/Super Charge", fileName = "card_superCharge", order = 3)]
public class Card_SuperCharge : Card
{
    void Reset()
    {
        cardName = "Super Charge";
        cardDetails_field = "+30% to super meter";
    }

    public override void ActivateCard_Field(Hunter user)
    {
        base.ActivateCard_Field(user);
    }

}
