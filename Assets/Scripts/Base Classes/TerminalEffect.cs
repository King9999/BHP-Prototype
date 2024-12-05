using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Terminal effects apply a buff to the hunter who touches the terminal with the effect. The effect
 * lasts for the duration of the dungeon run, and cannot be removed. */
public abstract class TerminalEffect : ScriptableObject
{
    public string effectName, effectMessage;    //effectMessage is what appears on screen when terminal is touched.

    public enum EffectID { Duplicate, Overflow, BleedMoney }
    public EffectID effectID;

    [Header("---Trigger effects---")]
    public bool triggerWhenDamageTaken;
    public bool triggerOnCardDraw;

    public virtual void ActivateEffect(Hunter hunter) 
    {
        
    }
                    
}
