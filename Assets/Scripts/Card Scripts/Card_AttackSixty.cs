using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Adds 60% ATP bonus during combat. No field effect
[CreateAssetMenu(menuName = "Cards/Attack +60", fileName = "card_attackSixty", order = 0)]
public class Card_AttackSixty : Card
{
    private const float atpMod = 0.6f;
    void Reset()
    {
        cardName = "Attack +60";
        cardDetails_combat = "+60% ATP";
    }
    private void OnEnable()
    {
        weight = 70;
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
