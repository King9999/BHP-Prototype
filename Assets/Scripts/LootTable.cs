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
    public int[] tableWeight;               //determines which category of items to access
    public List<LootItem> consumables;      //single-use items
    public List<LootItem> equipment;        //only contains equipment types since more checks have to be done.
    public List<LootItem> valuables;        //items that are sold
    public List<LootItem> dungeonMods;      //rarest items
    public List<LootItem> dataLogs;         //single player item only

    //table indexes
    private const int VALUABLES = 0;
    private const int CONSUMABLES = 1;
    private const int EQUIPMENT = 2;
    private const int DUNGEON_MODS = 3;
    private const int DATA_LOGS = 4;

   
    public List<LootItem> GetTable()
    {
        //check which table is going to be accessed
        int totalWeight = 0;
        int tableIndex = 0;
        for (int i = 0; i < tableWeight.Length; i++)
        {
            totalWeight += tableWeight[i];
        }

        int randValue = UnityEngine.Random.Range(0, totalWeight);
        Debug.Log("randValue: " + randValue);

        int j = 0;
        bool tableFound = false;

        while(!tableFound && j < tableWeight.Length) 
        //for (int i = 0; i < tableWeight.Length; i++)
        {
            if (randValue <= tableWeight[j])
            {
                //create this item
                tableIndex = j;
                tableFound = true;
                Debug.Log("Acessing table " + j + ", rand value is " + randValue);
            }
            else
            {
                randValue -= tableWeight[j];
                Debug.Log("Rand value is now " + randValue);
                j++;
            }
        }

        //access list of items based on tableIndex
        List<LootItem> chosenTable = new List<LootItem>();
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
        }

        return chosenTable;
        
    }

    public Item GetItem(List<LootItem> table)
    {
        if (table.Count <= 0)
            return null;

        //get total weight of all items in the table
        int totalWeight = 0;
        for (int i = 0; i < table.Count; i++)
        {
            totalWeight += table[i].itemWeight;
        }

        Debug.Log("---Getting random value from GetItem---");
        int randValue = UnityEngine.Random.Range(0, totalWeight);
        Debug.Log("randValue: " + randValue);

        int j = 0;
        bool itemFound = false;
        int itemIndex = 0;

        while (!itemFound && j < table.Count)
        {
            if (randValue <= table[j].itemWeight)
            {
                //create this item
                itemIndex = j;
                itemFound = true;
                Debug.Log("Acessing item " + j + ", rand value is " + randValue);
            }
            else
            {
                randValue -= table[j].itemWeight;
                Debug.Log("Rand value is now " + randValue);
                j++;
            }
        }

        if (itemFound)
            return table[itemIndex].item;
        else
            return null;

    }

}

[Serializable]
public class LootItem
{
    public Item item;
    public int itemWeight;

    public enum ItemType
    {
        Consumable, Valuable, Weapon, Armor, Accessory, DungeonMod
    }
}
