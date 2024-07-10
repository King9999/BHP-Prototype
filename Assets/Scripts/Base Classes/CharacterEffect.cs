using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//buffs and debuffs that affect characters.
public abstract class CharacterEffect : ScriptableObject
{
    public string effectName, effectDetails;
    public int totalDuration, currentDuration;

    public enum EffectType
    {
        Buff, Debuff
    }

    public EffectType effectType;

    public virtual void UpdateEffect(Character user) 
    {
        currentDuration++;
        if (currentDuration >= totalDuration)
        {
            //remove this effect
            currentDuration = 0;
            if (effectType == EffectType.Buff) 
            {
                user.buffs.Remove(this);
            }
            else
            {
                user.debuffs.Remove(this);
            }
        }
    }
}
