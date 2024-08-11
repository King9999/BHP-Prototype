using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//makes treasure chests more likely to contain weapons.
[CreateAssetMenu(menuName = "Item/Dungeon Mod/More Weapons", fileName = "dungeonMod_moreWeapons")]
public class DungeonMod_MoreWeapons : DungeonMod
{
    private int weightMod;        //50% more weight
    private int originalWeight;
    // Start is called before the first frame update
    void Reset()
    {
        itemName = "More Weapons";
        itemID = "dungeonMod_moreWeapons";
        details = "Treasure chests are more likely to contain weapons";
        itemType = ItemType.DunegonMod;
        price = 1000;
    }


    public override void ActivateMod()
    {
        //modify the weapon weight so that weapons are more common.
        ItemManager im = Singleton.instance.ItemManager;
        originalWeight = im.lootTable.equipment[0].itemWeight;
        Debug.Log("Orig. Weight " + originalWeight);
        weightMod = (originalWeight * 3) / 2;       //50% more weight
        im.lootTable.equipment[0].itemWeight = weightMod;
        Debug.Log("New Weight " + im.lootTable.equipment[0].itemWeight);
    }

    public override void DeactivateMod()
    {
        ItemManager im = Singleton.instance.ItemManager;
        im.lootTable.equipment[0].itemWeight = originalWeight;
    }
}
