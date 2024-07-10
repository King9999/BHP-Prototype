using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Weapons raise either ATP or MNP, and can come with mods. */
[CreateAssetMenu(menuName = "Item/Equipment/Weapon", fileName = "weapon_")]
public class Weapon : Item
{
    public int atp, mnp;
    public int itemLevel;
    private int maxItemLevel { get; } = 50;
    public List<ItemMod> itemMods;
    
    void Reset()
    {
        itemType = ItemType.Weapon; //default type
        itemLevel = 1;
        isEquipped = false;
    }

    public override void Equip(Hunter hunter)
    {
        //item can only be equipped if the player meets the level requirement
        if (hunter.hunterLevel < itemLevel)
            return;

        hunter.atp = hunter.str + atp;
        //hunter.dfp = hunter.vit + dfp;
        hunter.mnp = hunter.mnt + mnp;
        //hunter.rst = (hunter.mnt / 2) + rst;

        if (itemMods.Count > 0)
        {
            //apply effects of mods
        }
    }

    public override void Unequip(Hunter hunter)
    {
        if (hunter.equippedWeapon == null) return;

        hunter.atp = hunter.str - atp;
        //hunter.dfp = hunter.vit - dfp;
        hunter.mnp = hunter.mnt - mnp;
        //hunter.rst = (hunter.mnt / 2) - rst;

        if (itemMods.Count > 0)
        {
            //remove effects of item mods

        }
    }

}
