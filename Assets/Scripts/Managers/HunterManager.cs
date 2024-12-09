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
using UnityEngine.UIElements;
//using System;

/* This script handles hunter creation. The UI for hunter setup is here. */
public class HunterManager : MonoBehaviour
{
    public HunterUI ui;
    public List<Hunter> hunters;
    public Hunter hunterPrefab;
    public ActiveSkill basicAttackSkill;          //when a hunter is created, they get an attack skill that must be instantiated
    private int MaxHunters { get; } = 4;
    public int MaxInventorySize { get; } = 10;
    public int currentHunter;               //iterator to track current hunter's turn
    private int AllocationPoint { get; } = 1;
    public int startingAllocationPoints;
    public bool newHunter;  //if true, hunter was just created. The starting AP don't raise hunter level, so no bonus HP/SP
    public int rivalCount;  //used to generate CPU hunters during dungeon generation.

    public enum MenuState { NameEntry, PointAlloc, ChooseWeapon, RivalHunter, ShowHunterHuds }
    public MenuState state;

    public enum HunterMenuState { Default, CPUTurn, SelectCard, CardDetails, RollDiceToMove, SelectMoveTile, SelectSkillTile, Rest, ActionSubmenu, Inventory, 
        InventoryItemDetails, TooManyItems, SkillMenu, SkillDetails, ChooseAttackTarget }
    public HunterMenuState hunterMenuState;

    GameObject hunterContainer;                     //hunters are stored here for organization
    [Header("---CPU Hunters----")]
    public List<Hunter_AI> hunterBehaviours;        //used by CPU Hunters
    public float itemChanceMod;                     //used by dungeon mods to influence odds of CPU hunters carrying items.
    public enum HunterAIState { Idle, Moving, UseSkill, RemovingExtraItem }  //Moving = looking for a space to move to. Will look for points of interest.
                                                    //UseSkill = use a skill if CPU found a valid target during the Moving state.
    public HunterAIState aiState;

    //Super meter gain rates
    //Automatically at the start of a Hunter’s turn(2.5%)
    //Deal or take damage during combat(5%)

    public float SuperMeterGain_turnStart { get; } = 0.025f;
    public float SuperMeterGain_combatDamage { get; } = 0.05f;

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
        hunterContainer = new GameObject("Hunters");
        hunterContainer.transform.SetParent(this.transform);
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

    private void OnEnable()
    {
        //set up card menu buttons.
        CardMenu cm = Singleton.instance.CardMenu;
        cm.backButton.onClick.AddListener(OnHunterMenuBackButtonPressed);
        cm.selectCardButton.onClick.AddListener(OnSelectCardButtonPressed);
        cm.skipButton.onClick.AddListener(OnSkipCardButtonPressed);
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
                //ui.ShowHunterHuds(true);
                break;
        }
    }

    public void ChangeHunterMenuState(HunterMenuState state)
    {
        GameManager gm = Singleton.instance.GameManager;
        Inventory inv = Singleton.instance.Inventory;
        CardMenu cm = Singleton.instance.CardMenu;
        switch(state)
        {
            case HunterMenuState.Default:
                ui.ShowHunterMenuContainer(true);
                ui.ShowHunterMenu_Main(true, gm.ActiveCharacter());
                //hide all other UI
                //ui.ShowHunterMenu_DisplayCards(false);
                cm.ShowMenu(false);
                ui.ShowHunterMenu_RollDiceToMove(false);
                ui.ShowHunterMenu_ActionSubmenu(false);
                ui.ShowInventory(false);
                ui.ShowSkillsMenu(false);
                ui.ShowSelectingTargetMenu(false);
                //ui.ShowCardsMenu(false);
                ui.ShowCardDetails(false);
                break;

            case HunterMenuState.CPUTurn:
                ui.ShowHunterMenu_Main(false);
                break;

            case HunterMenuState.SelectCard:
                ui.ShowHunterMenu_Main(false);
                //ui.ShowHunterMenu_DisplayCards(true, gm.ActiveCharacter());
                cm.ShowMenu(true, gm.ActiveCharacter());
                //ui.ShowCardsMenu(true, gm.ActiveCharacter());
                gm.dice.ShowSingleDieUI(false);
                ui.ShowHunterMenu_RollDiceToMove(false);
                ui.ShowCardDetails(false);
                break;

            case HunterMenuState.CardDetails:
                //ui.ShowCardDetails(true);
                cm.ShowCardDetails(true);
                break;

            case HunterMenuState.RollDiceToMove:
                gm.dice.ShowSingleDieUI(true);
                ui.ShowHunterMenu_RollDiceToMove(true, gm.ActiveCharacter());
                ui.ShowHunterMenu_Main(false);
                //ui.ShowHunterMenu_DisplayCards(false);
                cm.ShowMenu(false);
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
            case HunterMenuState.TooManyItems:
                inv.ShowInventory(true, gm.ActiveCharacter() as Hunter);    //if TooManyItems, should also show extra item inventory
                ui.ShowHunterMenu_ActionSubmenu(false);
                //ui.ShowDetailsWindow(false);
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

    public Hunter_AI GetHunterAI(Hunter_AI.BehaviourType behaviourType)
    {
        bool behaviourFound = false;
        int i = 0;
        Hunter_AI hunterAI = null;
        while (!behaviourFound && i < hunterBehaviours.Count)
        {
            if (behaviourType == hunterBehaviours[i].behaviourType)
            {
                behaviourFound = true;
                hunterAI = hunterBehaviours[i];
            }
            else
            {
                i++;
            }

        }

        return hunterAI;
    }

    public int AverageHunterLevel()
    {
        int average = 0;

        for (int i = 0; i < hunters.Count; i++)
        {
            average += hunters[i].hunterLevel;
        }

        average /= hunters.Count;

        return average;
    }

    

    public void CreateHunter()
    {
        if (hunters.Count >= MaxHunters)
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
        ui.hunterEvdText.text = string.Format("{0}%", (hunter.evd * 100));
        ui.hunterMovText.text = hunter.mov.ToString();
        ui.hunterHpText.text = string.Format("{0}/{1}", hunter.healthPoints, hunter.maxHealthPoints);
        ui.hunterSpText.text = string.Format("{0}/{1}", hunter.skillPoints, hunter.maxSkillPoints);

        //point allocation values
        ui.strPointsText.text = hunter.strPoints.ToString();
        ui.vitPointsText.text = hunter.vitPoints.ToString();
        ui.mntPointsText.text = hunter.mntPoints.ToString();
        ui.spdPointsText.text = hunter.spdPoints.ToString();
        startingAllocationPoints = 16;
        ui.allocationPointsText.text = string.Format("{0} Allocation Points Remaining", startingAllocationPoints);

        //set up animations
        hunter.animations[0].sprites = Resources.LoadAll<Sprite>("Idle").ToList();
        hunter.animations[1].sprites = Resources.LoadAll<Sprite>("Run Cycle").ToList();

        //testing out cards
        //CardManager cm = Singleton.instance.CardManager;
        //cm.DrawCard(hunter, Card.CardID.TrapDrain);
        //cm.DrawCard(hunter, cm.deck, 3);

       

        /****Give super TODO: Player can choose which super they want. For now, will just pick random */
        SkillManager sm = Singleton.instance.SkillManager;
        hunter.super = sm.AddSuper();

        //give hunter an item
        //ItemManager im = ItemManager.instance;
        //hunter.inventory.Add(im.lootTable.GetItem(Table.ItemType.SkillChip));

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
        //EffectManager em = Singleton.instance.EffectManager;
        //em.AddEffect(StatusEffect.Effect.Berserk, hunter);

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
        SkillManager sm = Singleton.instance.SkillManager;
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
                        wpn.itemMods[i].modName = string.Format("[{0} skill] {1}", wpn.itemSkill.skillType, wpn.itemSkill.skillName);
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
                    //SkillManager sm = Singleton.instance.SkillManager;
                    
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
                            armor.itemMods[i].modName = string.Format("[{0} skill] {1}", armor.itemSkill.skillType, armor.itemSkill.skillName);
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
                    //SkillManager sm = Singleton.instance.SkillManager;

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
                            acc.itemMods[i].modName = string.Format("[{0} skill] {1}", acc.itemSkill.skillType, acc.itemSkill.skillName);
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
        Debug.LogFormat("Chance for CPU Hunter to have items: {0}", itemChance);
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
        Debug.LogFormat("Number of items CPU Hunter will carry: {0}", itemCount);

        //Test purposes only
        //hunter.spd = 0;

        //Add random super
        hunter.super = sm.AddSuper();

        //give hunter a random name TODO: Get a name from a file.
        hunter.characterName = string.Format("CPU {0}",hunters.Count);
        hunter.name = string.Format("Hunter - {0}", hunter.characterName);
        hunter.healthPoints = hunter.maxHealthPoints;
        hunter.skillPoints = hunter.maxSkillPoints;
        return hunter;
    }

    //CPU-controlled actions. Uses weight to determine priorities.
    public void ChangeCPUHunterState(HunterAIState aiState, Hunter hunter/*, List<Character> targets = null*/)
    {
        GameManager gm = Singleton.instance.GameManager;

        switch(aiState)
        {
            case HunterAIState.Idle:
                break;

            case HunterAIState.Moving:
                StartCoroutine(CPU_MoveHunter(hunter));
                break;

            case HunterAIState.UseSkill:
                Debug.LogFormat("{0} is using skill {1}", hunter.characterName, hunter.chosenSkill);
                StartCoroutine(CPU_UseSkill(hunter));
                break;
        }
    }

    /* Executes all actions for moving a hunter. Includes choosing a card, rolling dice, and moving hunter to new space. */
    IEnumerator CPU_MoveHunter(Hunter hunter)
    {

        //decide whether to use a card. Likelihood increases depending on behaviour.
        GameManager gm = Singleton.instance.GameManager;
        CardManager cm = Singleton.instance.CardManager;

        cm.selectedCard = hunter.cpuBehaviour.ChooseCard_Field(hunter);
        
        if (cm.selectedCard != null)
        {
            hunter.cards.Remove(cm.selectedCard);
            ui.activeCardText.text = string.Format("Active Card: {0}", cm.selectedCard.cardName);

            //is this card a MOV card?
            if (cm.selectedCard.triggerWhenDiceRolled == true)
            {
                cm.selectedCard.ActivateCard_Field(hunter);
            }
            //cm.selectedCard.ActivateCard_Field(hunter);
            Debug.LogFormat("{0} is playing card {1}", hunter.characterName, cm.selectedCard.cardName);
        }
        yield return new WaitForSeconds(1);

        //roll dice; show die UI and roll die after a second.
        gm.dice.ShowSingleDieUI(true);
        yield return new WaitForSeconds(1);

        //show move tiles. This code is identical to the code in the Update look of GameManager.
        int totalMove = Mathf.RoundToInt((hunter.mov + gm.dice.RollSingleDie(Dice.DiceType.Move) + gm.movementMod) * hunter.movMod);
        List<Room> moveRange = gm.ShowMoveRange(hunter, totalMove);
        Debug.LogFormat("Total Move for {0}: {1}", hunter.characterName, totalMove);

        //keep a list of characters and entities in range
        List<Character> charactersInRange = new List<Character>();
        List<Entity> entitiesInRange = new List<Entity>();

        foreach (Room pos in moveRange)
        {
            //if there are existing move tile objects, activate those first before instantiating new ones.
            if (gm.moveTileBin.Count > 0)
            {
                GameObject lastTile = gm.moveTileBin[0];
                lastTile.SetActive(true);
                lastTile.transform.position = new Vector3(pos.transform.position.x, 0.6f, pos.transform.position.z);
                gm.moveTileList.Add(lastTile);
                gm.moveTileBin.Remove(lastTile);
            }
            else
            {
                GameObject tile = Instantiate(gm.moveTilePrefab, gm.moveTileContainer.transform);
                tile.transform.position = new Vector3(pos.transform.position.x, 0.6f, pos.transform.position.z);
                gm.moveTileList.Add(tile);
            }

            //check each tile and look for entities or characters.
            if (pos.character != null)
            {
                charactersInRange.Add(pos.character);
            }

            if (pos.entity != null && !pos.entity.playerInteracted)
            {
                entitiesInRange.Add(pos.entity);
            }

        }

        if (gm.moveTileBin.Count <= 0)
        {
            gm.moveTileBin.TrimExcess();
        }

        Debug.LogFormat("Characters in range: {0}", charactersInRange.Count);
        Debug.LogFormat("Entities in range: {0}", entitiesInRange.Count);

        /*******find a character to attack. *******/
        Character targetChar = null;
        bool targetItemFound = false;
        bool bullyTargetFound = false;
        bool lootFound = false;
        int monsterCount = 0;               //need this in case all characters are monsters and hunter can't target them.
        if (charactersInRange.Count > 0)
        {
            int i = 0;
            bool charFound = false;
            while (!charFound && i < charactersInRange.Count)
            {
                if (charactersInRange[i] is Monster && !hunter.cpuBehaviour.canAttackMonsters)
                {
                    monsterCount++;
                    i++;
                    continue;
                }

                //The hunter with the target item is high priority. Also includes bully-specific condition.
                if (charactersInRange[i] is Hunter targetHunter && targetHunter.HasTargetItem())
                {
                    targetItemFound = true;
                    charFound = true;
                    hunter.targetChar = charactersInRange[i];
                }
                else if(hunter.cpuBehaviour is Hunter_AI_Bully bully && bully.bullyTarget == charactersInRange[i])
                {
                    bullyTargetFound = true;
                    charFound = true;
                    hunter.targetChar = charactersInRange[i];
                }
                else if(charactersInRange[i] is Hunter hunterCarryingItem && hunterCarryingItem.inventory.Count > 0)
                {
                    lootFound = true;
                    charFound = true;
                    hunter.targetChar = charactersInRange[i];
                }
                else
                {
                    i++;
                }

            }

            //if we get here, just pick a random hunter to target. If all characters are monsters and hunter can't target
            //monsters, this code is skipped.
            if (monsterCount < charactersInRange.Count)
            {
                while (!charFound)
                {
                    int randChar = Random.Range(0, charactersInRange.Count);

                    if (charactersInRange[randChar] is Monster && hunter.cpuBehaviour.canAttackMonsters)
                    {
                        hunter.targetChar = charactersInRange[randChar];
                        charFound = true;
                    }

                    if (charactersInRange[randChar] is Hunter)
                    {
                        hunter.targetChar = charactersInRange[randChar];
                        charFound = true;
                    }
                }
            }
        }

        /******check for entities******/
        Entity targetEntity = null;
        if (entitiesInRange.Count > 0)
        {
            //pick the closest entity
            float shortestDistance = 0;
            for (int i = 0; i < entitiesInRange.Count; i++)
            {
                //if this item is a chest and CPU can't open chests, we skip this entity.
                if (entitiesInRange[i] is Entity_TreasureChest && !hunter.cpuBehaviour.canOpenChests)
                    continue;

                if (entitiesInRange[i] is Entity_Terminal && !hunter.cpuBehaviour.canUseTerminals)
                    continue;

                if (i <= 0 || shortestDistance <= 0)
                {
                    shortestDistance = Vector3.Distance(hunter.transform.position, entitiesInRange[i].transform.position);
                    targetEntity = entitiesInRange[i];
                    continue;
                }

                float distance = Vector3.Distance(hunter.transform.position, entitiesInRange[i].transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    targetEntity = entitiesInRange[i];
                }
            }

        }

        yield return new WaitForSeconds(1);

        //clear move tiles and start moving character
        for (int j = 0; j < gm.moveTileList.Count; j++)
        {
            gm.moveTileList[j].SetActive(false);
            gm.moveTileBin.Add(gm.moveTileList[j]);
        }
        gm.moveTileList.Clear();
        gm.moveTileList.TrimExcess();

        if (hunter.targetChar == null && targetEntity == null)
        {
            //nothing of interest, move to a random spot. Move the full distance.
            //TODO: may make it so that CPU looks for an out of range target and move towards it.
            gm.MoveCPUCharacter(hunter, moveRange[moveRange.Count - 1].transform.position);
        }
        else
        {
            //choose between moving towards an entity or towards a character. Entities are preferred over
            //character, unless the character has the target item.
            if (targetItemFound || bullyTargetFound)
            {
                //TODO: determine which skill is going to be used and move into range to use the skill.
                hunter.chosenSkill = GetSkill(hunter, hunter.targetChar);
                Vector3 newPos = new Vector3(hunter.targetChar.transform.position.x, 0, hunter.targetChar.transform.position.z);
                gm.MoveCPUCharacter(hunter, newPos, true);
                Debug.Log("Moving towards hunter with target item, or is a bully target");
            }
            else if (targetEntity != null && hunter.cpuBehaviour.canOpenChests)
            {
                Vector3 newPos = new Vector3(targetEntity.transform.position.x, 0, targetEntity.transform.position.z);
                hunter.targetChar = null;       //not targeting a character so must clear.
                gm.MoveCPUCharacter(hunter, newPos);
                Debug.Log("moving to entity");
            }
            else //hunter has items in inventory, probably TODO: there's an error here, this code is sometimes executed without a target.
            {
                hunter.chosenSkill = GetSkill(hunter, hunter.targetChar);
                Vector3 newPos = new Vector3(hunter.targetChar.transform.position.x, 0, hunter.targetChar.transform.position.z);
                gm.MoveCPUCharacter(hunter, newPos, true);
                Debug.Log("Moving towards character.");
            }
            
        }
        //move CPU

        //clear move tiles
        /*for (int j = 0; j < gm.moveTileList.Count; j++)
        {
            gm.moveTileList[j].SetActive(false);
            gm.moveTileBin.Add(gm.moveTileList[j]);
        }
        gm.moveTileList.Clear();
        gm.moveTileList.TrimExcess();
        Vector3 destinationPos = new Vector3(selectTile.transform.position.x, 0, selectTile.transform.position.z);
        StartCoroutine(MoveCharacter(ActiveCharacter(), destinationPos));*/



    }

    //use a skill on a target. This code is only executed when a target is selected.
    IEnumerator CPU_UseSkill(Hunter hunter)
    {
        //display the skill's range
        GameManager gm = Singleton.instance.GameManager;
        List<Room> skillRange = gm.ShowSkillRange(hunter, hunter.chosenSkill.minRange, hunter.chosenSkill.maxRange);

        gm.DisplaySkillTiles(skillRange);

        yield return new WaitForSeconds(1);
        
        for (int i = 0; i < gm.skillTileList.Count; i++)
        {
            gm.skillTileList[i].SetActive(false);
            gm.skillTileBin.Add(gm.skillTileList[i]);
        }
        gm.skillTileList.Clear();
        gm.skillTileList.TrimExcess();

        //Start combat
        gm.ChangeGameState(gm.gameState = GameManager.GameState.Combat);
        //gm.StartCombat(hunter, hunter.targetChar);
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

    //choose a random skill. Used by CPU Hunters.
    ActiveSkill GetSkill(Hunter user, Character target)
    {
        ActiveSkill skill = null;
        bool skillFound = false;
        List<ActiveSkill> activeSkills = new List<ActiveSkill>();

        //collect all of the active skills
        foreach(Skill skl in user.skills)
        {
            if (skl is ActiveSkill actSkl)
            {
                activeSkills.Add(actSkl);
            }
        }

        while (!skillFound)
        {
            int randSkill = Random.Range(0, activeSkills.Count);

            if (activeSkills[randSkill].skillCost <= user.skillPoints ||
                (activeSkills[randSkill].requiresCharges && activeSkills[randSkill].skillCharges > 0))
            {
                skill = activeSkills[randSkill];
                skillFound = true;
            }
        }
        

        return skill;
    }

    /// <summary>
    /// Updates hunter's super meter by given amount
    /// </summary>
    /// <param name="hunter"></param>
    /// <param name="amount">The amount of meter to give. max value is 1.</param>
    public void UpdateSuperMeter(Hunter hunter, float amount)
    {
        if (amount < 0 || amount > 1)
        {
            Debug.LogError("Meter amount must be between 0 and 1!");
            return;
        }

        hunter.super.AddMeter(amount);

        //find hunter in hunter HUD
        ui.hunterHuds[hunter.HudID].superMeterUI.value = hunter.super.superMeter;
        /*bool hunterFound = false;
        int i = 0;
        while (!hunterFound && i < hunters.Count)
        {
            if (hunters[i] == hunter)
            {
                hunterFound = true;
                ui.hunterHuds[i].superMeterUI.value += amount;
            }
            else
            {
                i++;
            }
        }*/
    }

    //updates all values
    public void UpdateHunterHUD(Hunter hunter)
    {
        if (hunter == null)
            return;

        //Update HP/SP/Super meter
        ui.hunterHuds[hunter.HudID].hunterHpText.text = string.Format("{0}/{1}", hunter.healthPoints, hunter.maxHealthPoints);
        ui.hunterHuds[hunter.HudID].hunterSpText.text = string.Format("{0}/{1}", hunter.skillPoints, hunter.maxSkillPoints);
        ui.hunterHuds[hunter.HudID].superMeterUI.value = hunter.super.superMeter;
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
        ui.hunterHpText.text = string.Format("{0} / {1}", hunter.healthPoints, hunter.maxHealthPoints);
        ui.hunterSpText.text = string.Format("{0} / {1}", hunter.skillPoints, hunter.maxSkillPoints);
        
        ui.allocationPointsText.text = string.Format("{0} Allocation Points Remaining", startingAllocationPoints);
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
        ui.hunterEvdText.text = string.Format("{0}%", (hunter.evd * 100));
        ui.hunterHpText.text = string.Format("{0} / {1}", hunter.healthPoints, hunter.maxHealthPoints);
        ui.hunterSpText.text = string.Format("{0} / {1}", hunter.skillPoints, hunter.maxSkillPoints);

        ui.allocationPointsText.text = string.Format("{0} Allocation Points Remaining", startingAllocationPoints);
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
        ui.hunterHpText.text = string.Format("{0} / {1}", hunter.healthPoints, hunter.maxHealthPoints);
        ui.hunterSpText.text = string.Format("{0} / {1}", hunter.skillPoints, hunter.maxSkillPoints);

        ui.allocationPointsText.text = string.Format("{0} Allocation Points Remaining", startingAllocationPoints);
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
        ui.hunterHpText.text = string.Format("{0} / {1}", hunter.healthPoints, hunter.maxHealthPoints);
        ui.hunterSpText.text = string.Format("{0} / {1}", hunter.skillPoints, hunter.maxSkillPoints);

        ui.allocationPointsText.text = string.Format("{0} Allocation Points Remaining", startingAllocationPoints);
    }

    #endregion

    #region Button Methods
    public void OnAllocateStrButtonPressed()
    {
        if (startingAllocationPoints <= 0)
            return;
        AllocatePoint_STR(hunters[currentHunter], AllocationPoint);
    }

    public void OnAllocateVitButtonPressed()
    {
        if (startingAllocationPoints <= 0)
            return;
        AllocatePoint_VIT(hunters[currentHunter], AllocationPoint);
    }

    public void OnAllocateSpdButtonPressed()
    {
        if (startingAllocationPoints <= 0)
            return;
        AllocatePoint_SPD(hunters[currentHunter], AllocationPoint);
    }

    public void OnAllocateMntButtonPressed()
    {
        if (startingAllocationPoints <= 0)
            return;
        AllocatePoint_MNT(hunters[currentHunter], AllocationPoint);
    }

    public void OnDeallocateStrButtonPressed()
    {
        if (hunters[currentHunter].strPoints <= 0 || startingAllocationPoints >= 16)
            return;
        AllocatePoint_STR(hunters[currentHunter], -AllocationPoint);
    }

    public void OnDeallocateVitButtonPressed()
    {
        if (hunters[currentHunter].vitPoints <= 0 || startingAllocationPoints >= 16)
            return;
        AllocatePoint_VIT(hunters[currentHunter], -AllocationPoint);
    }

    public void OnDeallocateSpdButtonPressed()
    {
        if (hunters[currentHunter].spdPoints <= 0 || startingAllocationPoints >= 16)
            return;
        AllocatePoint_SPD(hunters[currentHunter], -AllocationPoint);
    }

    public void OnDeallocateMntButtonPressed()
    {
        if (hunters[currentHunter].mntPoints <= 0 || startingAllocationPoints >= 16)
            return;
        AllocatePoint_MNT(hunters[currentHunter], -AllocationPoint);
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
            if(ui.nameEntryField.text.Equals(""))
            {
                Debug.Log("Name cannot be empty!");
                return;
            }
            else
            {
                hunters[hunters.Count - 1].characterName = ui.nameEntryField.text;
                hunters[hunters.Count - 1].name = string.Format("Hunter - {0}", ui.nameEntryField.text); //object name
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
                break;

            case 1:
                item = im.lootTable.GetItem(Table.ItemType.Weapon, "weapon_railGun");
                break;

            case 2:
                item = im.lootTable.GetItem(Table.ItemType.Weapon, "weapon_augmenter");
                break;
        }

        Debug.LogFormat("Item is {0}", item);
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
        Debug.LogFormat("rival count: {0}", rivalCount);

        ChangeState(state = MenuState.ShowHunterHuds);
        //move to game scene
        SceneManager.LoadScene("Game");
    }

    //move button in hunter menu. Allows selecting card, and then rolling dice to move.
    public void OnMoveButtonPressed()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (gm.CharacterMoved)
            return;

        //gm.dice.ShowSingleDieUI(true);
        ChangeHunterMenuState(hunterMenuState = HunterMenuState.SelectCard);    //TODO: change to SelectCard when ready

    }

    //show a sub-menu to attack or use an item
    public void OnActionButtonPressed()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (gm.CharacterActed)
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
        if (gm.CharacterActed || gm.CharacterMoved)
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
                ui.activeCardText.text = string.Format("Active Card: {0}", cm.selectedCard.cardName);

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