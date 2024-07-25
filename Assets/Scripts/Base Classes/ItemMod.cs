using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Item mods provide an effect when an item is equipped, and removed when the item is unequipped. */
public abstract class ItemMod : ScriptableObject
{
    public string modName;
    public int modID;           //used for looking up mods in JSON file.
    public bool isChipSlot;    //if true, this mod can be replaced with a skill.

    public virtual void ActivateOnEquip(Hunter hunter) { }
    public virtual void DeactivateOnUnequip(Hunter hunter) { }
}
