using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        minRange = 0;
        maxRange = 1;
        usageType = UsageType.Battle;
        weaponRestriction = WeaponRestriction.None;
    }

    //if character has a melee weapon, they run up to target to strike. Otherwise, they attack from a distance.
    public override void ActivateSkill(Character user, Character target)
    {
        //base.ActivateSkill(user, target);

        //yield return Animate(user);
        //roll dice and add result to total damage
        int diceRoll = dice.RollDice(); 
        int singleDieRoll = dice.RollSingleDie();
        float totalDamage = Mathf.Round(user.atp + diceRoll) * dmgMod - (target.dfp + singleDieRoll);
    }

    public override IEnumerator Animate(Character character, Vector3 destination)
    {
        //different function depending on whether weapon is ranged or melee
        Hunter hunter = character as Hunter;
        //if (hunter.equippedWeapon.weaponType == Weapon.WeaponType.BeamSword)
        //{
            float moveSpeed = 12;

        //get the space in fron of target
        float direction = destination.x - hunter.transform.position.x;
        float newX = 0;
        if (destination.x >= hunter.transform.position.x)
        {
            newX = destination.x - 2;
        }
        else
        {
            newX = destination.x + 2;
        }

        destination = new Vector3(newX, destination.y, destination.z);
            //run up to space in front of target
            while (hunter.transform.position != destination)
            {
                hunter.transform.position = Vector3.MoveTowards(hunter.transform.position, destination, moveSpeed * Time.deltaTime);
                yield return null;
            }

            //attack animation

        //}
       // else
        //{
            //ranged attack. Just call sprite animation
           // yield return null;
        //}
    }

}
