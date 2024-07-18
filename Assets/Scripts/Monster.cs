using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Monsters are CPU-controlled characters. Their level is the average level of all hunters. The items they drop depends on their level. 
    At higher levels, monsters gain more abilities to make them more of a threat. */
public abstract class Monster : Character
{
    public int monsterLevel = 1;            //this is the average of the level of all hunters.
    public float baseAtp;
    public float baseDfp;
    public float baseMnp;
    public float baseEvd = 0.05f;
    public float baseRst;
    public int baseMov;
    public float baseHealthPoints;      //max health
    public float baseSkillPoints;        //max Sp
    public float growthRate;            //increases stats. Not sure if I want individual rates for each stat yet.
    public enum AI_State
    {
        Idle, Moving, Aggro, Retreating
    }

    public AI_State state;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void InitialzeStats(List<Hunter> hunters)
    {
        //get average hunter level
        int averageLevel = 0;
        if (hunters != null)
        {
            foreach (Hunter hunter in hunters)
            {
                averageLevel += hunter.hunterLevel;
            }
            averageLevel /= hunters.Count;
        }

        //growth rate is 0 if average level is less than 5.
        growthRate = averageLevel < 5 ? 0 : 0.125f;

        monsterLevel = averageLevel;
        maxHealthPoints = Mathf.Round(baseHealthPoints * 1 + (growthRate * monsterLevel));
        maxSkillPoints = Mathf.Round(baseSkillPoints * 1 + (growthRate * monsterLevel));
        atp = Mathf.Round(baseAtp * 1 + (growthRate * monsterLevel));
        dfp = Mathf.Round(baseDfp * 1 + (growthRate * monsterLevel));
        mnp = Mathf.Round(baseMnp * 1 + (growthRate * monsterLevel));
        rst = Mathf.Round(baseRst * 1 + (growthRate * monsterLevel));
        mov = baseMov + (monsterLevel / 5);
        //evd = Mathf.Round(baseEvd)
        healthPoints = maxHealthPoints;
        skillPoints = maxSkillPoints;
    }
}
