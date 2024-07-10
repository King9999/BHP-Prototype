using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* passive skills are activated when requirements are met. Powerful passives may have a cooldown to prevent them from being too strong. */
public abstract class PassiveSkill : Skill
{
    public bool triggerOnHit;           //activates when user hits a target
    public bool triggerWhenUserHit;     //activates when user takes damage
    public bool triggerOnEndOfUserTurn;     //activates when user's turn ends
    
}
