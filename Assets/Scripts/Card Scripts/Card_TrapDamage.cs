using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Trap - Damage", fileName = "card_trapDamage", order = 5)]
public class Card_TrapDamage : Card
{
    void Reset()
    {
        cardName = "Trap - Damage";
        cardDetails_field = "Sets a trap that deals 20% HP damage when triggered";
    }
    private void OnEnable()
    {
        defaultWeight = 50;
        weight = defaultWeight;
    }

    //place a trap on the space the hunter is on before moving.
    public override void ActivateCard_Field(Hunter user)
    {
        TrapManager tm = Singleton.instance.TrapManager;
        Trap trap = tm.GetTrap(Trap.TrapID.Damage);
        user.room.trap = trap;
        tm.activeTrapList.Add(user.room);
        Debug.LogFormat("{0} has placed a trap '{1}'", user.characterName, trap.trapName);
    }

}
