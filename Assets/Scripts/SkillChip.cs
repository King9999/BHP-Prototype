using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*skill chips contain skills, and can be equipped directly by hunters, or inserted into items. If they're inserted into items,
 * they cannot be removed afterwards. Skill chips are instantiated with a random skill. skills can never be unique. */
[CreateAssetMenu(menuName = "Item/Skill Chip", fileName = "skillChip_")]
public class SkillChip : Item
{
    public Skill skill;
    
    void Reset()
    {
        itemType = ItemType.SkillChip;
        price = 2500;   //all chips have the same price since there's no rarity associated with skills.
    }

    //generate a random skill.
    void Awake()
    {
        SkillManager sm = Singleton.instance.SkillManager;
        if (Random.value <= 0.5f)
            skill = sm.AddSkill(sm.activeSkillList);
        else
            skill = sm.AddSkill(sm.passiveSkillList);

        itemName = string.Format("Skill Chip [{0}]", skill.skillName);
        this.name += skill.skillName;   //appends skill name to the file name.
        details = skill.skillDetails;

        if (skill is ActiveSkill activeSkill)
        {
            details += string.Format("\n\nCost: {0} SP\nCooldown: {1}", activeSkill.skillCost, activeSkill.skillCooldown);
            if (activeSkill.minRange > 0)
                details += string.Format("\nRange: {0} - {1}", activeSkill.minRange, activeSkill.maxRange);
            else
                details += string.Format("\nRange: {0}", activeSkill.maxRange);
        }

        details += string.Format("\n\nWpn. Restriction: {0}\nUsage: {1}", skill.weaponRestriction, skill.usageType);
    }
}
