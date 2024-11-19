using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/MOV +2", fileName = "card_move2", order = 2)]
public class Card_MoveTwo : Card
{
    // Start is called before the first frame update
    void Reset()
    {
        cardName = "MOV +2";
        cardDetails_field = "Adds 2 spaces to total MOV";
        cardDetails_combat = "+20% chance to run away/prevent running away";
    }
    private void OnEnable()
    {
        weight = 60;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        GameManager gm = Singleton.instance.GameManager;
        gm.movementMod += 2;
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        Combat combat = Singleton.instance.Combat;
        if (user.isAttacker)
            combat.runPreventionMod += 0.2f;
        else
            combat.runMod += 0.2f;
    }
}
