using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Trap - Vise", fileName = "card_trapVise", order = 5)]
public class Card_TrapVise : Card
{
    void Reset()
    {
        cardName = "Trap - Vise";
        cardDetails_field = "Sets a trap that inflicts Disable Leg";
    }
    private void OnEnable()
    {
        defaultWeight = 70;
        weight = defaultWeight;
    }
    public override void ActivateCard_Field(Hunter user)
    {
        TrapManager tm = Singleton.instance.TrapManager;
        Trap trap = tm.GetTrap(Trap.TrapID.DisableLeg);
        user.room.trap = trap;
        tm.activeTrapList.Add(user.room);
        Debug.LogFormat("{0} has placed a trap '{1}'", user.characterName, trap.trapName);
    }

}
