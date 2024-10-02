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

    /*private void OnDestroy()
    {
        GameManager gm = Singleton.instance.GameManager;
        CleanupEffect(gm.ActiveCharacter());
    }*/

    public override void ApplyEffect(Character user)
    {
        if (stackCount >= 2)
            return;

        stackCount++;
        Debug.Log("Injured stack count on " + user.characterName + ": " + stackCount);
        if (stackCount <= 1)
            originalHealthPoints = user.maxHealthPoints;    //keep copy of hunter's original health points. 

        //user.maxHealthPoints *= 0.5f;
        user.maxHealthPoints = Mathf.Round(user.maxHealthPoints * 0.5f);
        user.healthPoints = user.maxHealthPoints;
        //if (!user.debuffs.Contains(this))
            //user.debuffs.Add(this);
    }
    //remove injured status
    public override void CleanupEffect(Character user)
    {
        Debug.Log("Removing injured status from " + user.characterName);
        user.maxHealthPoints = originalHealthPoints;
        base.CleanupEffect(user);
        //Destroy(this);
        //user.debuffs.Remove(this);
    }
}
