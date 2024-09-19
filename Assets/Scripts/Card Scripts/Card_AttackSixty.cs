using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Adds 60% ATP bonus during combat. No field effect
[CreateAssetMenu(menuName = "Cards/Attack +60", fileName = "card_attackSixty", order = 0)]
public class Card_AttackSixty : Card
{
    void Reset()
    {
        cardName = "Attack +60";
        cardDetails_combat = "+60% ATP";
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        base.ActivateCard_Combat(user);
    }
}
