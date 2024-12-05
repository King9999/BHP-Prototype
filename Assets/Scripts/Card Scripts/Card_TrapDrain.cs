using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Trap - Drain", fileName = "card_trapDrain", order = 5)]
public class Card_TrapDrain : Card
{
    void Reset()
    {
        cardName = "Trap - Drain";
        cardDetails_field = "Sets a trap that inflicts Card Drain";
    }
    private void OnEnable()
    {
        weight = 60;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        TrapManager tm = Singleton.instance.TrapManager;
        Trap trap = tm.GetTrap(Trap.TrapID.CardDrain);
        user.room.trap = trap;
        tm.activeTrapList.Add(user.room);
        Debug.LogFormat("{0} has placed a trap '{1}'", user.characterName, trap.trapName);
    }

}
