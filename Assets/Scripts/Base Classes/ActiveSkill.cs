using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public abstract class ActiveSkill : Skill
{
    public int skillCost, skillCharges, maxCharges;  //cost = SP cost, skillCharges = fixed number of uses before skill can't be used anymore.
    public bool requiresCharges;        //if true, skill must have charges in order to be used.
    public float dmgMod;      //multiplier of how much ATP or MNP affects the damage of the skill. 1 = 100% damage, < 1 = reduced damage, > 1 = more damage.
    public int minRange, maxRange;      //skills can have their own range, separate from the equipped weapon.
                                        //
    //public Dice dice;
    public enum SkillAttribute { PhysDamage, PsychDamage, Restorative, Buff, Debuff }
    public SkillAttribute attribute;
    public enum MonsterSkillRange { Melee, Ranged }            //used by monsters only since they don't equip weapons.
    public MonsterSkillRange monsterSkillRange;

    //moves/animates the character when skill is activated.
    public virtual IEnumerator Animate(Character character) { yield return null; }

    //this is used when the character moves towards its target
    public virtual IEnumerator Animate(Character character, Character target) { yield return null; }
    public virtual float CalculateDamage(Character attacker, Character defender, int attackDiceRoll, int defenderDiceRoll) 
    {
        float totalDamage = 0;
        float totalAttack_Atp = attacker.atp * attacker.atpMod * 0.8f + (attackDiceRoll * 0.1f);
        float totalAttack_Mnp = attacker.mnp * attacker.mnpMod * 0.8f + (attackDiceRoll * 0.1f);
        float totalDefense_Dfp = defender.dfp * defender.dfpMod * 0.9f + (defenderDiceRoll * 0.1f); //0.9 is used instead of 0.8 because if a
        float totalDefense_Rst = defender.rst * defender.rstMod * 0.9f + (defenderDiceRoll * 0.1f); //is rolled, only 90% of DFP is used.
        if (attribute == SkillAttribute.PhysDamage)
        {
            Debug.LogFormat("Total Attack (ATP): {0}, Total Defense (DFP): {1}", totalAttack_Atp, totalDefense_Dfp);
            totalDamage = totalAttack_Atp - totalDefense_Dfp;
        }
        else if (attribute == SkillAttribute.PsychDamage)
        {
            Debug.LogFormat("Total Attack (MNP): {0}, Total Defense (RST): {1}", totalAttack_Mnp, totalDefense_Rst);
            totalDamage = totalAttack_Mnp - totalDefense_Rst;
        }
        return totalDamage; 
    }

    //used for skills that don't deal damage, or have additional effects after dealing damage.
    public virtual void ApplyEffect(Character user, Character target) { }
    public virtual float CalculateFixedDamage(Character attacker, Character defender) { return 0; }
}
