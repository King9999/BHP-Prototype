using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Deals 2d6 * 3 damage to the enemy. User takes 50% the damage dealt. If both the user and enemy die, the user dies first, and
 * the enemy is left with 1 HP. This is to prevent abusing the skill. */
[CreateAssetMenu(menuName = "Skills/Active Skills/Risky Tackle", fileName = "activeSkill_RiskyTackle")]
public class ActiveSkill_RiskyTackle : ActiveSkill
{
    float userDamage;       //damage that's dealt to user.

    void Reset()
    {
        skillName = "Risky Tackle";
        skillID = "activeSkill_RiskyTackle";
        skillDetails = "Deal 3x damage. User takes half the damage dealt.";
        skillCost = 5;
        //skillCooldown = 0;
        //skillEffectDuration = 3;
        dmgMod = 3;
        minRange = 1;
        maxRange = 1;
        usageType = UsageType.Battle;
        weaponRestriction = WeaponRestriction.BeamSword;
    }

    /*public override void ActivateSkill(Character user, Character target)
    {
        base.ActivateSkill(user, target);

        roll dice and add result to total damage
        int diceRoll = dice.RollDice(); 
        int singleDieRoll = dice.RollSingleDie();
        float totalDamage = Mathf.Round(user.atp + diceRoll) * dmgMod - (target.dfp + singleDieRoll);
    }*/

    public override IEnumerator Animate(Character character, Character target)
    {
        Hunter hunter = character as Hunter;
        
        float moveSpeed = 12;

        //get the space in fron of target
        Vector3 destination = target.transform.position;
        float newX = 0;
        if (destination.x > hunter.transform.position.x)
        {
            newX = destination.x - 2;
        }
        else if (destination.x < hunter.transform.position.x)
        {
            newX = destination.x + 2;
        }

        destination = new Vector3(newX, destination.y, destination.z);
        Vector3 originalPos = hunter.transform.position;
        //run up to space in front of target
        while (hunter.transform.position != destination)
        {
            hunter.transform.position = Vector3.MoveTowards(hunter.transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        //deal damage, then return to original position.

        Combat combat = Singleton.instance.Combat;
        combat.DoDamage(hunter, target);
        combat.DoFixedDamage(hunter, hunter);       //self-damage

        yield return new WaitForSeconds(0.5f);
        hunter.transform.position = originalPos;
        
    }

    //This skill deals half damage to the user.
    public override float CalculateDamage(Character attacker, Character defender, int attackDiceRoll, int defenderDiceRoll)
    {
        float totalDamage = 0;
        float totalAttack_Atp = (attacker.atp * attacker.atpMod + attackDiceRoll) * dmgMod;
        float totalDefense_Dfp = defender.dfp * defender.dfpMod + defenderDiceRoll;
        
        Debug.LogFormat("Total Attack (ATP): {0}, Total Defense (DFP): {1}", totalAttack_Atp, totalDefense_Dfp);
        totalDamage = totalAttack_Atp - totalDefense_Dfp;

        //get 50% damage.
        userDamage = totalDamage / 2;
       
        return totalDamage;
    }

    //deal damage to user.
    public override float CalculateFixedDamage(Character attacker, Character defender) 
    {
        return userDamage;
    }
}
