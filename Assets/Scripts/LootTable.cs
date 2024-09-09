using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This contains every item in the game. Items are divided into categories, and each category has a weight that determines
 * whether the game generates an item from the category. Valuables are the most common items, followed by consumables, equipment,
 * and then dungeon mods. This loot table only generates items, not the item mods. */
[CreateAssetMenu(menuName = "Loot Table", fileName = "masterLootTable")]
public class LootTable : ScriptableObject
{
    //public int[] tableWeight;               //determines which category of items to access
    //public List<LootItem> consumables;      //single-use items
    //public List<LootItem> equipment;        //only contains equipment types since more checks have to be done.
    //public List<LootItem> valuables;        //items that are sold
    //public List<LootItem> dungeonMods;      //rarest items
    public List<LootItem> dataLogs;         //single player item only
    //public List<Item> weapons;
    //public List<Item> armor;                /* These contain specific equipment */
    //public List<Item> accessories;
    public List<Table> itemTables;      // table 0 is weapons, 1 is armor, 2 is accessories

    //table indexes
    private const int VALUABLES = 0;
    private const int CONSUMABLES = 1;
    private const int EQUIPMENT = 2;
    private const int DUNGEON_MODS = 3;
    private const int DATA_LOGS = 4;

   
    public Table GetTable()
    {
        //check which table is going to be accessed
        int totalWeight = 0;
        int tableIndex = 0;
        for (int i = 0; i < itemTables.Count; i++)
        {
            totalWeight += itemTables[i].tableWeight;
        }

        int randValue = UnityEngine.Random.Range(0, totalWeight);
        //Debug.Log("Total Weight: " + totalWeight);

        int j = 0;
        bool tableFound = false;

        while(!tableFound && j < itemTables.Count) 
        //for (int i = 0; i < tableWeight.Length; i++)
        {
            if (randValue <= itemTables[j].tableWeight)
            {
                //create this item
                tableIndex = j;
                tableFound = true;
                Debug.Log("Acessing table " + j + ", rand value is " + randValue);
            }
            else
            {
                randValue -= itemTables[j].tableWeight;
                //Debug.Log("Rand value is now " + randValue);
                j++;
            }
        }

        //access list of items based on tableIndex
        /*Table chosenTable = new Table();
        switch (tableIndex)
        {
            case VALUABLES:
                chosenTable = valuables;
                break;

            case CONSUMABLES:
                chosenTable = consumables;
                break;

            case EQUIPMENT:
                chosenTable = equipment;
                break;

            case DUNGEON_MODS:
                chosenTable = dungeonMods;
                break;

            case DATA_LOGS:
                chosenTable = dataLogs;
                break;
        }*/

        return itemTables[j];
        
    }

    //gets a random item from the given table
    public Item GetItem(/*List<LootItem> table*/ Table table)
    {
        if (table.item.Count <= 0)
            return null;

        //get total weight of all items in the table
        int totalWeight = 0;
        for (int i = 0; i < table.item.Count; i++)
        {
            totalWeight += table.item[i].itemWeight;
        }

        Debug.Log("---Getting random value from GetItem---");
        int randValue = UnityEngine.Random.Range(0, totalWeight);
        Debug.Log("total weight: " + totalWeight);

        int j = 0;
        bool itemFound = false;

        while (!itemFound && j < table.item.Count)
        {
            if (randValue <= table.item[j].itemWeight)
            {
                //create this item
                itemFound = true;
                Debug.Log("Acessing item " + j + ", rand value is " + randValue);
            }
            else
            {
                randValue -= table.item[j].itemWeight;
                Debug.Log("Rand value is now " + randValue);
                j++;
            }
        }

        if (itemFound)
            return Instantiate(table.item[j].item);
        else
            return null;

    }

    //gets a specific item from the given table
    public Item GetItem(/*List<Item> table*/ Table table, string itemID)
    {
        if (table.item.Count <= 0)
            return null;

        int j = 0;
        bool itemFound = false;

        while (!itemFound && j < table.item.Count)
        {
            if (table.item[j].item.itemID.Equals(itemID))
            {
                //create this item
                itemFound = true;
                Debug.Log("Acessing item " + j);
            }
            else
            {
                j++;
            }
        }

        if (itemFound)
            return Instantiate(table.item[j].item);
        else
            return null;

    }

}

[Serializable]
public class LootItem
{
    public Item item;
    public int itemWeight;
}

[Serializable]
public class Table
{
    public ItemType itemType;
    public int tableWeight;
    public List<LootItem> item;
    
    
    public enum ItemType
    {
        Consumable, Valuable, Weapon, Armor, Accessory, DungeonMod
    }
}
