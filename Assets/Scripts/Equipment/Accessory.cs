using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Item;

[CreateAssetMenu(menuName = "Item/Equipment/Accessory", fileName = "acc_")]
public class Accessory : Item
{
    public float atp, mnp;
    public float dfp, rst;
    public float str, vit, mnt, spd, evd;
    public int mov;

    public List<ItemMod> itemMods;  //if there's a chip slot, there can only be 1 item mod.
    public Skill itemSkill;         //only available if item has an empty chip slot
    public bool hasChipSlot;        //if true, itemSkill is available.
    public bool isUniqueItem;       //if true, item has a fixed mod that it will always have.
    public int modCount = 3;            //default is 3. If item is not unique and has a chip slot, this value is 1. If item is unique, this value is 2.


    protected bool isEquipped = false;     

    private void Awake()
    {
        //if unique item, roll 2 mods. There should already be 1 mod on the item.
    }

    void Reset()
    {
        itemType = ItemType.Accessory; //default type
        itemLevel = 1;
    }

    /* equipping an accessory works a little differently. It simply adds to the existing values. */
    public override void Equip(Hunter hunter)
    {
        //item can only be equipped if the player meets the level requirement
        if (hunter.hunterLevel < itemLevel || isEquipped)
            return;

        isEquipped = true;
        hunter.equippedAccessory = this;
        hunter.atp += atp;
        hunter.mnp += mnp;
        hunter.dfp += dfp;
        hunter.rst += rst;
        hunter.str += str;
        hunter.vit += vit;
        hunter.spd += spd;
        hunter.mnt += mnt;
        hunter.evd += evd;
        hunter.mov += mov;
        
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
        if (hunter.equippedAccessory == null || !isEquipped) return;

        isEquipped = false;
        hunter.equippedAccessory = null;

        hunter.atp -= atp;
        hunter.mnp -= mnp;
        hunter.dfp -= dfp;
        hunter.rst -= rst;
        hunter.str -= str;
        hunter.vit -= vit;
        hunter.spd -= spd;
        hunter.mnt -= mnt;
        hunter.evd -= evd;
        hunter.mov -= mov;

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
