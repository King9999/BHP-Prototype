using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/* Base class for Hunters, Monsters, and Bosses. */
public abstract class Character : MonoBehaviour
{
    public string characterName;
    public float healthPoints, maxHealthPoints;
    public float skillPoints, maxSkillPoints;
    public float str;           //strength
    public float vit;           //vitality
    public float spd;           //speed
    public float mnt;           //mentality
    public float atp;           //attack power. STR + equipped weapon + any other bonuses
    public float dfp;           //defense power. VIT + equipped armor + any other bonuses
    public float mnp;           //mental power. MNT + equipped weapon + any other bonuses
    public float rst;           //resistance. MNT + equipped armor + any other bonuses
    public float mov;           //movement. MOV = SPD / 3 (rounded down) + any other bonuses
    public float evd = 0.05f;           //evade chance. 
    protected bool isTheirTurn; //if true, avatar can perform actions.
    protected bool turnTaken;

    //stat modifiers. 1 = 100% effectiveness, < 1 = lower effectiveness, > 1 = high effectiveness.
    public float hpMod = 1;
    public float spMod = 1;
    public float atpMod = 1;
    public float dfpMod = 1;
    public float spdMod = 1;
    public float mnpMod = 1;
    public float rstMod = 1;
    public float evdMod = 1;

    //coroutine check
    protected bool animateAttackCoroutineOn;



    //public List<Skill> skills;          //list of skills the avatar can choose from.
    //public List<Skill> skillEffects;    //list of skills this avatar is being affected by. Includes both permanent effects and those with durations.
    //public Dictionary<Skill, int> skillEffects;

    //resists are a value from 0 to 1. This allows multiple sources of a resist to take effect,
    //and makes it easier to recalculate values when equipping/unequipping sources.
    [Header("Ailment Status")]
    public float resistPoison;
    public float resistDizzy;
    public float resistBlind;
    public float resistBerserk;
    public float resistCardDrain;
    public float resistDisableLeg;
    public float resistDisableSkill;
    public float resistDisableSuper;
    public float resistWeak;

    /* STATUS DETAILS
        -------------
        Poisoned = Target loses (5% * duration count) of HP each turn. Lasts 3 turns.
        Dizzy = Target cannot act for 1 turn. Ailment removed if target is hit.
        Blind = 70% chance that an attack will miss. Cannot evade traps. Lasts for 3 turns
        Injured = HP is halved after being defeated. Stacks 2 times.
        Berserk = 50% more ATP but can't be controlled. Cannot defend.
        Disable Leg = total MOV reduced by 50% 
        Disable Skill = Cannot use skills
        Disable Super = Cannot gain super meter. 
        Weak = Roll 1 die for attacks/skills */
    public enum Status
    {
        Poisoned, Dizzy, Blind, Injured, Berserk, DisableLeg, DisableSkill, DisableSuper, Weak
    }
    public List<Status> status;     //ailments can stack. NOTE: This might be redundant since there will be scriptable objects to handle status effects
    
}
