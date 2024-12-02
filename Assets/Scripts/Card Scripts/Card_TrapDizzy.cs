using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Trap - Dizzy", fileName = "card_trapDizzy", order = 5)]
public class Card_TrapDizzy : Card
{
    void Reset()
    {
        cardName = "Trap - Dizzy";
        cardDetails_field = "Sets a trap that inflicts Dizzy";
    }
    private void OnEnable()
    {
        weight = 50;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        TrapManager tm = Singleton.instance.TrapManager;
        Trap trap = tm.GetTrap(Trap.TrapID.Dizzy);
        user.room.trap = trap;
        tm.activeTrapList.Add(user.room);
        Debug.LogFormat("{0} has placed a trap '{1}'", user.characterName, trap.trapName);
    }

}
