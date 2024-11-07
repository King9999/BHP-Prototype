using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ActiveSkill : Skill
{
    public int skillCost, skillCharges, maxCharges;  //cost = SP cost, skillCharges = fixed number of uses before skill can't be used anymore.
    public bool requiresCharges;        //if true, skill must have charges in order to be used.
    public float dmgMod;      //multiplier of how much ATP or MNP affects the damage of the skill. 1 = 100% damage, < 1 = reduced damage, > 1 = more damage.
    public int minRange, maxRange;      //skills can have their own range, separate from the equipped weapon.
                                        //
    public Dice dice;
    public enum SkillAttribute { Damage, Restorative, Buff, Debuff }
    public SkillAttribute attribute;

    //moves/animates the character when skill is activated.
    public virtual IEnumerator Animate(Character character) { yield return null; }

    //this is used when the character moves towards its target
    public virtual IEnumerator Animate(Character character, Vector3 destination) { yield return null; }
}
