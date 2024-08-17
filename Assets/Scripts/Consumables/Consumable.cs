using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Consumable : Item
{
    private void Reset()
    {
        itemType = ItemType.Consumable;
    }
    public virtual void ActivateEffect(Hunter user) 
    { 

    }
}
