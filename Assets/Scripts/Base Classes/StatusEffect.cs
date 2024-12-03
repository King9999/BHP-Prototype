using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//buffs and debuffs that affect characters.
public abstract class StatusEffect : ScriptableObject
{
    public string effectName, effectDetails;
    public int totalDuration, currentDuration;
    public bool hasDuration;                //if false, effect is permanent.
    public Sprite effectIcon;
    //public TextMeshPro durationText;

    public enum Effect { Dizzy, Berserk, Injured, Blind, CardDrain, Poison, DisableLeg, DisableSkill, DisableSuper,
    Weakened }


    public Effect effect;
    public enum EffectType
    {
        Buff, Debuff
    }

    public EffectType effectType;

    public virtual void UpdateEffect(Character user) 
    {
        if (!hasDuration)
            return;

        currentDuration++;
        if (currentDuration >= totalDuration)
        {
            //remove this effect
            //currentDuration = 0;
            CleanupEffect(user);
        }
    }

    public virtual void ApplyEffect(Character user)
    {

    }

    //removes effect
    public virtual void CleanupEffect(Character user)
    {
        if (effectType == EffectType.Buff)
        {
            user.buffs.Remove(this);
        }
        else
        {
            user.debuffs.Remove(this);
        }
        
        Destroy(this);
    }
}
