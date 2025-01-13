using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Sends the user to the dungeon exit. If they don't have the target item, the user is sent to a random location. 
 In combat, grants 100% chance to run away. */
[CreateAssetMenu(menuName = "Cards/Exit!", fileName = "card_exit", order = 1)]
public class Card_Exit : Card
{
    void Reset()
    {
        cardName = "Exit!";
        cardDetails_field = "Teleport to the dungeon exit. If the user doesn't have the target item, they are sent to a random location.";
        cardDetails_combat = "100% chance to run away/prevent running away";
    }
    private void OnEnable()
    {
        defaultWeight = 40;
        weight = defaultWeight;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        //GameManager gm = Singleton.instance.GameManager;
        Dungeon dun = Singleton.instance.Dungeon;
        dun.UpdateCharacterRoom(user, dun.exitRoom);
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        Combat combat = Singleton.instance.Combat;
        if (user.isAttacker)
            combat.runPreventionMod += 1;
        else
            combat.runMod += 1;
    }
}
