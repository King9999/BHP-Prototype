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

        //display damage
        GameManager gm = Singleton.instance.GameManager;
        gm.DisplayDamage(target, damageAmount);

        //update HUD
        if (target is Hunter hunter)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            hm.UpdateHunterHUD(hunter);
        }

        //destroy itself
        TrapManager tm = Singleton.instance.TrapManager;

        //find trap in active trap list and remove it
        tm.RemoveTrap(this);
        /*int i = 0;
        bool trapFound = false;
        while (!trapFound && i < tm.activeTrapList.Count)
        {
            if (tm.activeTrapList[i].trap == this)
            {
                trapFound = true;
                Debug.LogFormat("Removed trap {0} from room {1}", trapName, tm.activeTrapList[i].roomID);
                tm.activeTrapList[i].trap = null;
                tm.activeTrapList.Remove(tm.activeTrapList[i]);
            }
            else
            {
                i++;
            }
        }*/
        
        //Object.Destroy(this);
    }
}
