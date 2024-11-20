using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//base class for all interactable objects in a dungeon
public abstract class Entity : MonoBehaviour
{
    public bool playerInteracted;       //a player interacted with object, so no other players can interact.
    public EntityEffect effect;         //scriptable object
    public Room room;                   //reference to the room the object is on.


    /* activate an entity.
     * Hunter parameter: the hunter who activated the entity. They receive any bonuses/items the entity provides. */
    /*public void ActivateEntity(Hunter hunter)
    {
        if (effect == null) return;

        effect.
    }*/
}
