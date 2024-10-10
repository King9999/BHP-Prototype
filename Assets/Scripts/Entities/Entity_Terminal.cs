using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Terminals grant a random positive effect to the hunter that accesses it. */
public class Entity_Terminal : Entity
{
    public Sprite activeTerminal, inactiveTerminal;

    // Start is called before the first frame update
    void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = activeTerminal;
    }

    //can use the below code to check the radius for when adding objects to dungeon in the Dungeon script. If another object is colliding with
    //the sphere, this object's position is re-rolled.
    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 2);
    }*/

    public void ActivateTerminal(Hunter hunter)
    {
        //get a random bonus and apply it to hunter.
    }
    
}
