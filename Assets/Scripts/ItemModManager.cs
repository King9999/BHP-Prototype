using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/* contains a list of all item mods in the game. item mod objects are instantiated from this list. */
public class ItemModManager : MonoBehaviour
{
    public List<ItemMod> itemMods;
    public List<ItemMod> newMods;
    // Start is called before the first frame update
    void Start()
    {
       
        ItemMod mod = GetItemMod(1);      //IMPORTANT: use Object.Instantiate to create new instances of scriptable objects!
        Debug.Log("New mod is " + mod.modName);
        newMods.Add(mod);
        
    }

    public ItemMod GetItemMod(int modLevel)
    {
        if (modLevel < 1 || modLevel > 5) 
            return null;

        int randIndex = Random.Range(0, itemMods.Count + 1);
        ItemMod mod = Object.Instantiate(itemMods[randIndex]);
        return mod;
    }

}
