using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Base class for all effects in game */
public abstract class Effect : ScriptableObject
{
    public virtual void ActivateEffect() { }
}
