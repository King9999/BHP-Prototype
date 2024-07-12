using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/* contains a list of all item mods in the game. item mod objects are instantiated from this list. */
public class ItemModManager : MonoBehaviour
{
    //public List<ItemMod> itemMods;
    public List<ItemMod> newMods;

    /* I'm going to have different level mods, which will all be contained in separate lists. */
    private int maxModLevel { get; } = 4;

    [Serializable]
    public struct ItemModTable
    {
        public List<ItemMod> itemMods;
    }

    public List<ItemModTable> itemModTables;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            ItemMod mod = GetItemMod(0);      //IMPORTANT: use Object.Instantiate to create new instances of scriptable objects!
            Debug.Log("New mod is " + mod.modName);
            newMods.Add(mod);
        }
    }

    public ItemMod GetItemMod(int modLevel)
    {
        if (modLevel < 0 || modLevel > maxModLevel) 
            return null;

        int randIndex = 0; //UnityEngine.Random.Range(0, itemModTables[0].itemMods.Count + 1);
        ItemMod mod = Instantiate(itemModTables[0].itemMods[randIndex]);
        return mod;
    }

}
