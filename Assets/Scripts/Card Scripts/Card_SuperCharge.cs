using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//user gains 30% super meter. No combat effect
[CreateAssetMenu(menuName = "Cards/Super Charge", fileName = "card_superCharge", order = 3)]
public class Card_SuperCharge : Card
{
    private const float superGain = 0.3f;
    void Reset()
    {
        cardName = "Super Charge";
        cardDetails_field = "+30% to super meter";
    }
    private void OnEnable()
    {
        defaultWeight = 50;
        weight = defaultWeight;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        HunterManager hm = Singleton.instance.HunterManager;
        hm.UpdateSuperMeter(user, superGain);
    }

}
