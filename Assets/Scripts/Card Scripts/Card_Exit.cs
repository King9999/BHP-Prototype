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
        cardDetails_combat = "100% chance to run away";
    }
    private void OnEnable()
    {
        weight = 80;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        GameManager gm = Singleton.instance.GameManager;
        //Add code to locate the exit and send hunter there.
        //check inventory for target item. If it's not there, then send hunter to a random location.
    }

    public override void ActivateCard_Combat(Hunter user)
    {
        GameManager gm = Singleton.instance.GameManager;
        gm.combatManager.chanceToRun = 1;
    }
}
