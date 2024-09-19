using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/MOV +1", fileName = "card_move1")]
public class Card_MoveOne : Card
{
    // Start is called before the first frame update
    void Reset()
    {
        cardName = "MOV +1";
        cardDetails_field = "Adds 1 space to total MOV";
        cardDetails_combat = "+10% chance to run away";
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
