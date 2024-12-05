using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Escape the dungeon if the hunter has the target item. If a character doesn't have the target item, they will be
 * teleported to a random empty space. Monsters can use exits to warp. */
public class Entity_Exit : Entity
{
    //public Sprite exitSprite;

    //can use the below code to check the radius for when adding objects to dungeon in the Dungeon script. If another object is colliding with
    //the sphere, this object's position is re-rolled.
    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 2);
    }*/

    public void TeleportCharacter(Character character)
    {
        Dungeon dun = Singleton.instance.Dungeon;
        if (character is Hunter hunter)
        {
            //check for target item
            if (hunter.HasTargetItem())
            {
                //game is over, target has been found.
                Debug.LogFormat("{0} is the winner!", hunter.characterName);
            }
            else
            {
                //teleport to random empty space
                bool roomFound = false;
                while(!roomFound)
                {
                    int randRoom = Random.Range(0, dun.dungeonRooms.Count);

                    if (dun.dungeonRooms[randRoom].entity == null && dun.dungeonRooms[randRoom].character == null)
                    {
                        roomFound = true;
                        dun.UpdateCharacterRoom(hunter, dun.dungeonRooms[randRoom]);

                    }
                }
            }
        }
        else
        {
            //teleport to empty space
            bool roomFound = false;
            while (!roomFound)
            {
                int randRoom = Random.Range(0, dun.dungeonRooms.Count);

                if (dun.dungeonRooms[randRoom].entity == null && dun.dungeonRooms[randRoom].character == null)
                {
                    roomFound = true;
                    dun.UpdateCharacterRoom(character, dun.dungeonRooms[randRoom]);
                }
                    
            }
        }
    }

    /*private bool HunterHasTargetItem(Hunter hunter)
    {
        bool targetFound = false;
        int i = 0;
        while (!targetFound && i < hunter.inventory.Count)
        {
            if (hunter.inventory[i].isTargetItem) 
                targetFound = true;
            else
                i++;
        }
              
        return targetFound;
    }*/

}
