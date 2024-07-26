using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Item mods provide an effect when an item is equipped, and removed when the item is unequipped. */
public abstract class ItemMod : ScriptableObject
{
    public string modName;
    public string modID;           //used for looking up mods in JSON file.
    public int modLevel;        //a level from 1 to 5. Higher level = stronger mods. Unique mods ignore mod level.
    public bool isChipSlot;    //if true, this mod can be replaced with a skill.


    public virtual void ActivateOnEquip(Hunter hunter) { }
    public virtual void DeactivateOnUnequip(Hunter hunter) { }
}
