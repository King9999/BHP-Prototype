using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* In the field, adds 1 to total movement. In combat, +10% chance to run away from combat. */
[CreateAssetMenu(menuName = "Cards/MOV +1", fileName = "card_move1", order = 2)]
public class Card_MoveOne : Card
{ 
    // Start is called before the first frame update
    void Reset()
    {
        cardName = "MOV +1";
        cardDetails_field = "Adds 1 space to total MOV";
        cardDetails_combat = "+10% chance to run away/prevent running away";
    }
    private void OnEnable()
    {
        weight = 50;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        GameManager gm = Singleton.instance.GameManager;
        gm.movementMod += 1;
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        Combat combat = Singleton.instance.Combat;
        if (user.isAttacker)
            combat.runPreventionMod += 0.1f;
        else
            combat.runMod += 0.1f;
    }
}
