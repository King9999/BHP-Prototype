using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.XR;
using static UnityEditor.Progress;
using JetBrains.Annotations;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;

/* This script handles hunter creation. The UI for hunter setup is here. */
public class HunterManager : MonoBehaviour
{
    public HunterUI ui;
    public List<Hunter> hunters;
    public Hunter hunterPrefab;
    public ActiveSkill basicAttackSkill;          //when a hunter is created, they get an attack skill that must be instantiated
    private int maxHunters { get; } = 4;
    public int currentHunter;               //iterator to track current hunter's turn
    private int allocationPoint { get; } = 1;
    public int startingAllocationPoints;
    public bool newHunter;  //if true, hunter was just created. The starting AP don't raise hunter level, so no bonus HP/SP
    public int rivalCount;  //used to generate CPU hunters during dungeon generation.

    public enum MenuState { NameEntry, PointAlloc, ChooseWeapon, RivalHunter, ShowHunterHuds }
    public MenuState state;

    public enum HunterMenuState { Default, SelectCard, CardDetails, RollDiceToMove, SelectMoveTile, SelectSkillTile, Rest, ActionSubmenu, Inventory, 
        InventoryItemDetails, SkillMenu, SkillDetails, ChooseAttackTarget }
    public HunterMenuState hunterMenuState;

    GameObject hunterContainer;                     //hunters are stored here for organization
    [Header("---CPU Hunters----")]
    public List<Hunter_AI> hunterBehaviours;        //used by CPU Hunters
    public float itemChanceMod;                     //used by dungeon mods to influence odds of CPU hunters carrying items.
    public enum HunterAIState { Moving, UseSkill }  //Moving = looking for a space to move to. Will look for points of interest.
                                                    //UseSkill = use a skill if CPU found a valid target during the Moving state.
    public HunterAIState aiState;

    public static HunterManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Singleton.instance.HunterManager = this;

        //create container for hunters
        hunterContainer = new GameObject();
        hunterContainer.transform.SetParent(this.transform);
        hunterContainer.name = "Hunters";
        transform.SetParent(Singleton.instance.transform);
        ui.transform.SetParent(GetComponentInChildren<Canvas>().transform);     //Hunter UI must persist when scene changes.
        //DontDestroyOnLoad(instance);
    }
    // Start is called before the first frame update
    void Start()
    {
        //Singleton.instance.HunterManager = this;
        //state = MenuState.PointAlloc;
        ChangeState(state = MenuState.NameEntry);
        CreateHunter();
    }

    //this is used to enable hunters when ready to be placed in the dungeon.
    public void ToggleHunter(Hunter hunter, bool toggle)
    {
        hunter.gameObject.SetActive(toggle);
    }


    public void ChangeState(MenuState state)
    {
        switch(state)
        {
            case MenuState.NameEntry:
                ui.ShowNameEntryMenu(true);
                ui.ShowPointAllocationMenu(false);
                ui.ShowWeaponSelectionMenu(false);
                ui.ShowRivalHunterMenu(false);
                ui.ShowHunterHuds(false);
                ui.ShowHunterMenuContainer(false);
                break;

            case MenuState.PointAlloc:
                ui.ShowNameEntryMenu(false);
                ui.ShowPointAllocationMenu(true);
                ui.ShowWeaponSelectionMenu(false);
                break;

            case MenuState.ChooseWeapon:
                ui.ShowPointAllocationMenu(false);
                ui.ShowWeaponSelectionMenu(true);
                ui.ShowRivalHunterMenu(false);
                break;

            case MenuState.RivalHunter:
                ui.ShowWeaponSelectionMenu(false);
                ui.ShowRivalHunterMenu(true);
                break;

            case MenuState.ShowHunterHuds:
                ui.ShowRivalHunterMenu(false);
                ui.ShowHunterHuds(true);
                break;
        }
    }

    public void ChangeHunterMenuState(HunterMenuState state)
    {
        GameManager gm = Singleton.instance.GameManager;
        switch(state)
        {
            case HunterMenuState.Default:
                ui.ShowHunterMenuContainer(true);
                ui.ShowHunterMenu_Main(true, gm.ActiveCharacter());
                //hide all other UI
                ui.ShowHunterMenu_DisplayCards(false);
                ui.ShowHunterMenu_RollDiceToMove(false);
                ui.ShowHunterMenu_ActionSubmenu(false);
                ui.ShowInventory(false);
                ui.ShowSkillsMenu(false);
                ui.ShowSelectingTargetMenu(false);
                //ui.ShowCardsMenu(false);
                ui.ShowCardDetails(false);
                break;

            case HunterMenuState.SelectCard:
                ui.ShowHunterMenu_Main(false);
                ui.ShowHunterMenu_DisplayCards(true, gm.ActiveCharacter());
                //ui.ShowCardsMenu(true, gm.ActiveCharacter());
                gm.dice.ShowSingleDieUI(false);
                ui.ShowHunterMenu_RollDiceToMove(false);
                ui.ShowCardDetails(false);
                break;

            case HunterMenuState.CardDetails:
                ui.ShowCardDetails(true);
                break;

            case HunterMenuState.RollDiceToMove:
                gm.dice.ShowSingleDieUI(true);
                ui.ShowHunterMenu_RollDiceToMove(true, gm.ActiveCharacter());
                ui.ShowHunterMenu_Main(false);
                ui.ShowHunterMenu_DisplayCards(false);
                break;

            case HunterMenuState.SelectMoveTile:
                ui.ShowHunterMenu_RollDiceToMove(false);
                break;

            case HunterMenuState.ActionSubmenu:
                ui.ShowHunterMenu_Main(false);
                ui.ShowHunterMenu_ActionSubmenu(true, gm.ActiveCharacter());
                ui.ShowInventory(false);
                ui.ShowSkillsMenu(false);
                break;

            case HunterMenuState.Inventory:
                ui.ShowInventory(true);
                ui.ShowHunterMenu_ActionSubmenu(false);
                ui.ShowDetailsWindow(false);
                break;

            case HunterMenuState.InventoryItemDetails:
                ui.ShowDetailsWindow(true);
                break;

            case HunterMenuState.SkillMenu:
                ui.ShowSkillsMenu(true);
                ui.ShowHunterMenu_ActionSubmenu(false);
                ui.ShowSkillDetails(false);
                ui.ShowSelectingTargetMenu(false);
                break;

            case HunterMenuState.SkillDetails:
                ui.ShowSkillDetails(true);
                break;

            case HunterMenuState.SelectSkillTile:
                ui.ShowSkillsMenu(false);
                ui.ShowSkillDetails(false);
                ui.ShowSelectingTargetMenu(true, gm.ActiveCharacter());
                break;


        }
    }

    

    

    public void CreateHunter()
    {
        if (hunters.Count >= maxHunters)
            return;

        Hunter hunter = Instantiate(hunterPrefab);
        hunter.transform.SetParent(hunterContainer.transform);     //doing this will make the hunter object persist when scene changes.
        //hunter.characterName = "King";
        //hunter.name = "Test Hunter";
        hunter.InitializeStats();
        newHunter = true;
        int newestHunter = hunters.Count;   //the new hunter will be added to the end of the list.
        ui.hunterStrText.text = hunter.str.ToString();
        ui.hunterSpdText.text = hunter.spd.ToString();
        ui.hunterVitText.text = hunter.vit.ToString();
        ui.hunterMntText.text = hunter.mnt.ToString();
        ui.hunterHuds[newestHunter].hunterNameText.text = hunter.characterName.ToString();
        ui.hunterAtpText.text = hunter.atp.ToString();
        ui.hunterDfpText.text = hunter.dfp.ToString();
        ui.hunterMnpText.text = hunter.mnp.ToString();
        ui.hunterRstText.text = hunter.rst.ToString();
        ui.hunterEvdText.text = (hunter.evd * 100) + "%";
        ui.hunterMovText.text = hunter.mov.ToString();
        ui.hunterHpText.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        ui.hunterSpText.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
        //ui.hunterHuds[newestHunter].hunterHpText.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        //ui.hunterHuds[newestHunter].hunterSpText.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;

        //point allocation values
        ui.strPointsText.text = hunter.strPoints.ToString();
        ui.vitPointsText.text = hunter.vitPoints.ToString();
        ui.mntPointsText.text = hunter.mntPoints.ToString();
        ui.spdPointsText.text = hunter.spdPoints.ToString();
        startingAllocationPoints = 16;
        ui.allocationPointsText.text = startingAllocationPoints + " Allocation Points Remaining";

        //testing out cards
        //CardManager cm = Singleton.instance.CardManager;
        //cm.DrawCard(hunter, cm.deck, 3);

        

        //give hunter a weapon
        /*ItemManager im = ItemManager.instance;
        hunter.Equip(im.GenerateWeapon());
        ui.equippedWeaponText.text = hunter.equippedWeapon.itemName;
        ui.hunterAtpText.text = hunter.atp.ToString();*/
        ToggleHunter(hunter, false);    //disable hunter for now
        hunters.Add(hunter);
        //hunters[0].inventory.Add(im.GenerateWeapon());  //adding weapon as a test
    }

    #region CPU Hunter code
    //********create a hunter based on the given level.*********/
    struct Stats
    {
        public enum StatType { Str, Vit, Mnt, Spd }
        public StatType statType;
        public int weight;    //the likelihood the stat will be rolled.       
    }
    public Hunter CreateCPUHunter(int hunterLevel)
    {
        
        Hunter hunter = Instantiate(hunterPrefab);
        hunter.transform.SetParent(hunterContainer.transform);
        hunter.cpuControlled = true;
        hunter.InitializeStats();

        //get a behaviour, which will determine stat growth.
        int randBehaviour = Random.Range(0, hunterBehaviours.Count);
        hunter.cpuBehaviour = Instantiate(hunterBehaviours[randBehaviour]);

        //generate stats. First use the starting allocation points.
        List<Stats> stats = new List<Stats>();


        //add STR
        Stats str = new Stats();
        str.statType = Stats.StatType.Str;
        str.weight = hunter.cpuBehaviour.rollStr;
        stats.Add(str);

        //add VIT
        Stats vit = new Stats();
        vit.statType = Stats.StatType.Vit;
        vit.weight = hunter.cpuBehaviour.rollVit;
        stats.Add(vit);

        //add MNT
        Stats mnt = new Stats();
        mnt.statType = Stats.StatType.Mnt;
        mnt.weight = hunter.cpuBehaviour.rollMnt;
        stats.Add(mnt);

        //add SPD
        Stats spd = new Stats();
        spd.statType = Stats.StatType.Spd;
        spd.weight = hunter.cpuBehaviour.rollSpd;
        stats.Add(spd);

        //sort list by highest value first
        stats = stats.OrderByDescending(x => x.weight).ToList();
        /*Debug.Log("Stats Order for " + hunter.cpuBehaviour.behaviourType + ":\n");
        foreach(Stats stat in  stats)
        {
            Debug.Log(stat.statType + ": " + stat.weight + "\n");
        }*/

        //get total weight.
        int totalWeight = 0;
        for (int i = 0; i < stats.Count; i++)
        {
            totalWeight += stats[i].weight;
        }

        for (int i = 0; i < hunter.startingAllocationPoints; i++)
        {
            int randWeight = Random.Range(0, totalWeight);
            bool statFound = false;
            int j = 0;
            while (!statFound && j < stats.Count)
            {
                if (randWeight <= stats[j].weight)
                {
                    statFound = true;
                    //get which stat this is and allocate point to it
                    switch (stats[j].statType)
                    {
                        case Stats.StatType.Str:
                            hunter.AllocateToStr(1, true);
                            break;

                        case Stats.StatType.Vit:
                            hunter.AllocateToVit(1, true);
                            break;

                        case Stats.StatType.Mnt:
                            hunter.AllocateToMnt(1, true);
                            break;

                        case Stats.StatType.Spd:
                            hunter.AllocateToSpd(1, true);
                            break;
                    }
                }
                else
                {
                    randWeight -= stats[j].weight;
                    j++;
                }
            }
           

        }

        //raise stats up to hunter level
        for (int i = 0; i < hunterLevel; i++)
        {
            int randWeight = Random.Range(0, totalWeight);
            bool statFound = false;
            int j = 0;
            while (!statFound && j < stats.Count)
            {
                if (randWeight <= stats[j].weight)
                {
                    statFound = true;
                    //get which stat this is and allocate point to it
                    switch (stats[j].statType)
                    {
                        case Stats.StatType.Str:
                            hunter.AllocateToStr(1, false);
                            break;

                        case Stats.StatType.Vit:
                            hunter.AllocateToVit(1, false);
                            break;

                        case Stats.StatType.Mnt:
                            hunter.AllocateToMnt(1, false);
                            break;

                        case Stats.StatType.Spd:
                            hunter.AllocateToSpd(1, false);
                            break;
                    }
                }
                else
                {
                    randWeight -= stats[j].weight;
                    j++;
                }
            }
        }

        /*Debug.Log("CPU Hunter Stats");
        Debug.Log("STR: " + hunter.str);
        Debug.Log("VIT: " + hunter.vit);
        Debug.Log("MNT: " + hunter.mnt);
        Debug.Log("SPD: " + hunter.spd);*/

        //add basic attack skill. NOTE: this skill must always be added before equipping a weapon.
        AddBasicAttackSkill(hunter);

        //get random gear to equip. CPU Hunters always have a weapon. Certain behaviours have specific weapon types
        ItemManager im = Singleton.instance.ItemManager;
        Item item = null;
        if (hunter.cpuBehaviour.behaviourType == Hunter_AI.BehaviourType.Mage)
        {
            item = im.lootTable.GetItem(Table.ItemType.Weapon, "weapon_augmenter");
        }
        else if (hunter.cpuBehaviour.behaviourType == Hunter_AI.BehaviourType.Aggro)
        {
            item = im.lootTable.GetItem(Table.ItemType.Weapon, "weapon_beamSword");
        }
        else
        {
            item = im.lootTable.GetItem(Table.ItemType.Weapon);
        }

        if (item is Weapon wpn)
        {
            im.GenerateMods(wpn);
            //wpn.hasChipSlot = true;
            //if item has a chip slot, then get a skill
            if (wpn.hasChipSlot && wpn.itemSkill == null)
            {
                SkillManager sm = Singleton.instance.SkillManager;
                bool skillFound = false;
                while (!skillFound)
                {
                    if (Random.value <= 0.5f)
                        wpn.itemSkill = sm.AddSkill(sm.activeSkillList);
                    else
                        wpn.itemSkill = sm.AddSkill(sm.passiveSkillList);

                    //check if the skill added is compatible with the weapon
                    if (wpn.weaponType == Weapon.WeaponType.BeamSword && (wpn.itemSkill.weaponRestriction == Skill.WeaponRestriction.None
                        || wpn.itemSkill.weaponRestriction == Skill.WeaponRestriction.BeamSword))
                    {
                        skillFound = true;
                    }

                    else if (wpn.weaponType == Weapon.WeaponType.Railgun && (wpn.itemSkill.weaponRestriction == Skill.WeaponRestriction.None
                        || wpn.itemSkill.weaponRestriction == Skill.WeaponRestriction.Gun))
                    {
                        skillFound = true;
                    }

                    else if (wpn.weaponType == Weapon.WeaponType.Augmenter && (wpn.itemSkill.weaponRestriction == Skill.WeaponRestriction.None
                        || wpn.itemSkill.weaponRestriction == Skill.WeaponRestriction.Augmenter))
                    {
                        skillFound = true;
                    }
                }
                //find the item mod with <CHIP SLOT> and replace with skill name
                bool modFound = false;
                int i = 0;
                while(!modFound && i < wpn.itemMods.Count)
                {
                    if (wpn.itemMods[i].modID.ToLower().Equals("itemMod_ChipSlot".ToLower()))
                    {
                        modFound = true;
                        wpn.itemMods[i].modName = "[" + wpn.itemSkill.skillType + " skill] " + wpn.itemSkill.skillName;
                    }
                    else
                        i++;
                }

            }

            hunter.Equip(wpn);
        }

        //armor check
        if (Random.value <= 0.5f)
        {
            Item newArmor = im.lootTable.GetItem(Table.ItemType.Armor);
            if (newArmor is Armor armor)
            {
                im.GenerateMods(armor);

                //if item has a chip slot, then get a skill
                if (armor.hasChipSlot && armor.itemSkill == null)
                {
                    SkillManager sm = Singleton.instance.SkillManager;
                    
                    if (Random.value <= 0.5f)
                        armor.itemSkill = sm.AddSkill(sm.activeSkillList);
                    else
                        armor.itemSkill = sm.AddSkill(sm.passiveSkillList);
                    /* NOTE: It'll be possible for armor to get a skill that can only be used by specific weapon types.
                     * This should be OK, as it just means that the hunter will need to have the correct weapon equipped to use the skill.
                     */
                    //find the item mod with <CHIP SLOT> and replace with skill name
                    bool modFound = false;
                    int i = 0;
                    while (!modFound && i < armor.itemMods.Count)
                    {
                        if (armor.itemMods[i].modID.ToLower().Equals("itemMod_ChipSlot".ToLower()))
                        {
                            modFound = true;
                            armor.itemMods[i].modName = "[" + armor.itemSkill.skillType + " skill] " + armor.itemSkill.skillName;
                        }
                        else
                            i++;
                    }
                }

                hunter.Equip(armor);
            }
        }

        //accessory check
        if (Random.value <= 0.5f)
        {
            Item newAcc = im.lootTable.GetItem(Table.ItemType.Accessory);
            if (newAcc is Accessory acc)
            {
                im.GenerateMods(acc);

                //if item has a chip slot, then get a skill
                if (acc.hasChipSlot && acc.itemSkill == null)
                {
                    SkillManager sm = Singleton.instance.SkillManager;

                    if (Random.value <= 0.5f)
                        acc.itemSkill = sm.AddSkill(sm.activeSkillList);
                    else
                        acc.itemSkill = sm.AddSkill(sm.passiveSkillList);

                    //find the item mod with <CHIP SLOT> and replace with skill name
                    bool modFound = false;
                    int i = 0;
                    while (!modFound && i < acc.itemMods.Count)
                    {
                        if (acc.itemMods[i].modID.ToLower().Equals("itemMod_ChipSlot".ToLower()))
                        {
                            modFound = true;
                            acc.itemMods[i].modName = "[" + acc.itemSkill.skillType + " skill] " + acc.itemSkill.skillName;
                        }
                        else
                            i++;
                    }
                }

                hunter.Equip(acc);
            }
        }

        //determine if hunter carries any items in inventory. These can be taken by other hunters.
        //Chance to have items = (CPU Hunter level x 0.01) / 2 + any dungeon modifiers that influence the probability
        //Number of items = 60 % chance for 1 item, 35 % chance for 2 items, 5 % chance for 3 items.
        float itemChance = hunterLevel * 0.01f / 2 * (1 + itemChanceMod);
        Debug.Log("Chance for CPU Hunter to have items: " +  itemChance);
        int itemCount = 0;
        if (Random.value <= itemChance)
        {
            float prob = Random.value;
            if (prob <= 0.05f)
            {
                itemCount = 3;
            }
            else if (prob <= 0.35f)
                itemCount = 2;
            else
                itemCount = 1;
        }
        Debug.Log("Number of items CPU Hunter will carry: " + itemCount);

        //Test purposes only
        hunter.spd = 0;

        //give hunter a random name TODO: Get a name from a file.
        hunter.characterName = "CPU " + hunters.Count;
        hunter.name = "Hunter - " + hunter.characterName;
        hunter.healthPoints = hunter.maxHealthPoints;
        hunter.skillPoints = hunter.maxSkillPoints;
        return hunter;
    }

    //CPU-controlled actions. Uses weight to determine priorities.
    public void ChangeCPUHunterState(HunterAIState aiState, Hunter hunter)
    {
        GameManager gm = Singleton.instance.GameManager;

        switch(aiState)
        {
            case HunterAIState.Moving:
                StartCoroutine(MoveCPUHunter(hunter));
                break;

            case HunterAIState.UseSkill:
                break;
        }
    }

    /* Executes all actions for moving a hunter. Includes choosing a card, rolling dice, and moving hunter to new space. */
    IEnumerator MoveCPUHunter(Hunter hunter)
    {
        //decide whether to use a card. Likelihood increases depending on behaviour.
        GameManager gm = Singleton.instance.GameManager;
        bool cardPicked = false;
        int i = 0;
        Card card = null;
        while (!cardPicked && i < hunter.cards.Count)
        {
            card = hunter.cpuBehaviour.ChooseCard_Field(hunter);

            if (card != null)
                cardPicked = true;
            else
                i++;

            yield return null;
        }
    }

    #endregion

    //adds basic attack skill and takes on properties of equipped weapon
    void AddBasicAttackSkill(Hunter hunter)
    {
        ActiveSkill attackSkill = Instantiate(basicAttackSkill);
        attackSkill.minRange = 0;
        attackSkill.maxRange = 1;
        hunter.skills.Add(attackSkill);
    }

    #region Point Allocation
    //Allocates a point to STR when clicked
    void AllocatePoint_STR(Hunter hunter, int amount)
    {
        //NOTE: second condition is minus because if amount is -1, a point is being refunded since two negatives = positive
        //if (startingAllocationPoints <= 0 || startingAllocationPoints - amount > 16)    
            //return;

        startingAllocationPoints -= amount;
        hunter.AllocateToStr(amount, newHunter);
        ui.hunterStrText.text = hunter.str.ToString();
        ui.strPointsText.text = hunter.strPoints.ToString();
        ui.hunterAtpText.text = hunter.atp.ToString();
        ui.hunterHpText.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        ui.hunterSpText.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;

        
        ui.allocationPointsText.text = startingAllocationPoints + " Allocation Points Remaining";
    }

    
    void AllocatePoint_SPD(Hunter hunter, int amount)
    {
        //if (startingAllocationPoints <= 0 || startingAllocationPoints - amount > 16)
            //return;

        startingAllocationPoints -= amount;
        hunter.AllocateToSpd(amount, newHunter);
        ui.hunterSpdText.text = hunter.spd.ToString();
        ui.spdPointsText.text = hunter.spdPoints.ToString();
        ui.hunterMovText.text = hunter.mov.ToString();
        ui.hunterEvdText.text = (hunter.evd * 100) + "%";
        ui.hunterHpText.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        ui.hunterSpText.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;

        ui.allocationPointsText.text = startingAllocationPoints + " Allocation Points Remaining";
    }

    void AllocatePoint_VIT(Hunter hunter, int amount)
    {
        //if (startingAllocationPoints <= 0 || startingAllocationPoints - amount > 16)
            //return;

        startingAllocationPoints -= amount;
        hunter.AllocateToVit(amount, newHunter);
        ui.hunterVitText.text = hunter.vit.ToString();
        ui.vitPointsText.text = hunter.vitPoints.ToString();
        ui.hunterDfpText.text = hunter.dfp.ToString();
        ui.hunterHpText.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        ui.hunterSpText.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;

        ui.allocationPointsText.text = startingAllocationPoints + " Allocation Points Remaining";
    }

    void AllocatePoint_MNT(Hunter hunter, int amount)
    {
        //if (startingAllocationPoints <= 0 || startingAllocationPoints - amount > 16)
            //return;
        startingAllocationPoints -= amount;
        hunter.AllocateToMnt(amount, newHunter);
        ui.hunterMntText.text = hunter.mnt.ToString();
        ui.mntPointsText.text = hunter.mntPoints.ToString();
        ui.hunterRstText.text = hunter.rst.ToString();
        ui.hunterMnpText.text = hunter.mnp.ToString();
        ui.hunterHpText.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        ui.hunterSpText.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;

        ui.allocationPointsText.text = startingAllocationPoints + " Allocation Points Remaining";
    }

    #endregion

    #region Button Methods
    public void OnAllocateStrButtonPressed()
    {
        if (startingAllocationPoints <= 0)
            return;
        AllocatePoint_STR(hunters[currentHunter], allocationPoint);
    }

    public void OnAllocateVitButtonPressed()
    {
        if (startingAllocationPoints <= 0)
            return;
        AllocatePoint_VIT(hunters[currentHunter], allocationPoint);
    }

    public void OnAllocateSpdButtonPressed()
    {
        if (startingAllocationPoints <= 0)
            return;
        AllocatePoint_SPD(hunters[currentHunter], allocationPoint);
    }

    public void OnAllocateMntButtonPressed()
    {
        if (startingAllocationPoints <= 0)
            return;
        AllocatePoint_MNT(hunters[currentHunter], allocationPoint);
    }

    public void OnDeallocateStrButtonPressed()
    {
        if (hunters[currentHunter].strPoints <= 0 || startingAllocationPoints >= 16)
            return;
        AllocatePoint_STR(hunters[currentHunter], -allocationPoint);
    }

    public void OnDeallocateVitButtonPressed()
    {
        if (hunters[currentHunter].vitPoints <= 0 || startingAllocationPoints >= 16)
            return;
        AllocatePoint_VIT(hunters[currentHunter], -allocationPoint);
    }

    public void OnDeallocateSpdButtonPressed()
    {
        if (hunters[currentHunter].spdPoints <= 0 || startingAllocationPoints >= 16)
            return;
        AllocatePoint_SPD(hunters[currentHunter], -allocationPoint);
    }

    public void OnDeallocateMntButtonPressed()
    {
        if (hunters[currentHunter].mntPoints <= 0 || startingAllocationPoints >= 16)
            return;
        AllocatePoint_MNT(hunters[currentHunter], -allocationPoint);
    }

    public void OnEquipmentSelectButtonPressed()
    {
        //state = MenuState.ChooseWeapon;
        ChangeState(state = MenuState.ChooseWeapon);
    }

    public void OnAllocationButtonPressed()
    {
        if (state == HunterManager.MenuState.NameEntry)
        {
            if(ui.nameEntryField.text == "")
            {
                Debug.Log("Name cannot be empty!");
                return;
            }
            else
            {
                hunters[hunters.Count - 1].characterName = ui.nameEntryField.text;
                hunters[hunters.Count - 1].name = "Hunter - " + ui.nameEntryField.text; //object name
            }
        }
            
        ChangeState(state = MenuState.PointAlloc);
    }

    public void OnNameEntryButtonPressed()
    {
        ChangeState(state = MenuState.NameEntry);
    }

    public void OnFinishHunterButtonPressed()
    {
        //create an instance of the weapon the player chose
        /*ItemManager im = Singleton.instance.ItemManager;
        Item item = null;

        switch (ui.WeaponDropdownValue())
        {
            case 0:
                item = im.lootTable.GetItem(im.lootTable.weapons, "weapon_beamSword");
                Debug.Log("Item is " + item);
                break;

            case 1:
                item = im.lootTable.GetItem(im.lootTable.weapons, "weapon_railGun");
                Debug.Log("Item is " + item);
                break;

            case 2:
                item = im.lootTable.GetItem(im.lootTable.weapons, "weapon_augmenter");
                Debug.Log("Item is " + item);
                break;
        }

        hunters[hunters.Count - 1].Equip((Weapon)item);*/

        //ui.ShowWeaponSelectionMenu(false);
        /*item = im.lootTable.GetItem(im.lootTable.consumables); //only has medispray
        if (item is Consumable medispray)       //This is how to get the type of a scriptable object!
        {
            medispray.ActivateEffect(hunters[hunters.Count - 1]);
        }*/

        //move to game scene
        //SceneManager.LoadScene("Game");
        ChangeState(state = MenuState.RivalHunter);
    }

    public void OnFinishRivalHunterButtonPressed()
    {

        //equip the selected weapon here
        //create an instance of the weapon the player chose
        ItemManager im = Singleton.instance.ItemManager;
        Item item = null;
        //int weaponIndex = (int)Table.ItemType.Weapon;
        switch (ui.WeaponDropdownValue())
        {
            case 0:
                item = im.lootTable.GetItem(Table.ItemType.Weapon, "weapon_beamSword");
                Debug.Log("Item is " + item);
                break;

            case 1:
                item = im.lootTable.GetItem(Table.ItemType.Weapon, "weapon_railGun");
                Debug.Log("Item is " + item);
                break;

            case 2:
                item = im.lootTable.GetItem(Table.ItemType.Weapon, "weapon_augmenter");
                Debug.Log("Item is " + item);
                break;
        }
        Hunter hunter = hunters[hunters.Count - 1];

        //add basic attack skill to hunter.
        AddBasicAttackSkill(hunter);

        hunter.Equip((Weapon)item);
        newHunter = false;      //level will start going up now when allocating points

        //testing status effects
        //EffectManager em = Singleton.instance.EffectManager;
        //em.AddEffect(StatusEffect.Effect.Injured, hunter);
        //em.AddEffect(StatusEffect.Effect.Injured, hunter);

        //testing skill chip
        /*for (int i = 0; i < 3; i++)
        {
            item = im.lootTable.GetItem(Table.ItemType.SkillChip);
            hunter.inventory.Add(item);
        }*/

        //save the number of rivals, it will be needed during dungeon generation.
        rivalCount = ui.RivalDropdownValue();
        Debug.Log("rival count: " + rivalCount);

        ChangeState(state = MenuState.ShowHunterHuds);
        //move to game scene
        SceneManager.LoadScene("Game");
    }

    //move button in hunter menu. Allows selecting card, and then rolling dice to move.
    public void OnMoveButtonPressed()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (gm.characterMoved)
            return;

        //gm.dice.ShowSingleDieUI(true);
        ChangeHunterMenuState(hunterMenuState = HunterMenuState.SelectCard);    //TODO: change to SelectCard when ready

    }

    //show a sub-menu to attack or use an item
    public void OnActionButtonPressed()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (gm.characterActed)
            return;

        //show submenu here
        ChangeHunterMenuState(hunterMenuState = HunterMenuState.ActionSubmenu);
    }

    public void OnSelectCardButtonPressed()
    {
        //cannot proceed if no card was selected.
        CardManager cm = Singleton.instance.CardManager;
        if (cm.selectedCard == null)
            return;

        ChangeHunterMenuState(hunterMenuState = HunterMenuState.RollDiceToMove);
    }

    public void OnSkipCardButtonPressed()
    {
        CardManager cm = Singleton.instance.CardManager;
        cm.selectedCard = null;
        ChangeHunterMenuState(hunterMenuState = HunterMenuState.RollDiceToMove);
    }

    public void OnRestButtonPressed()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (gm.characterActed || gm.characterMoved)
            return;

        if (gm.ActiveCharacter() is Hunter hunter)
            hunter.Rest();
        gm.EndTurn();
    }

    //using a skill includes attacking with equipped weapon, and using field skills.
    public void OnSkillButtonPressed()
    {
        ChangeHunterMenuState(hunterMenuState = HunterMenuState.SkillMenu);
    }

    public void OnItemButtonPressed()
    {
        ChangeHunterMenuState(hunterMenuState = HunterMenuState.Inventory);
    }

    public void OnEndTurnButtonPressed()
    {
        GameManager gm = Singleton.instance.GameManager;
        gm.EndTurn();
    }

    public void OnRollDiceButtonPressed()
    {
        GameManager gm = Singleton.instance.GameManager;
        CardManager cm = Singleton.instance.CardManager;

        //if player used card, it's removed from their hand here. Player cannot undo actions at this point.
        if (gm.ActiveCharacter() is Hunter hunter)
        {
            if (cm.selectedCard != null)
            {
                hunter.cards.Remove(cm.selectedCard);
                ui.activeCardText.text = "Active Card: " + cm.selectedCard.cardName;

                //is this card a MOV card?
                if (cm.selectedCard.triggerWhenDiceRolled == true)
                {
                    cm.selectedCard.ActivateCard_Field(hunter);
                }
            }
        }

        gm.GetMoveRange();
        ChangeHunterMenuState(hunterMenuState = HunterMenuState.SelectMoveTile);
        /*Dungeon dungeon = Singleton.instance.Dungeon;
        int totalMove = gm.dice.RollSingleDie() + gm.ActiveCharacter().mov;
        gm.movementPositions = gm.ShowMoveRange(dungeon.dungeonGrid, gm.ActiveCharacter(), totalMove);
        Debug.Log("Total Move: " + totalMove);*/
    }

    //different UI is closed/opened depending on hunter menu state
    public void OnHunterMenuBackButtonPressed()
    {
        GameManager gm = Singleton.instance.GameManager;
        CardManager cm = Singleton.instance.CardManager;
        switch (hunterMenuState)
        {
            case HunterMenuState.SelectCard:
            case HunterMenuState.ActionSubmenu:
                //clear selected card
                cm.selectedCard = null;
                ChangeHunterMenuState(hunterMenuState = HunterMenuState.Default);
                break;

            case HunterMenuState.RollDiceToMove:
                cm.selectedCard = null;
                ChangeHunterMenuState(hunterMenuState = HunterMenuState.SelectCard);
                break;

            case HunterMenuState.Inventory:
            case HunterMenuState.SkillMenu:
                ChangeHunterMenuState(hunterMenuState = HunterMenuState.ActionSubmenu);
                break;

            case HunterMenuState.SelectSkillTile:
                gm.selectTile.SetActive(false);
                ChangeHunterMenuState(hunterMenuState = HunterMenuState.SkillMenu);
                break;

            
        }
    }
    #endregion

}