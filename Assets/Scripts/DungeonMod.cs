using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Dungeon mods add modifiers to dungeons. They are typically found in dungeons, but can sometimes be sold in shops */
[CreateAssetMenu(menuName = "Item/Dungeon Mod", fileName = "mod_")]
public class DungeonMod : Item
{
    //public DungeonModEffect modEffect;
    
    void Reset()
    {
        itemType = ItemType.DunegonMod;
    }

    public void ActivateMod() 
    {
        //if (modEffect == null) return;

        //trigger mod effect here
    }
  
}
