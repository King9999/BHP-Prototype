using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ActiveSkill : Skill
{
    public int skillCost, skillCharges;  //cost = SP cost, skillCharges = fixed number of uses before skill can't be used anymore.
    public bool requiresCharges;        //if true, skill must have charges in order to be used.
    public float dmgEffectiveness;      //multiplier of how much ATP or MNP affects the damage of the skill. 1 = 100% damage, < 1 = reduced damage, > 1 = more damage.
    public int minRange, maxRange;      //skills can have their own range, separate from the equipped weapon.
                                        //
    public Dice dice;
}
