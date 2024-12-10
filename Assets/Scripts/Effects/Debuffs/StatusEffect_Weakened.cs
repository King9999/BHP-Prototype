using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Roll 1 die instead of 2 when using any skill.
[CreateAssetMenu(menuName = "Effects/Status Effects/Weakened", fileName = "statEffect_Weakened")]
public class StatusEffect_Weakened : StatusEffect
{
   void Reset()
   {
        effectName = "Weakened";
        effectDetails = "Roll 1 die when using a skill";
        effectType = EffectType.Debuff;
        effect = Effect.Weakened;
        hasDuration = true;
        totalDuration = 3;
   }

    //Not much to this. The game simply checks if this debuff is on the character.
    public override void ApplyEffect(Character user)
    {
        Debug.LogFormat("{0}'s been weakened!", user.characterName);
    }

    public override void CleanupEffect(Character user)
    {
        Debug.LogFormat("Removing 'Weakened' debuff from {0}", user.characterName);
        base.CleanupEffect(user);   
    }
}
