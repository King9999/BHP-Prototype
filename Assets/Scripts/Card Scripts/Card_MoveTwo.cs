using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/MOV +2", fileName = "card_move2")]
public class Card_MoveTwo : Card
{
    // Start is called before the first frame update
    void Reset()
    {
        cardName = "MOV +2";
        cardDetails_field = "Adds 2 spaces to total MOV";
        cardDetails_combat = "+20% chance to run away";
    }

    public override void ActivateCard_Field(Hunter user)
    {
        base.ActivateCard_Field(user);
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        base.ActivateCard_Combat(user);
    }
}
