using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//not much to this. As long as this object is in debuffs, player can do nothing.
[CreateAssetMenu(menuName = "Effects/Status Effects/Disable Leg", fileName = "statEffect_DisableLeg")]
public class StatusEffect_DisableLeg : StatusEffect
{
   void Reset()
   {
        effectName = "Disable Leg";
        effectDetails = "Total MOV is reduced by half.";
        effectType = EffectType.Debuff;
        effect = Effect.DisableLeg;
        hasDuration = false;
   }

    public override void ApplyEffect(Character user)
    {
        Debug.LogFormat("{0}'s MOV is reduced!", user.characterName);
        GameManager gm = Singleton.instance.GameManager;
        user.movMod -= 0.5f;
        gm.ForceStop = true;
    }

    //remove disabled leg
    public override void CleanupEffect(Character user)
    {
        Debug.LogFormat("Removing 'Disable Leg' debuff from {0}", user.characterName);
        user.movMod += 0.5f;
        base.CleanupEffect(user);   
    }
}
