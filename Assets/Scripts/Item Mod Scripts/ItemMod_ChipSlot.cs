using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//chip slots allow the player to add a skill to item.
[CreateAssetMenu(menuName = "Item Mod/Chip Slot", fileName = "itemMod_chipSlot")]
public class ItemMod_ChipSlot : ItemMod
{
    private void Reset()
    {
        modName = "<CHIP SLOT>";
        isChipSlot = true;
        modID = "ItemMod_ChipSlot";
        modLevel = 1;
    }

    /*When equipped, the player will have access to a submenu to add a skill to the item. Once the skill is equipped, this mod must 
     be replaced. */
    public override void ActivateOnEquip(Hunter hunter)
    {
        
    }

    public override void DeactivateOnUnequip(Hunter hunter)
    {
        
    }
}
