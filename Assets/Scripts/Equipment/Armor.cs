using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Net.WebRequestMethods;

[CreateAssetMenu(menuName = "Item/Equipment/Armor", fileName = "armor_")]
public class Armor : Item
{
    public int dfp, rst;
    public List<ItemMod> itemMods;  //if there's a chip slot, there can only be 1 item mod.
    //public Skill itemSkill;         //only available if item has an empty chip slot
    public bool hasChipSlot;        //if true, itemSkill is available.
    [System.NonSerialized] protected bool isEquipped = false;     //NonSerialized means Unity will reset the variable state

    // Start is called before the first frame update
    void Reset()
    {
        itemType = ItemType.Armor; //default type
        itemLevel = 1;
    }

    public override void Equip(Hunter hunter)
    {
        //item can only be equipped if the player meets the level requirement
        if (hunter.hunterLevel < itemLevel || isEquipped)
            return;

        isEquipped = true;
        hunter.equippedArmor = this;
        hunter.dfp = hunter.vit + dfp;
        hunter.rst = Mathf.Round(hunter.mnt / 2) + rst;

        if (itemMods.Count > 0)
        {
            //apply effects of mods
            foreach (ItemMod mod in itemMods)
            {
                mod.ActivateOnEquip(hunter);
            }
        }

        //TODO: if there's a skill, add it to hunter's inventory.

    }

    public override void Unequip(Hunter hunter)
    {
        if (hunter.equippedArmor == null || !isEquipped) return;

        isEquipped = false;
        hunter.equippedArmor = null;
        hunter.dfp = hunter.vit - dfp;
        hunter.rst = Mathf.Round(hunter.mnt / 2) - rst;

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
