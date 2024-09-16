using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//makes treasure chests more likely to contain weapons.
[CreateAssetMenu(menuName = "Item/Dungeon Mod/More Weapons", fileName = "dungeonMod_moreWeapons")]
public class DungeonMod_MoreWeapons : DungeonMod
{
    private int weightMod;        //50% more weight
    private int originalWeight;
    private float creditsChanceMod = -0.3f;
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
        Dungeon dun = Singleton.instance.Dungeon;
        int weaponIndex = (int)Table.ItemType.Weapon;
        originalWeight = im.lootTable.itemTables[weaponIndex].tableWeight;
        Debug.Log("Orig. Weight " + originalWeight);
        weightMod = originalWeight * 3;       //200% more weight
        dun.creditsChanceMod = creditsChanceMod;
        im.lootTable.itemTables[weaponIndex].tableWeight = weightMod;
        Debug.Log("New Weight " + im.lootTable.itemTables[weaponIndex].tableWeight);
    }

    public override void DeactivateMod()
    {
        Dungeon dun = Singleton.instance.Dungeon;
        ItemManager im = Singleton.instance.ItemManager;
        int weaponIndex = (int)Table.ItemType.Weapon;
        im.lootTable.itemTables[weaponIndex].tableWeight = originalWeight;
        dun.creditsChanceMod = 0;
    }
}
