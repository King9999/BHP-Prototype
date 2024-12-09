using JetBrains.Annotations;
using System;
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
    public float evd;           //evade chance. 
    protected bool isTheirTurn; //if true, avatar can perform actions.
    protected bool turnTaken;
    public bool cpuControlled;
    public bool isAttacker, isDefender;

    //stat modifiers. 1 = 100% effectiveness, < 1 = lower effectiveness, > 1 = high effectiveness.
    public float hpMod = 1;
    public float spMod = 1;
    public float atpMod = 1;
    public float dfpMod = 1;
    public float spdMod = 1;
    public float mnpMod = 1;
    public float rstMod = 1;
    public float evdMod = 1;
    public float movMod = 1;             
    public float debuffResist = 0;      //reduces the chance of being affected by a debuff. Value from 0 to 1. This is a hidden value.

    //coroutine check
    protected bool animateAttackCoroutineOn;

    [Header("Audiovisual\n-------")]
    public GameObject attackSpark;      //the visual effects for when character does a basic attack.
    public AudioClip attackSound;       //SFX for basic attack


    public List<Skill> skills;          //list of skills the avatar can choose from.
    [Header("---Targeting---")]
    public ActiveSkill chosenSkill;           //the skill the character will use.
    public Character targetChar;
    //public List<Skill> skillEffects;    //list of skills this avatar is being affected by. Includes both permanent effects and those with durations.
    //public Dictionary<Skill, int> skillEffects;

    //resists are a value from 0 to 1. This allows multiple sources of a resist to take effect,
    //and makes it easier to recalculate values when equipping/unequipping sources.
    [Header("---Ailment Status---")]
    public float resistPoison;
    public float resistDizzy;
    public float resistBlind;
    public float resistBerserk;
    public float resistCardDrain;
    public float resistDisableLeg;
    public float resistDisableSkill;
    public float resistDisableSuper;
    public float resistWeak;

    public enum CharacterState
    {
        Idle, Attacking, Guarding, Moving, Resting, Injured
    }

    public CharacterState characterState;

    [Header("---Room Location---")]
    public Room room;           //the room in the dungeon this character is currently occupying. Fast way to get a refernce to a room.

    //animations are contained here.
    [Serializable]
    public class CharacterAnimation
    {
        public CharacterState characterState;  //refers to the other characterState
        public List<Sprite> sprites;
    }
    [Header("---Animations---")]
    public List<CharacterAnimation> animations;
    bool looping;       //used to repeat animation indefinitely
    SpriteRenderer sr;
    bool animateCoroutineOn;
    /* DEBUFF DETAILS
        -------------
        Poisoned = Target loses (5% * duration count) of HP each turn. Lasts 3 turns.
        Dizzy = Target cannot act for 1 turn. Dizzy is removed if target is hit by an attack.
        Blind = 70% chance that an attack will miss. Cannot evade traps. Lasts for 3 turns
        Injured = HP is halved after being defeated. Stacks 2 times.
        Berserk = 50% more ATP but can't be controlled. Cannot defend.
        Disable Leg = total MOV reduced by 50% 
        Disable Skill = Cannot use skills
        Disable Super = Cannot gain super meter. 
        Weak = Roll 1 die for attacks/skills 
        Unlucky = when attacking or defending, all rolls for the afflicted user occur twice, and the worst of the two results is applied.*/

   
    public enum Debuff
    {
        Poisoned, Dizzy, Blind, Injured, Berserk, DisableLeg, DisableSkill, DisableSuper, Weak, Unlucky
    }

    /* BUFF DETAILS
     * -------
     * Regen = 15% of max HP restored at the end of each turn.
     * Empowered = ATP and MNP increased by 25%
     * Haste = +4 to MOV
     * SecondWind = When HP is 0, 50% HP is restored immediately, and Injured does not occur. SecondWind cannot be applied again on the same character after triggering.
     * Lucky = when attacking or defending, all rolls for the user occur twice, and the best of the two results is applied. 
     * 
     * */
    public enum Buff
    {
        Regen, Empowered, Haste, SecondWind, Lucky
    }

    [Header("---Status Effects---")]
    public List<StatusEffect> debuffs;     //Characters can have up to 3 buffs and debuffs. Adding a 4th overwrites the oldest effect.
    public List<StatusEffect> buffs;
    public bool MaxDebuffs { get { return debuffs.Count >= 3; } }
    public bool MaxBuffs { get { return buffs.Count >= 3; } }

    public void Attack(ActiveSkill skill, Character target) 
    {
        StartCoroutine(skill.Animate(this, target)); 
    }
    public void Defend() { }

    private void OnEnable()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Checks for valid targets in a given range. Used by CPU characters.
    /// </summary>
    /// <param name="skillRange">the rooms to check for any characters</param>
    /// <returns>The list of valid characters.</returns>
    public List<Character> CPU_CheckCharactersInRange(List<Room> skillRange) 
    {
        List<Character> targetChars = new List<Character>();

        foreach (Room room in skillRange)
        {
            if (room.character != null)
            {
                targetChars.Add(room.character);
            }
        }

        return targetChars;
    }

    //this method will be used to display different animations.
    public void ChangeCharacterState(CharacterState state)
    {
        animateCoroutineOn = false; //stops current animation
        StopAllCoroutines();

        switch(state)
        {
            case CharacterState.Idle:
                StartCoroutine(AnimateCharacter(state, looping: true));
                break;

            case CharacterState.Attacking:
                break;

            case CharacterState.Guarding:
                break;

            case CharacterState.Moving:
                StartCoroutine(AnimateCharacter(state, looping: true));
                break;

            case CharacterState.Resting:
                break;

            case CharacterState.Injured:
                break;
        }
    }

    public StatusEffect GetStatusEffect(StatusEffect.Effect effectType, List<StatusEffect> effects)
    {
        StatusEffect statusEffect = null;
        int i = 0;
        bool effectFound = false;
        while(!effectFound && i < effects.Count)
        {
            if (effects[i].effect == effectType)
            {
                effectFound = true;
                statusEffect = effects[i];
            }
            else
            {
                i++;
            }
        }
        return statusEffect;
    }

    /// <summary>
    /// Play given animation.
    /// </summary>
    /// <param name="state">The state containing the animation set to play.</param>
    /// <param name="looping">If true, animation repeats infinitely.</param>
    /// <returns></returns>
    IEnumerator AnimateCharacter(CharacterState state, bool looping = false)
    {
        //find the animation set.
        bool stateFound = false;
        int i = 0;
        while(!stateFound && i < animations.Count)
        {
            if (state == animations[i].characterState)
            {
                stateFound = true;
            }
            else
            {
                i++;
            }
        }

        if (!stateFound)
            //can't continue further
            yield return null;

        int j = 0;
        float animationTime = 0.016f;
        animateCoroutineOn = true;
        while (animateCoroutineOn && j < animations[i].sprites.Count)
        {
            sr.sprite = animations[i].sprites[j];
            yield return new WaitForSeconds(animationTime);

            //j is reset to first animation frame if looping is true
            j = (j + 1 >= animations[i].sprites.Count && looping == true) ? 0 : j + 1;
            
        }


        
        //once state is found, run the frames.
        yield return null;
    }

    //performs movement before damage is calculated.
    protected virtual IEnumerator AnimateAttack(Character attacker, Character defender)
    {
        yield return null;
    }

    

}
