using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* these contain base stats for every monster in the game. */
[CreateAssetMenu(menuName = "Monster Data", fileName = "monsterData_")]
public class MonsterData : ScriptableObject
{
    public string monsterName;
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
    //public MonsterDropTable dropTable;

    [Serializable]
    public struct DropTable
    {
        public Item item;
        public int itemWeight;
    }

    [Serializable]
    public struct DropTables
    {
        public int minLevel;            //the minimum monster level required to access this drop table.
        public List<DropTable> dropTable;
    }

    public List<DropTables> dropTables;

    
}
