using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/* contains a list of all item mods in the game. item mod objects are instantiated from this list. Unique mods are not managed
 by this script, and instead are attached to the items themselves. */
public class ItemModManager : MonoBehaviour
{
    //public List<ItemMod> itemMods;
    public List<ItemMod> newMods;

    /* I'm going to have different level mods, which will all be contained in separate lists. */
    private int maxModLevel { get; } = 4;
    const int COMMON = 0;
    const int RARE = 1;
    const int SUPER_RARE = 2;

    [System.Serializable]
    public struct ItemModTable
    {
        public ModRarity rarity;
        public List<ItemMod> itemMods;
        public int weight;            //the chance that the game pulls a mod from a table.

        public enum ModRarity { Common, Rare, SuperRare }
    }

    public List<ItemModTable> itemModTable;             //3 tables, each with different rarity
    // Start is called before the first frame update
    void Start()
    {
        /*for (int i = 0; i < 100; i++)
        {
            ItemMod mod = GetItemMod(0);      //IMPORTANT: use Object.Instantiate to create new instances of scriptable objects!
            Debug.Log("New mod is " + mod.modName);
            newMods.Add(mod);
        }*/

        //get 
    }

    public ItemMod GetItemMod()
    {
        //GameManager gm = Singleton.instance.GameManager;
        //HunterManager hm = Singleton.instance.HunterManager;

        //if (modLevel < 0 || modLevel > maxModLevel) 
        //return null;

        //Roll to figure out which mod to get from the table. Each mod is categorized as common, rare, and super rare.
        //get total weight
        int totalWeight = 0;
        for (int i = 0; i < itemModTable.Count; i++)
        {
            totalWeight += itemModTable[i].weight;
        }

        //get random value and check result against the weight of each table.
        int roll = Random.Range(0, totalWeight);
        Debug.LogFormat("Rolled {0}", roll);

        int j = 0;
        bool modFound = false;
        while (!modFound && j < itemModTable.Count)
        {
            if (roll <= itemModTable[j].weight)
            {
                modFound = true;
            }
            else
            {
                roll -= itemModTable[j].weight;
                j++;
            }
        }

        /*if (roll <= itemModTable[SUPER_RARE].weight)
        {
            selectedTable = SUPER_RARE;
        }
        else if (roll <= itemModTable[RARE].weight)
        {
            selectedTable = RARE;
        }
        else
        {
            selectedTable = COMMON;
        }*/

        //select a random mod from the given table. the mod must check the equipment
        int randIndex = Random.Range(0, itemModTable[j].itemMods.Count);
        return Instantiate(itemModTable[j].itemMods[randIndex]);
           
    }

}
