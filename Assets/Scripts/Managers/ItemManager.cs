using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

/* Manages all items in the game. Used to generate items dynamically */
public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;
    public float maxItemModBaseChance;      //chance that a non-unique item has 3 mods 
    public float itemModBonusChance;        //dungeon mod can increase chance for 3 mods
    [SerializeField] private LootTable masterLootTable;
    public LootTable lootTable;    //lootTable is instance of masterLootTable. masterLootTable is never accessed.

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
        maxItemModBaseChance = 0.15f;

        lootTable = Instantiate(masterLootTable);
       

        //sort all table items by weight in case they're not sorted already.
        SortTableItems(lootTable.itemTables);

        //testing dungeon mods
        //ActivateDungeonMods();

    }


    public void SortTableItems(List<Table> tables)
    {
        for (int i = 0; i < tables.Count; i++)
        {
            tables[i].item = tables[i].item.OrderByDescending(x => x.itemWeight).ToList();
        }
    }

    public void SortTableWeight(List<Table> tables)
    {
        //for (int i = 0; i < tables.Count; i++)
        //{
            lootTable.itemTables = tables.OrderByDescending(x => x.tableWeight).ToList();
        //}
    }

    public void GenerateMods(Weapon weapon)
    {
        /*  RULES FOR GENERATING MODS ON ITEM
         *  -----
         *  Item level must be at least 5
         *  The number of mods the item has is random from 0 to 3, but additional rules influence the number.
         *  If the item is unique, the mod count becomes 2.
         *  If one of the item mods is a chip slot, the mod count becomes 1.
         * 
         * */

        //get item level
        HunterManager hm = Singleton.instance.HunterManager;
        int averageLevel = 0;
        if (hm.hunters.Count > 0)
        {
            foreach (Hunter hunter in hm.hunters)
            {
                averageLevel += hunter.hunterLevel;
            }

            averageLevel /= hm.hunters.Count;
        }

        //weapon.itemLevel = averageLevel >= 5 ? averageLevel : 1;
        if (averageLevel >= 5)
        {
            weapon.itemLevel = averageLevel;
        }
        else
        {
            weapon.itemLevel = 1;
            weapon.modCount = 0;
        }

        //weapon's level must be at least 5 to have mods on it.
        string mods = "";
        if (weapon.itemLevel >= 5)        
        {
            //is item unique?
            if (weapon.isUniqueItem)
            {
                weapon.modCount = 2;
            }
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
            
            int j = 0;
            while (j < weapon.modCount)
            {
                ItemMod itemMod = im.GetItemMod();

                //if item mod is a chip slot, break the loop
                if (itemMod.isChipSlot)
                {
                    if (weapon.itemMods.Count <= 1 && !weapon.isUniqueItem)
                    {
                        weapon.itemMods.Add(itemMod);
                        weapon.hasChipSlot = true;
                        mods += string.Format("{0}\n", itemMod.modName);
                        j += 2;     //chip slot takes up 2 mod slots
                    }
                }
                else
                {
                    weapon.itemMods.Add(itemMod);
                    mods += string.Format("{0}\n", itemMod.modName);
                    j++;
                }
            }
        }
        else
        {
            weapon.modCount = 0;
        }

        //modify the name of item with "+" depending on mod count
        string rank = "";
        for (int i = 0; i < weapon.itemMods.Count; i++)
        {
            rank += "+";
        }
        weapon.itemName += rank;

        Debug.LogFormat("Weapon mods for {0}:\n{1}", weapon.itemName, mods);
        //return weapon;
    }

    public void GenerateMods(Armor armor)
    {
        //get item level
        HunterManager hm = Singleton.instance.HunterManager;
        int averageLevel = 0;
        if (hm.hunters.Count > 0)
        {
            foreach (Hunter hunter in hm.hunters)
            {
                averageLevel += hunter.hunterLevel;
            }

            averageLevel /= hm.hunters.Count;
        }

        //armor.itemLevel = averageLevel >= 5 ? averageLevel : 1;
        if (averageLevel >= 5)
        {
            armor.itemLevel = averageLevel;
        }
        else
        {
            armor.itemLevel = 1;
            armor.modCount = 0;
        }

        //armor's level must be at least 5 to have mods on it.
        string mods = "";
        if (armor.itemLevel >= 5)        
        {
            //is item unique?
            if (armor.isUniqueItem)
            {
                armor.modCount = 2;
            }
            else
            {
                //3 mods is rare
                if (Random.value <= maxItemModBaseChance + itemModBonusChance)
                {
                    armor.modCount = 3;
                }
                else
                {
                    armor.modCount = Random.Range(0, 3);
                }
            }


            //generate item mods
            ItemModManager im = Singleton.instance.ItemModManager;

            int j = 0;
            while (j < armor.modCount)
            {
                ItemMod itemMod = im.GetItemMod();

                //if item mod is a chip slot, break the loop
                if (itemMod.isChipSlot)
                {
                    if (armor.itemMods.Count <= 1 && !armor.isUniqueItem)
                    {
                        armor.itemMods.Add(itemMod);
                        armor.hasChipSlot = true;
                        mods += string.Format("{0}\n", itemMod.modName);
                        j += 2;     //chip slot takes up 2 mod slots
                    }
                }
                else
                {
                    armor.itemMods.Add(itemMod);
                    mods += string.Format("{0}\n", itemMod.modName);
                    j++;
                }
            }
        }
        else
        {
            armor.modCount = 0;
        }

        //modify the name of item with "+" depending on mod count
        string rank = "";
        for (int i = 0; i < armor.itemMods.Count; i++)
        {
            rank += "+";
        }
        armor.itemName += rank;

        Debug.LogFormat("armor mods for {0}:\n{1}", armor.itemName, mods);
    }

    public void GenerateMods(Accessory acc)
    {
        //get item level
        HunterManager hm = Singleton.instance.HunterManager;
        int averageLevel = 0;
        if (hm.hunters.Count > 0)
        {
            foreach (Hunter hunter in hm.hunters)
            {
                averageLevel += hunter.hunterLevel;
            }

            averageLevel /= hm.hunters.Count;
        }

        if (averageLevel >= 5)
        {
            acc.itemLevel = averageLevel;
        }
        else
        {
            acc.itemLevel = 1;
            acc.modCount = 0;
        }

        //acc's level must be at least 5 to have mods on it.
        string mods = "";
        if (acc.itemLevel >= 5)
        {
            //is item unique?
            if (acc.isUniqueItem)
            {
                acc.modCount = 2;
            }
            else
            {
                //3 mods is rare
                if (Random.value <= maxItemModBaseChance + itemModBonusChance)
                {
                    acc.modCount = 3;
                }
                else
                {
                    acc.modCount = Random.Range(0, 3);
                }
            }


            //generate item mods
            ItemModManager im = Singleton.instance.ItemModManager;

            int j = 0;
            while (j < acc.modCount)
            {
                ItemMod itemMod = im.GetItemMod();

                //if item mod is a chip slot, break the loop
                if (itemMod.isChipSlot)
                {
                    if (acc.itemMods.Count <= 1 && !acc.isUniqueItem)
                    {
                        acc.itemMods.Add(itemMod);
                        acc.hasChipSlot = true;
                        mods += string.Format("{0}\n", itemMod.modName);
                        j += 2;     //chip slot takes up 2 mod slots
                    }
                }
                else
                {
                    acc.itemMods.Add(itemMod);
                    mods += string.Format("{0}\n", itemMod.modName);
                    j++;
                }
            }
        }
        else
        {
            acc.modCount = 0;
        }

        //modify the name of item with "+" depending on mod count
        string rank = "";
        for (int i = 0; i < acc.itemMods.Count; i++)
        {
            rank += "+";
        }
        acc.itemName += rank;

        Debug.LogFormat("acc mods for {0}:\n{1}", acc.itemName, mods);

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

    //adds random item to given chest
    public void GenerateChestItem(Entity_TreasureChest chest)
    {
        if (chest == null)
        {
            Debug.Log("Chest doesn't exist!");
            return;
        }
        Table table = new Table();
        table = lootTable.GetTable();
        Item item = lootTable.GetItem(table.itemType);

        //if item can have mods on it, add them now
        if (item is Weapon wpn)
        {
            GenerateMods(wpn);
        }
        else if (item is Armor armor)
        {
            GenerateMods(armor);
        }
        else if (item is Accessory acc)
        {
            GenerateMods(acc);
        }

        chest.item = item;
    }

    //adds random item from a specific table. Can be a target item.
    public void GenerateChestItem(Entity_TreasureChest chest, Table.ItemType table, bool targetItem = false)
    {
        if (chest == null)
        {
            Debug.Log("Chest doesn't exist!");
            return;
        }
        
        Item item = lootTable.GetItem(table);

        //if item can have mods on it, add them now
        if (item is Weapon wpn)
        {
            GenerateMods(wpn);
        }
        else if (item is Armor armor)
        {
            GenerateMods(armor);
        }
        else if (item is Accessory acc)
        {
            GenerateMods(acc);
        }

        item.isTargetItem = targetItem;

        if (item.isTargetItem)
        {
            item.itemName = string.Format("{0} [TARGET]", item.itemName);
        }

        chest.item = item;
    }

    /// <summary>
    /// Adds a specific item from a specific table. Can be a target item
    /// </summary>
    /// <param name="chest">The entity that the item will be stored in.</param>
    /// <param name="table">The table category to search.</param>
    /// <param name="itemID">The name of the item to retrieve from the given table.</param>
    /// <param name="targetItem">If true, this item is required to complete the dungeon.</param>
    public void GenerateChestItem(Entity_TreasureChest chest, Table.ItemType table, string itemID, bool targetItem = false)
    {
        if (chest == null || itemID.Equals(""))
            return;

        Item item = lootTable.GetItem(table, itemID);

        //if item can have mods on it, add them now
        if (item is Weapon wpn)
        {
            GenerateMods(wpn);
            //SkillManager sm = Singleton.instance.SkillManager;
            //wpn.itemSkill = sm.AddSkill(sm.passiveSkillList, "passiveSkill_Stun");
        }
        else if (item is Armor armor)
        {
            GenerateMods(armor);
        }
        else if (item is Accessory acc)
        {
            GenerateMods(acc);
        }

        item.isTargetItem = targetItem;

        if (item.isTargetItem)
        {
            item.itemName = string.Format("{0} [TARGET]", item.itemName);
        }

        chest.item = item;
    }
}
