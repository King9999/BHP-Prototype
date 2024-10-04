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
        price = 500;   //all chips have the same price since there's no rarity associated with skills.
    }

    //generate a random skill.
    void Awake()
    {
        SkillManager sm = Singleton.instance.SkillManager;
        if (Random.value <= 0.5f)
            skill = sm.AddSkill(sm.activeSkillList);
        else
            skill = sm.AddSkill(sm.passiveSkillList);

        itemName = "Skill Chip [" + skill.skillName + "]";
        this.name += skill.skillName;
        details = skill.skillDetails;

        if (skill is ActiveSkill activeSkill)
        {
            details += "\n\nCost: " + activeSkill.skillCost + " SP\nCooldown: " + activeSkill.skillCooldown;
            if (activeSkill.minRange > 0)
                details += "\nRange: " + activeSkill.minRange + " - " + activeSkill.maxRange;
            else
                details += "\nRange: " + activeSkill + activeSkill.maxRange;
        }

        details += "\n\nWpn. Restriction: " + skill.weaponRestriction + "\nUsage: " + skill.usageType;
    }
}
