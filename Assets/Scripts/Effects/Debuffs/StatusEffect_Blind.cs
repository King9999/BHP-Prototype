using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//All attacks have a 30% chance to hit. Target cannot avoid traps.
[CreateAssetMenu(menuName = "Effects/Status Effects/Blind", fileName = "statEffect_Blind")]
public class StatusEffect_Blind : StatusEffect
{
    private const float hitChance = 0.3f;
    private const float evdMod = 1000;   //ensures that mod is always 0 in case other effects boost evasion.
   void Reset()
   {
        effectName = "Blind";
        effectDetails = "Attacks have a 30% chance to hit";
        effectType = EffectType.Debuff;
        hasDuration = true;
        totalDuration = 3;
   }

    public override void ApplyEffect(Character user)
    {
        Debug.LogFormat("{0} is blinded!", user.characterName);
        user.evdMod -= evdMod;
    }

    public bool AttackSuccessful()
    {
        float roll = Random.value;
        Debug.LogFormat("Blind in effect, rolled {0} hit chance", roll);
        if (roll <= hitChance)
            return true;
        else 
            return false;
    }


    //remove blind status
    public override void CleanupEffect(Character user)
    {
        Debug.LogFormat("Removing blind status from {0}", user.characterName);
        user.evdMod += evdMod;
        base.CleanupEffect(user);   
    }
}
