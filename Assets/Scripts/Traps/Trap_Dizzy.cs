using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple trap that inflicts Dizzy.
[CreateAssetMenu(menuName = "Traps/Dizzy", fileName = "trap_Dizzy")]
public class Trap_Dizzy : Trap
{
    public override void ActivateTrap(Character target)
    {
        //Chance to inflict Dizzy = 100% - target's dizzy resistance
        if (Random.value <= 1 - target.resistDizzy)
        { 
            Debug.LogFormat("{0} triggered a dizzy trap and is now dizzy!", target.characterName);

            EffectManager em = Singleton.instance.EffectManager;
            em.AddEffect(StatusEffect.Effect.Dizzy, target);

            //display damage
            GameManager gm = Singleton.instance.GameManager;
            gm.DisplayStatusEffect(target, "DIZZY");

            //update HUD
            if (target is Hunter hunter)
            {
                HunterManager hm = Singleton.instance.HunterManager;
                hm.UpdateHunterHUD(hunter);
            }
        }

        //destroy itself
        TrapManager tm = Singleton.instance.TrapManager;
        tm.RemoveTrap(this);
       
    }
}
