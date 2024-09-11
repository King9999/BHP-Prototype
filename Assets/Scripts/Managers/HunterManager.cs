using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.XR;
using static UnityEditor.Progress;

/* This script handles hunter creation. The UI for hunter setup is here. */
public class HunterManager : MonoBehaviour
{
    public HunterUI ui;
    public List<Hunter> hunters;
    public Hunter hunterPrefab;
    private int maxHunters { get; } = 4;
    public int currentHunter;               //iterator to track current hunter's turn
    private int allocationPoint { get; } = 1;
    public int startingAllocationPoints;
    public bool newHunter;  //if true, hunter was just created. The starting AP don't raise hunter level, so no bonus HP/SP
    public int rivalCount;  //used to generate CPU hunters during dungeon generation.

    public enum MenuState { NameEntry, PointAlloc, ChooseWeapon, RivalHunter, ShowHunterHuds }
    public MenuState state;

    public enum HunterMenuState { Default, SelectCard, RollDiceToMove, SelectMoveTile, Rest, ActionSubmenu, Inventory, 
        ChooseAttackTarget}
    public HunterMenuState hunterMenuState;

    GameObject hunterContainer;         //hunters are stored here for organization

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
                break;

            case HunterMenuState.SelectCard:
                ui.ShowHunterMenu_Main(false);
                ui.ShowHunterMenu_DisplayCards(true, gm.ActiveCharacter());
                ui.ShowHunterMenu_RollDiceToMove(false);
                break;

            case HunterMenuState.RollDiceToMove:
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
                break;

            case HunterMenuState.Inventory:
                ui.ShowInventory(true);
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

        //give hunter a weapon
        /*ItemManager im = ItemManager.instance;
        hunter.Equip(im.GenerateWeapon());
        ui.equippedWeaponText.text = hunter.equippedWeapon.itemName;
        ui.hunterAtpText.text = hunter.atp.ToString();*/
        ToggleHunter(hunter, false);    //disable hunter for now
        hunters.Add(hunter);
        //hunters[0].inventory.Add(im.GenerateWeapon());  //adding weapon as a test
    }

    #region Point Allocation
    //Allocates a point to STR when clicked
    private void AllocatePoint_STR(Hunter hunter, int amount)
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

    
    private void AllocatePoint_SPD(Hunter hunter, int amount)
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

    private void AllocatePoint_VIT(Hunter hunter, int amount)
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

    private void AllocatePoint_MNT(Hunter hunter, int amount)
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
        int weaponIndex = (int)Table.ItemType.Weapon;
        switch (ui.WeaponDropdownValue())
        {
            case 0:
                item = im.lootTable.GetItem(im.lootTable.itemTables[weaponIndex], "weapon_beamSword");
                Debug.Log("Item is " + item);
                break;

            case 1:
                item = im.lootTable.GetItem(im.lootTable.itemTables[weaponIndex], "weapon_railGun");
                Debug.Log("Item is " + item);
                break;

            case 2:
                item = im.lootTable.GetItem(im.lootTable.itemTables[weaponIndex], "weapon_augmenter");
                Debug.Log("Item is " + item);
                break;
        }

        hunters[hunters.Count - 1].Equip((Weapon)item);
        newHunter = false;      //level will start going up now when allocating points

        //save the number of rivals, it will be needed during dungeon generation.
        rivalCount = ui.RivalDropdownValue() + 1;   //we add 1 due to zero indexing
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

        gm.dice.ShowSingleDieUI(true);
        ChangeHunterMenuState(hunterMenuState = HunterMenuState.RollDiceToMove);    //TODO: change to SelectCard when ready

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
        switch (hunterMenuState)
        {
            case HunterMenuState.SelectCard:
            case HunterMenuState.ActionSubmenu:
                ChangeHunterMenuState(hunterMenuState = HunterMenuState.Default);
                break;

            case HunterMenuState.RollDiceToMove:
                gm.dice.ShowSingleDieUI(false);
                ChangeHunterMenuState(hunterMenuState = HunterMenuState.Default); //TODO: Change state to SelectCard when ready
                break;

            case HunterMenuState.Inventory:
                ChangeHunterMenuState(hunterMenuState = HunterMenuState.ActionSubmenu);
                break;

            
        }
    }
    #endregion

}