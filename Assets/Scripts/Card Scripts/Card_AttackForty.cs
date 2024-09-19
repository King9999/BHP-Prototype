using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Adds 40% ATP bonus during combat. No field effect
[CreateAssetMenu(menuName = "Cards/Attack +40", fileName = "card_attackForty", order = 0)]
public class Card_AttackForty : Card
{
    void Reset()
    {
        cardName = "Attack +40";
        cardDetails_combat = "+40% ATP";
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        base.ActivateCard_Combat(user);
    }
}
