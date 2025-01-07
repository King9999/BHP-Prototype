using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/* Monsters are CPU-controlled characters. Their level is the average level of all hunters. The items they drop depends on their level. 
    At higher levels, monsters gain more abilities to make them more of a threat. */
public abstract class Monster : Character
{
    public MonsterData monsterData;
    public int monsterLevel = 1;            //this is equal to the average of the level of all hunters.
    public float baseAtp;
    public float baseDfp;
    public float baseMnp;
    public float baseEvd = 0.05f;
    public float baseRst;
    public float baseSpd;
    public int baseMov;
    public float baseHealthPoints;      //max health
    public float baseSkillPoints;        //max Sp
    public int minAttackRange, maxAttackRange;      //range of standard attack.
    public int baseMoney, credits;       //amount of money dropped scales with level.
    public float growthRate;            //increases stats. Not sure if I want individual rates for each stat yet.
    const float growthValue = 0.125f;
    public float dropChance;            //the chance that monster drops an item. Data is pulled from MonsterData.
    public enum AI_State
    {
        Idle, Moving, Aggro, Retreating
    }

    public AI_State state;
    public Monster_AI cpuBehaviour;

    private void Awake()
    {
        cpuControlled = true;
    }


    /* Generate monster stats based on the average level of the hunters.
     * The average will be calculated elsewhere.
     * */
    public void InitializeStats(int averageLevel, MonsterData data)
    {
        if (averageLevel < 1) return;

        //get monster data
        characterName = data.monsterName;
        baseAtp = data.baseAtp;
        baseDfp = data.baseDfp;
        baseMnp = data.baseMnp;
        baseEvd = data.baseEvd;
        baseRst = data.baseRst;
        baseSpd = data.baseSpd;
        baseHealthPoints = data.baseHealthPoints;
        baseSkillPoints = data.baseSkillPoints;
        minAttackRange = data.minAttackRange;
        maxAttackRange = data.maxAttackRange;
        growthRate = data.growthRate;
        cpuBehaviour = data.behaviour;
        dropChance = data.dropChance;

        //add skills
        for (int i = 0; i < data.skills.Count; i++)
            skills.Add(Instantiate(data.skills[i]));

        //update basic attack data
        ActiveSkill basicAttack = data.skills[0] as ActiveSkill;
        basicAttack.attribute = data.attribute;
        basicAttack.monsterSkillRange = data.monsterSkillRange;
        basicAttack.minRange = data.minAttackRange;
        basicAttack.maxRange = data.maxAttackRange;
        basicAttack.dmgMod = data.dmgMod;


        //growth rate is 0 if average level is less than 5.
        growthRate = averageLevel < 5 ? 0 : growthValue;

        monsterLevel = averageLevel;
        maxHealthPoints = Mathf.Round(baseHealthPoints * (1 + growthRate * monsterLevel));
        maxSkillPoints = Mathf.Round(baseSkillPoints * (1 + growthRate * monsterLevel));
        atp = Mathf.Round(baseAtp * (1 + growthRate * monsterLevel));
        dfp = Mathf.Round(baseDfp * (1 + growthRate * monsterLevel));
        mnp = Mathf.Round(baseMnp * (1 + growthRate * monsterLevel));
        rst = Mathf.Round(baseRst * (1 + growthRate * monsterLevel));
        spd = Mathf.Round(baseSpd * (1 + growthRate * monsterLevel));
        evd = Mathf.Round(baseEvd * (1 + growthRate * monsterLevel));
        //Debug.Log("Monster ATP: " + baseAtp * 1 + (growthRate * monsterLevel));
        mov = baseMov + (monsterLevel / 10);
        credits = baseMoney + (monsterLevel - 1) * Mathf.RoundToInt(baseMoney * 0.55f);
        healthPoints = maxHealthPoints;
        skillPoints = maxSkillPoints;
    }

    public void MoveMonster()
    {
        StartCoroutine(cpuBehaviour.MoveMonster(this));
    }

    public void UseSkill()
    {
        StartCoroutine(cpuBehaviour.UseSkill(this));
    }

    //used in cases where monster may be in range to attack before moving.
    public void CheckHuntersInRange()
    {
        //Before moving, check if the hunter is already in attack range by checking all applicable skills.
        //Pick basic attack for checking, will choose another skill later
        List<Room> skillRange = new List<Room>();
        List<Character> targetChars = new List<Character>();
        List<ActiveSkill> activeSkills = new List<ActiveSkill>();
        GameManager gm = Singleton.instance.GameManager;
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i] is ActiveSkill activeSkill && activeSkill.skillCost <= skillPoints)
            {
                skillRange = gm.ShowSkillRange(this, activeSkill.minRange, activeSkill.maxRange);
                targetChars = CPU_CheckCharactersInRange(skillRange);
                if (targetChars.Count > 0)
                {
                    activeSkills.Add(activeSkill);
                }
            }

        }

        //pick a random skill
        MonsterManager mm = Singleton.instance.MonsterManager;

        if (activeSkills.Count > 0)
        {
            int randSkill = Random.Range(0, activeSkills.Count);
            int randTarget = Random.Range(0, targetChars.Count);
            chosenSkill = activeSkills[randSkill];
            targetChar = targetChars[randTarget];
            mm.ChangeMonsterState(this, mm.aiState = MonsterManager.MonsterState.UseSkill);
        }
        else
        {
            mm.ChangeMonsterState(this, mm.aiState = MonsterManager.MonsterState.Moving);
        }
    }
}
