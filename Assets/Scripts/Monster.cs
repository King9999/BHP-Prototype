using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Monsters are CPU-controlled characters. Their level is the average level of all hunters. The items they drop depends on their level. 
    At higher levels, monsters gain more abilities to make them more of a threat. */
public abstract class Monster : Character
{
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
    public int baseMoney;                //amount of money dropped scales with level.
    public float growthRate;            //increases stats. Not sure if I want individual rates for each stat yet.
    public enum AI_State
    {
        Idle, Moving, Aggro, Retreating
    }

    public AI_State state;
    public Monster_AI cpuBehaviour;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /* Generate monster stats based on the average level of the hunters.
     * The average will be calculated elsewhere.
     * */
    public void InitialzeStats(int averageLevel)
    {
        if (averageLevel < 1) return;
        //get average hunter level
        /*int averageLevel = 0;
        if (hunters != null)
        {
            foreach (Hunter hunter in hunters)
            {
                averageLevel += hunter.hunterLevel;
            }
            averageLevel /= hunters.Count;
        }*/

        //growth rate is 0 if average level is less than 5.
        growthRate = averageLevel < 5 ? 0 : 0.125f;

        monsterLevel = averageLevel;
        maxHealthPoints = baseHealthPoints * (1 + Mathf.Round(growthRate * monsterLevel));
        maxSkillPoints = baseSkillPoints * (1 + Mathf.Round(growthRate * monsterLevel));
        atp = baseAtp * (1 + Mathf.Round(growthRate * monsterLevel));
        dfp = baseDfp * (1 + Mathf.Round(growthRate * monsterLevel));
        mnp = baseMnp * (1 + Mathf.Round(growthRate * monsterLevel));
        rst = baseRst * (1 + Mathf.Round(growthRate * monsterLevel));
        spd = baseSpd * (1 + Mathf.Round(growthRate * monsterLevel));
        evd = baseEvd * (1 + Mathf.Round(growthRate * monsterLevel));
        //Debug.Log("Monster ATP: " + baseAtp * 1 + (growthRate * monsterLevel));
        mov = baseMov + (monsterLevel / 5);
        //evd = Mathf.Round(baseEvd)
        healthPoints = maxHealthPoints;
        skillPoints = maxSkillPoints;
    }
}
