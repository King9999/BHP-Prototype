using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Treasure chests contain randomly generated items. Characters can pass through them, and also land on them,
 * but only Hunters can open chests. */
public class Entity_TreasureChest : Entity
{
    public Item item;       //the item the chest holds
    public int credits;     //money
    public Sprite openChest, closedChest;

    // Start is called before the first frame update
    void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = closedChest;
    }

    //can use the below code to check the radius for when adding objects to dungeon in the Dungeon script. If another object is colliding with
    //the sphere, this object's position is re-rolled.
    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 2);
    }*/

    public void OpenChest(Hunter hunter)
    {
        if (playerInteracted)
            return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = openChest;

        if (item != null)
        {
            hunter.inventory.Add(item);

            if (item.isTargetItem)
            {
                //TODO: play some fanfare and special animation
                hunter.targetItem = hunter.inventory[hunter.inventory.Count - 1];
            }
            Debug.LogFormat("{0} obtained {1}", hunter.characterName, item.itemName);
            item = null;
        }
        else  //chest contains money
        {
            hunter.credits += credits;
            Debug.LogFormat("{0} obtained {1} CR", hunter.characterName, credits);
            credits = 0;
        }
        playerInteracted = true;
    }
}
