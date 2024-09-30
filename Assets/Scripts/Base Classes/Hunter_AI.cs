using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* CPU Hunters use a scriptable object that control their behaviour. These objects will have toggles that determine a hunter's
 * actions. Behaviours also influence which stats are more likely to rise during hunter generation. 
 * This allows CPU hunters to specialize in a few stats, while being weak in others. */
public abstract class Hunter_AI : ScriptableObject
{
    [Header("---AI Behaviours---")]
    public string behaviourType;        //internal info only. It tells me what kind of behaviour the hunter has.
    public bool canAttackHunters;
    public bool canAttackMonsters;          //also includes bosses
    public bool canOpenChests;
    public bool canUseTerminals;

    [Header("---Stats Influence---")]
    
    public int rollStr;
    public int rollVit;               /* These are the odds that a stat is chosen during hunter generation.
                                    * The sum of these values must be 100% */
    public int rollSpd;
    public int rollMnt;
}
