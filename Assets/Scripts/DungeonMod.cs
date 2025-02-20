using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Dungeon mods add modifiers to dungeons. They are typically found in dungeons, but can sometimes be sold in shops */
//[CreateAssetMenu(menuName = "Item/Dungeon Mod", fileName = "mod_")]
public abstract class DungeonMod : Item
{
    //public DungeonModEffect modEffect;
    public string modUpside, modDownside;       //these will be displayed in-game somewhere
    
    void Reset()
    {
        //itemType = ItemType.DunegonMod;
    }

    public virtual void ActivateMod() 
    {
        //if (modEffect == null) return;

        //trigger mod effect here
    }

    //undoes any changes by the mod
    public virtual void DeactivateMod()
    {

    }
  
}
