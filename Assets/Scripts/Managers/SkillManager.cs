using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* handles all skills in the game. It can add or remove skills from characters. */
public class SkillManager : MonoBehaviour
{
    public List<Skill> activeSkillList, passiveSkillList;
    public List<SuperAbility> superList;

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

    //add random super
    public SuperAbility AddSuper()
    {
        int randSuper = Random.Range(0, superList.Count);
        return Instantiate(superList[randSuper]);
    }

    public SuperAbility AddSuper(string skillID)
    {
        bool skillFound = false;
        int i = 0;
        SuperAbility super = null;
        while (!skillFound && i < superList.Count)
        {
            if (superList[i].skillID.Equals(skillID))
            {
                skillFound = true;
                super = Instantiate(superList[i]);
            }
            else
            {
                i++;
            }
        }

        return super;
    }
}
