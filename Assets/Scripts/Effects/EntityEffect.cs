using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* effects that are triggered by interacting with an entity such as a terminal or an exit point */
//[CreateAssetMenu(menuName = "Effects/Entity Effects", fileName = "entityEffect_")]
public abstract class EntityEffect : ScriptableObject
{
    public string effectName, effectDetails;    //for my information only
    public int effectID;                        //used to locate an effect easily

    /* activate an entity.
     * Hunter parameter: the hunter who activated the entity. They receive any bonuses/items the entity provides. */
    public virtual void Activate(Hunter hunter) { }
}
