using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Hunter becomes uncontrollable for 3 turns. ATP increased by 50%, but cannot defend or roll dice to reduce damage.
[CreateAssetMenu(menuName = "Effects/Status Effects/Berserk", fileName = "statEffect_Berserk")]
public class StatusEffect_Berserk : StatusEffect
{
   void Reset()
    {
        effectName = "Berserk";
        effectDetails = "Target is uncontrollable. Gain 50% ATP. Cannot defend or roll dice to reduce damage";
        effectType = EffectType.Debuff;
    }

    public override void ApplyEffect(Character user)
    {
        Debug.Log("Adding berserk status to " + user.characterName);
        user.atpMod += 0.5f;
        //make user cpu controlled for the duration of the debuff.
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
    }
}
