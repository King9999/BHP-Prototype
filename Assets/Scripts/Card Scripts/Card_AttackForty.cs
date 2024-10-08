using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Adds 40% ATP bonus during combat. No field effect
[CreateAssetMenu(menuName = "Cards/Attack +40", fileName = "card_attackForty", order = 0)]
public class Card_AttackForty : Card
{
    private const float atpMod = 0.4f;
    void Reset()
    {
        cardName = "Attack +40";
        cardDetails_combat = "+40% ATP";
    }

    private void OnEnable()
    {
        weight = 60;
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
