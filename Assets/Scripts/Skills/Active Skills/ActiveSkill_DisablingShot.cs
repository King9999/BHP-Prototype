using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Active Skills/Disabling Shot", fileName = "activeSkill_DisablingShot")]
public class ActiveSkill_DisablingShot : ActiveSkill
{
    void Reset()
    {
        skillName = "Disabling Shot";
        skillID = "activeSkill_DisablingShot";
        skillDetails = "Inflicts Disable Leg if damage dealt successfully";
        skillCost = 6;
        skillCooldown = 4;
        skillEffectDuration = 3;
        dmgMod = 0.33f;
        minRange = 2;
        maxRange = 4;
        usageType = UsageType.Field;
        weaponRestriction = WeaponRestriction.Gun;
    }

    /*public override void ActivateSkill(Character user, Character target)
    {
        base.ActivateSkill(user, target);

        //roll dice and add result to total damage
        int diceRoll = dice.RollDice(); 
        int singleDieRoll = dice.RollSingleDie();
        float totalDamage = Mathf.Round(user.atp * dmgMod) + diceRoll - (target.dfp + singleDieRoll);
    }*/
}
