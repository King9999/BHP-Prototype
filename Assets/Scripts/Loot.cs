using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Loot is an item that has no use besides selling it. Oftentimes, it's a target item in a dungeon. */
[CreateAssetMenu(menuName = "Item/Loot", fileName = "loot_")]
public abstract class Loot : Item
{
    void Reset()
    {
        itemType = ItemType.Loot;
    }

}
