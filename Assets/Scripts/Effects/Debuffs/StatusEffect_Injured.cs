using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Reduces max HP by 50%. Stacks 2 times. Inujred debuff is applied whenever hunter is defeated by another hunter.
 * If they're defeated by a monster, they're removed from the dungeon. */
[CreateAssetMenu(menuName = "Effects/Status Effects/Injured", fileName = "statEffect_Injured")]
public class StatusEffect_Injured : StatusEffect
{
    private int stackCount = 0;
    private float originalHealthPoints;
   void Reset()
    {
        effectName = "Injured";
        effectDetails = "Max HP reduced by 50%. Can stack 2 times.";
        effectType = EffectType.Debuff;
        hasDuration = false;
    }


    public override void ApplyEffect(Character user)
    {
        if (stackCount >= 2)
            return;

        stackCount++;
        Debug.LogFormat("Injured stack count on {0}: {1}", user.characterName, stackCount);
        if (stackCount <= 1)
            originalHealthPoints = user.maxHealthPoints;    //keep copy of hunter's original health points. 

        user.maxHealthPoints = Mathf.Round(user.maxHealthPoints * 0.5f);
        user.healthPoints = user.maxHealthPoints;
        HunterManager hm = Singleton.instance.HunterManager;
        hm.UpdateHunterHUD(user as Hunter);
    }

    //remove injured status
    public override void CleanupEffect(Character user)
    {
        Debug.LogFormat("Removing injured status from {0}", user.characterName);
        user.maxHealthPoints = originalHealthPoints;
        base.CleanupEffect(user);
    }
}
