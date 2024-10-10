using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Hunter becomes uncontrollable for 3 turns. If Hunter is controlled by human,
 * CPU controls the hunter for duration of debuff. ATP increased by 50%, but cannot defend or roll dice to reduce damage. */
[CreateAssetMenu(menuName = "Effects/Status Effects/Berserk", fileName = "statEffect_Berserk")]
public class StatusEffect_Berserk : StatusEffect
{
    private Hunter_AI aiCopy;       //copy of behaviour before it was changed.
    private Monster_AI monsterAICopy;
    private bool cpuControlState;
   void Reset()
    {
        effectName = "Berserk";
        effectDetails = "Target is uncontrollable. Gain 50% ATP. Cannot defend or roll dice to reduce damage";
        effectType = EffectType.Debuff;
        hasDuration = true;
    }

    public override void ApplyEffect(Character user)
    {
        Debug.Log("Adding berserk status to " + user.characterName);
        user.atpMod += 0.5f;

        //make user cpu controlled for the duration of the debuff.
        cpuControlState = user.cpuControlled;
        user.cpuControlled = true;
        
        if (user is Hunter hunter)
        {
            aiCopy = hunter.cpuBehaviour;
            HunterManager hm = Singleton.instance.HunterManager;
            hunter.cpuBehaviour = hm.GetHunterAI(Hunter_AI.BehaviourType.Aggro);
        }
        else if (user is Monster monster)
        {
            monsterAICopy = monster.cpuBehaviour;
            //TODO: overwrite the character CPU behaviour with monster version of Aggro.
        }
    }


    //remove berserk status
    public override void CleanupEffect(Character user)
    {
        Debug.Log("Removing berserk status from " + user.characterName);
        user.atpMod -= 0.5f;
        if (user.atpMod < 0)
        {
            user.atpMod = 0;
        }

        //revert AI behaviour
        user.cpuControlled = cpuControlState;
        if (user is Hunter hunter)
        {
            hunter.cpuBehaviour = aiCopy;
        }
        else if (user is Monster monster)
        {
            monster.cpuBehaviour = monsterAICopy;
        }

        base.CleanupEffect(user);   //destroys this object.
    }
}
