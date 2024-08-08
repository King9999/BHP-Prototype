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
    public int[] tableWeight;   //determines which category of items to access
    public List<LootItem> consumables;      //single-use items
    public List<LootItem> equipment;        //only contains equipment types since more checks have to be done.
    public List<LootItem> valuables;        //items that are sold
    public List<LootItem> dungeonMods;      //rarest items
    public List<LootItem> dataLogs;         //single player item only

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public int GetTable()
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

        return tableIndex;
        
    }

}

[Serializable]
public class LootItem
{
    public Item itemType;
    public int itemWeight;

    public enum ItemType
    {
        Consumable, Valuable, Weapon, Armor, Accessory, DungeonMod
    }
}
