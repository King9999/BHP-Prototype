using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple trap that deals 20% HP damage to target.
[CreateAssetMenu(menuName = "Traps/Damage", fileName = "trap_Damage")]
public class Trap_Damage : Trap
{
    private float damageAmount;
    public override void ActivateTrap(Character target)
    {
        damageAmount = Mathf.Round(target.maxHealthPoints * 0.2f);
        target.healthPoints -= damageAmount;
        Debug.LogFormat("{0} triggered a damage trap and has taken {1} damage!", target.characterName, damageAmount);

        //update HUD
        if (target is Hunter hunter)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            hm.UpdateHunterHUD(hunter);
        }
    }
}
