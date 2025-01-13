using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/MOV +3", fileName = "card_move3", order = 2)]
public class Card_MoveThree : Card
{
    // Start is called before the first frame update
    void Reset()
    {
        cardName = "MOV +3";
        cardDetails_field = "Adds 3 spaces to total MOV";
        cardDetails_combat = "+30% chance to run away/prevent running away";
    }
    private void OnEnable()
    {
        defaultWeight = 70;
        weight = defaultWeight;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        GameManager gm = Singleton.instance.GameManager;
        gm.movementMod += 3;
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        Combat combat = Singleton.instance.Combat;
        if (user.isAttacker)
            combat.runPreventionMod += 0.3f;
        else
            combat.runMod += 0.3f;
    }
}
