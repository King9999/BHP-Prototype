using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* handles all skills in the game. It can add or remove skills from characters. */
public class SkillManager : MonoBehaviour
{
    public List<Skill> activeSkillList, passiveSkillList;

    public static SkillManager instance;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    //adds a random skill from a given list
    public Skill AddSkill(List<Skill> skillList/*, Skill.WeaponRestriction restriction*/)
    {
        if (skillList.Count <= 0)
            return null;

        /*int randSkill;
        do
        {
            randSkill = Random.Range(0, skillList.Count);
        }
        while (restriction != skillList[randSkill].weaponRestriction);*/
        int randSkill = Random.Range(0, skillList.Count);
        return Instantiate(skillList[randSkill]);
    }

    
    public Skill AddSkill(List<Skill> skillList, string skillID)
    {
        bool skillFound = false;
        int i = 0;
        while (!skillFound && i < skillList.Count)
        {
            if (skillID.ToLower().Equals(skillList[i].skillID.ToLower()))
            {
                skillFound = true;
            }
            else
            {
                i++;
            }
        }

        if (skillFound)
            return skillList[i];
        else
            return null;
    }
}
