using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Every character has this skill. It costs nothing to use, and its effects change with the equipped weapon. */
[CreateAssetMenu(menuName = "Skills/Active Skills/Basic Attack", fileName = "activeSkill_BasicAttack")]
public class ActiveSkill_BasicAttack : ActiveSkill
{
    void Reset()
    {
        skillName = "Attack";
        skillID = "activeSkill_BasicAttack";
        skillDetails = "Deal ATP/MNP damage. Damage, range and effects are based on equipped weapon.";
        skillCost = 0;
        dmgMod = 1;
        minRange = 1;
        maxRange = 1;
        usageType = UsageType.Battle;
        weaponRestriction = WeaponRestriction.None;
    }

    public override void ActivateSkill(Character user, Character target)
    {
        base.ActivateSkill(user, target);

        //roll dice and add result to total damage
        int diceRoll = dice.RollDice(); 
        int singleDieRoll = dice.RollSingleDie();
        float totalDamage = Mathf.Round(user.atp + diceRoll) * dmgMod - (target.dfp + singleDieRoll);
    }
}
