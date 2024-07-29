using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Deals 2d6 * 3 damage to the enemy. User takes 50% the damage dealt. If both the user and enemy die, the user dies first, and
 * the enemy is left with 1 HP. This is to prevent abusing the skill. */
[CreateAssetMenu(menuName = "Skills/Active Skills/Risky Tackle", fileName = "activeSkill_RiskyTackle")]
public class ActiveSkill_RiskyTackle : ActiveSkill
{
    void Reset()
    {
        skillName = "Risky Tackle";
        skillID = "ActiveSkill_RiskyTackle";
        skillDetails = "Deal (2d6 + ATP) * 3 damage. User takes half the damage dealt.";
        skillCost = 5;
        //skillCooldown = 0;
        //skillEffectDuration = 3;
        dmgEffectiveness = 3;
        minRange = 1;
        maxRange = 1;
        usageType = UsageType.Battle;
        weaponRestriction = WeaponRestriction.Melee;
    }

    public override void ActivateSkill(Character user, Character target)
    {
        base.ActivateSkill(user, target);

        //roll dice and add result to total damage
        int diceRoll = dice.RollDice(); 
        int singleDieRoll = dice.RollSingleDie();
        float totalDamage = Mathf.Round(user.atp + diceRoll) * dmgEffectiveness - (target.dfp + singleDieRoll);
    }
}
