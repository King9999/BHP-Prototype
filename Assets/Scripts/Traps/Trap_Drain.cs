using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple trap that inflicts Card Drain.
[CreateAssetMenu(menuName = "Traps/Drain", fileName = "trap_Drain")]
public class Trap_Drain : Trap
{
    public override void ActivateTrap(Character target)
    {
       
        Debug.LogFormat("{0} triggered a Drain trap! Call cards gone", target.characterName);

        EffectManager em = Singleton.instance.EffectManager;
        em.AddEffect(StatusEffect.Effect.CardDrain, target);

        //display message
        //GameManager gm = Singleton.instance.GameManager;
        //gm.DisplayStatusEffect(target, "CARD DRAIN");

        //update HUD
        if (target is Hunter hunter)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            hm.UpdateHunterHUD(hunter);
        }
        
        //destroy itself
        TrapManager tm = Singleton.instance.TrapManager;
        tm.RemoveTrap(this);
       
    }
}
