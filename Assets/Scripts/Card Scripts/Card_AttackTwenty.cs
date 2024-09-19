using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Adds 20% ATP bonus during combat. No field effect
[CreateAssetMenu(menuName = "Cards/Attack +20", fileName = "card_attackTwenty", order = 0)]
public class Card_AttackTwenty : Card
{
    void Reset()
    {
        cardName = "Attack +20";
        cardDetails_combat = "+20% ATP";
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        base.ActivateCard_Combat(user);
    }
}
