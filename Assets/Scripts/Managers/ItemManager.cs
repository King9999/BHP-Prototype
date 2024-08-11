using System.Collections.Generic;
using UnityEngine;

/* Manages all items in the game. Used to generate items dynamically */
public class ItemManager : MonoBehaviour
{
    public List<Weapon> masterWeaponList;
    public static ItemManager instance;
    public float maxItemModBaseChance;      //chance that a non-unique item has 3 mods 
    public float itemModBonusChance;        //dungeon mod can increase chance for 3 mods
    public LootTable masterLootTable, lootTable;    //lootTable is instance 

    [Header("---Dungeon Mods---")]
    public List<DungeonMod> dungeonMods;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        itemModBonusChance = 0;
        maxItemModBaseChance = 0.2f;

        lootTable = Instantiate(masterLootTable);

        //testing dungeon mods
        ActivateDungeonMods();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            List<LootItem> table = new List<LootItem>();
            table = lootTable.GetTable();
            Item item = lootTable.GetItem(table);
            if (item != null)
                Debug.Log("Generated " + item.itemName);
            else
                Debug.Log("No item found");
        }*/

        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach(DungeonMod mod in dungeonMods)
            {
                mod.DeactivateMod();
            }
        }*/
    }

    public Weapon GenerateWeapon()
    {
        Weapon weapon = Instantiate(masterWeaponList[0]);

        /*  RULES FOR GENERATING MODS ON ITEM
         *  -----
         *  Item level must be at least 5
         *  The number of mods the item has is random from 0 to 3, but additional rules influence the number.
         *  If the item is unique, the mod count becomes 2.
         *  If one of the item mods is a chip slot, the mod count becomes 1.
         * 
         * */

        //get item level
        GameManager gm = Singleton.instance.GameManager;
        int averageLevel = 0;
        if (gm.hunters.Count > 0)
        {
            foreach (Hunter hunter in gm.hunters)
            {
                averageLevel += hunter.hunterLevel;
            }

            averageLevel /= gm.hunters.Count;
        }

        weapon.itemLevel = averageLevel >= 5 ? averageLevel : 1;

        //is item unique?
        if (weapon.isUniqueItem)
        {
            weapon.modCount = 2;
            //add item's unique mod
        }
        /*else if (weapon.hasChipSlot)
        {
            weapon.modCount = Random.Range(0, 2);
        }*/
        else
        {
            //3 mods is rare
            if (Random.value <= maxItemModBaseChance + itemModBonusChance)
            {
                weapon.modCount = 3;
            }
            else
            {
                weapon.modCount = Random.Range(0, 3);
            }

            
        }

        //generate item mods
        ItemModManager im = Singleton.instance.ItemModManager;
        string mods = "";
        int j = 0;
        bool chipSlotFound = false;
        while(!chipSlotFound && j < weapon.modCount) 
        {
            ItemMod itemMod = im.GetItemMod(/*1*/);

            //if item mod is a chip slot, break the loop
            if (itemMod.isChipSlot)
            {
                if (weapon.itemMods.Count <= 1 && !weapon.isUniqueItem)
                {
                    weapon.itemMods.Add(itemMod);
                    mods += itemMod.modName + "\n";
                    chipSlotFound = true;
                }
            }
            else
            {
                weapon.itemMods.Add(itemMod);
                mods += itemMod.modName + "\n";
                j++;
            }
        }

        //modify the name of item with "+" depending on mod count
        string rank = "";
        for (int i = 0; i < weapon.itemMods.Count; i++)
        {
            rank += "+";
        }
        weapon.itemName += rank;

        Debug.Log("Weapon mods for " + weapon.itemName + ":\n" + mods);
        return weapon;
    }

    public void ActivateDungeonMods()
    {
        if (dungeonMods.Count <= 0)
            return;

        foreach (DungeonMod mod in dungeonMods)
        {
            mod.ActivateMod();
        }
    }
}
