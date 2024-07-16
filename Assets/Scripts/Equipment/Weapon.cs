using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Weapons raise either ATP or MNP, and can come with mods. */
[CreateAssetMenu(menuName = "Item/Equipment/Weapon", fileName = "weapon_")]
public class Weapon : Item
{
    public float atp, mnp;
    
    public List<ItemMod> itemMods;  //if there's a chip slot, there can only be 1 item mod.
    //public Skill itemSkill;         //only available if item has an empty chip slot
    public bool hasChipSlot;        //if true, itemSkill is available.
    public bool isUniqueItem;       //if true, chip slot counts as 1 item mod instead of 2.
    public int modCount = 3;            //default is 3. If item is not unique and has a chip slot, this value is 1. If item is unique, this value is 2.

    [System.NonSerialized] protected bool isEquipped = false;     //NonSerialized means Unity will reset the variable state

    void Reset()
    {
        itemType = ItemType.Weapon; //default type
        itemLevel = 1;
        //isEquipped = false;
    }

    public override void Equip(Hunter hunter)
    {
        //item can only be equipped if the player meets the level requirement
        if (hunter.hunterLevel < itemLevel || isEquipped)
            return;

        isEquipped = true;
        hunter.equippedWeapon = this;
        hunter.atp = hunter.str + atp;
        hunter.mnp = hunter.mnt + mnp;

        if (itemMods.Count > 0)
        {
            //apply effects of mods
            foreach(ItemMod mod in itemMods)
            {
                mod.ActivateOnEquip(hunter);
            }
        }

        //TODO: if there's a skill, add it to hunter's inventory.
        
    }

    public override void Unequip(Hunter hunter)
    {
        if (hunter.equippedWeapon == null || !isEquipped) return;

        isEquipped = false;
        hunter.equippedWeapon = null;
        hunter.atp = hunter.str - atp;
        hunter.mnp = hunter.mnt - mnp;

        if (itemMods.Count > 0)
        {
            //remove effects of item mods
            foreach (ItemMod mod in itemMods)
            {
                mod.DeactivateOnUnequip(hunter);
            }
        }

        //TODO: remove skill from inventory
    }

}
