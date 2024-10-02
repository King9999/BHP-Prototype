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
    public List<TableItem> dataLogs;         //single player item only
    //public List<Item> weapons;
    //public List<Item> armor;                /* These contain specific equipment */
    //public List<Item> accessories;
    public List<Table> itemTables;      // table 0 is valuables, 1 is consumables, 2 is weapons, 3 is armor

    //table indexes
    private const int VALUABLES = 0;
    private const int CONSUMABLES = 1;
    private const int EQUIPMENT = 2;
    private const int DUNGEON_MODS = 3;
    private const int DATA_LOGS = 4;

   
    public Table GetTable()
    {
        //sort the tables in case the weights were changed


        //check which table is going to be accessed
        int totalWeight = 0;
        //int tableIndex = 0;
        for (int i = 0; i < itemTables.Count; i++)
        {
            totalWeight += itemTables[i].tableWeight;
        }

        int randValue = UnityEngine.Random.Range(0, totalWeight);
        Debug.Log("Total Weight: " + totalWeight);
        Debug.Log("Init. Rand value: " + randValue);

        int j = 0;
        bool tableFound = false;

        while(!tableFound && j < itemTables.Count) 
        {
            if (randValue <= itemTables[j].tableWeight)
            {
                //create this item
                //tableIndex = j;
                tableFound = true;
                Debug.Log("Acessing table " + j + ", rand value is " + randValue);
            }
            else
            {
                randValue -= itemTables[j].tableWeight;
                Debug.Log("Rand value is now " + randValue);
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
    public Item GetItem(Table.ItemType itemType)    //the parameter is itemType in case the table list is sorted.
    {

        //find the table matching the type
        Table table = new Table();
        bool tableFound = false;
        int j = 0;
        while(!tableFound && j < itemTables.Count)
        {
            if (itemTables[j].itemType == itemType)
            {
                tableFound = true;
                table = itemTables[j];
            }
            else
            {
                j++;
            }
        }

        if (table.item.Count <= 0)
            return null;

        //get total weight of all items in the table
        int totalWeight = 0;
        for (int i = 0; i < table.item.Count; i++)
        {
            totalWeight += table.item[i].itemWeight;
        }

        //Debug.Log("---Getting random value from GetItem---");
        int randValue = UnityEngine.Random.Range(0, totalWeight);
        //Debug.Log("total weight: " + totalWeight);

        j = 0;
        bool itemFound = false;

        while (!itemFound && j < table.item.Count)
        {
            if (randValue <= table.item[j].itemWeight)
            {
                //create this item
                itemFound = true;
                //Debug.Log("Acessing item " + j + ", rand value is " + randValue);
            }
            else
            {
                randValue -= table.item[j].itemWeight;
                //Debug.Log("Rand value is now " + randValue);
                j++;
            }
        }

        if (itemFound)
        {
            Debug.Log("Generating random item " + table.item[j].item.itemName);
            return Instantiate(table.item[j].item);
        }
        else
            return null;

    }

    //gets a specific item from the given table
    public Item GetItem(Table.ItemType itemType, string itemID)
    {

        Table table = new Table();
        bool tableFound = false;
        int j = 0;
        while (!tableFound && j < itemTables.Count)
        {
            if (itemTables[j].itemType == itemType)
            {
                tableFound = true;
                table = itemTables[j];
            }
            else
            {
                j++;
            }
        }

        if (table.item.Count <= 0)
            return null;

        j = 0;
        bool itemFound = false;

        while (!itemFound && j < table.item.Count)
        {
            if (table.item[j].item.itemID.Equals(itemID))
            {
                //create this item
                itemFound = true;
                //Debug.Log("Acessing item " + j);
            }
            else
            {
                j++;
            }
        }

        if (itemFound)
        {
            Debug.Log("Generating specific item " + table.item[j].item.itemName);
            return Instantiate(table.item[j].item);
        }
        else
            return null;

    }

}

[Serializable]
public class TableItem
{
    public Item item;
    public int itemWeight;
    public int requiredLevel;
}

[Serializable]
public class Table
{
    public ItemType itemType;
    public int tableWeight;
    public List<TableItem> item;
    
    
    public enum ItemType
    {
        Valuable, Consumable, Weapon, Armor, Accessory, DungeonMod
    }
}
