using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Restores Hunter's HP to full, and removes all debuffs, including Injured. Debuffs are removed
 * before the HP is restored. */
[CreateAssetMenu(menuName = "Super Ability/Full Heal", fileName = "sa_fullHeal")]
public class SuperAbility_FullHeal : SuperAbility
{
    // Start is called before the first frame update
    void Reset()
    {
        skillName = "Full Heal";
        chargePentalty = 0.5f;
    }

    public override void ActivateSkill(Character user, Character target)
    {
        superMeter = 0;
        foreach(CharacterEffect debuff in target.debuffs)
        {
            debuff.CleanupEffect(target);
        }
        target.debuffs.Clear();
        target.healthPoints = target.maxHealthPoints;

    }
}