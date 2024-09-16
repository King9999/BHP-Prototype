using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//makes treasure chests more likely to contain consumables.
[CreateAssetMenu(menuName = "Item/Dungeon Mod/More Consumables", fileName = "dungeonMod_moreConsumables")]
public class DungeonMod_MoreConsumables : DungeonMod
{
    private int weightMod;        //100% more weight
    private int originalWeight;
    private float creditsChanceMod = -0.3f;
    // Start is called before the first frame update
    void Reset()
    {
        itemName = "More Consumables";
        itemID = "dungeonMod_moreConsumables";
        details = "Treasure chests are more likely to contain consumables";
        itemType = ItemType.DunegonMod;
        price = 1000;
    }


    public override void ActivateMod()
    {
        //modify the weapon weight so that weapons are more common.
        ItemManager im = Singleton.instance.ItemManager;
        Dungeon dun = Singleton.instance.Dungeon;
        int conIndex = (int)Table.ItemType.Consumable;
        originalWeight = im.lootTable.itemTables[conIndex].tableWeight;
        dun.creditsChanceMod = creditsChanceMod;
        Debug.Log("Using More Consumables mod\n------\nOrig. Weight " + originalWeight);
        weightMod = originalWeight * 2;       //100% more weight
        im.lootTable.itemTables[conIndex].tableWeight = weightMod;
        Debug.Log("New Weight for table " + conIndex + ": " + im.lootTable.itemTables[conIndex].tableWeight);
    }

    public override void DeactivateMod()
    {
        ItemManager im = Singleton.instance.ItemManager;
        Dungeon dun = Singleton.instance.Dungeon;
        dun.creditsChanceMod = 0;
        int conIndex = (int)Table.ItemType.Consumable;
        im.lootTable.itemTables[conIndex].tableWeight = originalWeight;
    }
}
