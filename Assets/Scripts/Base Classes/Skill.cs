using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* base class for all skills in the game. Includes active and passive skills. */
public abstract class Skill : ScriptableObject
{
    public int skillID;
    public string skillName, skillDetails;
    public int skillCooldown, currentCooldown;                            //skillCooldown = number of turns that must pass before skill can be used again.
    public bool skillInCooldown;
    public int skillEffectDuration;                 //how long a skill lasts on the user or a target.

    public virtual void ActivateSkill(Character user, Character target) 
    {
        if (skillInCooldown) return;
        
    }
    public virtual void ActivateSkill(Character user, List<Character> targets) 
    {
        if (skillInCooldown) return;
    }

    public void ApplyCooldown()
    {
        if (skillCooldown <= 0 || skillInCooldown) return;

        skillInCooldown = true;
        currentCooldown = 0;
    }

    public void EndCooldown()
    {
        if (!skillInCooldown) return;

        skillInCooldown = false;
        currentCooldown = 0;
    }    
}
