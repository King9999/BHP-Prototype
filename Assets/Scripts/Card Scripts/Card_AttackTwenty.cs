using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Adds 20% ATP bonus during combat. No field effect
[CreateAssetMenu(menuName = "Cards/Attack +20", fileName = "card_attackTwenty", order = 0)]
public class Card_AttackTwenty : Card
{
    private const float atpMod = 0.2f;
    void Reset()
    {
        cardName = "Attack +20";
        cardDetails_combat = "+20% ATP";
    }

    private void OnEnable()
    {
        defaultWeight = 50;
        weight = defaultWeight;
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        user.atpMod += atpMod;
    }

    public override void DeactivateCard_Combat(Hunter user)
    {
        user.atpMod -= atpMod;
        if (user.atpMod < 0)
        {
            user.atpMod = 0;
        }
    }
}
