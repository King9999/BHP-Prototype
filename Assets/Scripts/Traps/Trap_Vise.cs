using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple trap that inflicts Disable Leg.
[CreateAssetMenu(menuName = "Traps/Vise", fileName = "trap_Vise")]
public class Trap_Vise : Trap
{
    public override void ActivateTrap(Character target)
    {
       
        Debug.LogFormat("{0} triggered a Vise trap! MOV reduced!", target.characterName);

        EffectManager em = Singleton.instance.EffectManager;
        em.AddEffect(StatusEffect.Effect.DisableLeg, target);

        //display message
        GameManager gm = Singleton.instance.GameManager;
        gm.DisplayStatusEffect(target, "DISABLE LEG");

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
