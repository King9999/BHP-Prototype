using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Manages buffs and debuffs, and terminal effects. */
public class EffectManager : MonoBehaviour
{
    public List<StatusEffect> statusEffects;    //contains both buffs and debuffs

    public static EffectManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }


    //add a given effect to a character.
    public void AddEffect(StatusEffect.Effect effect, Character character)
    {
        //find the status effect
        bool effectFound = false;
        int i = 0;
        StatusEffect statusEffect = null;
        while (!effectFound && i < statusEffects.Count)
        {
            if (statusEffects[i].effect == effect)
            {
                effectFound = true;
                statusEffect = Instantiate(statusEffects[i]);
            }
            else
            {
                i++;
            }
        }

        if (statusEffect.effectType == StatusEffect.EffectType.Buff)
        {
            character.buffs.Add(statusEffect);
        }
        else
        {
            character.debuffs.Add(statusEffect);
        }
        statusEffect.ApplyEffect(character);
    }
   
}
