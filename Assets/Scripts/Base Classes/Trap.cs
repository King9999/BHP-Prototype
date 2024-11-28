using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Traps are triggered when a character passes over them. They are invisible, but there can be ways to detect them. */
public abstract class Trap : ScriptableObject
{
    public string trapName;
    public enum TrapID      //This must be updated as new traps are added.
    {
        Damage, Dizzy, DisableLeg, CardDrain
    }
    public TrapID trapID;


    public virtual void ActivateTrap(Character target) { }
}
