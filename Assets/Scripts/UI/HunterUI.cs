using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
//using UnityEngine.UIElements;

/* script for all UI relating to Hunters. Is utilized by Hunter Manager */
public class HunterUI : MonoBehaviour
{
    //[Header("---UI---")]
    //public List<TextMeshProUGUI> hunterNameText;
    //public List<TextMeshProUGUI> hunterHpText, hunterSpText;    //shows both current and max values
    //public List<Slider> superMeterUI;
    public TextMeshProUGUI hunterStrText, hunterVitText, hunterMntText, hunterSpdText, hunterAtpText, 
        hunterDfpText, hunterMnpText, hunterRstText, hunterEvdText, hunterMovText, hunterHpText, hunterSpText;
    public TextMeshProUGUI strPointsText, spdPointsText, vitPointsText, mntPointsText;
    public TextMeshProUGUI equippedWeaponText, equippedArmorText, equippedAccText;
    public HunterHUD[] hunterHuds;
    public TextMeshProUGUI allocationPointsText;
    public TextMeshProUGUI weaponDetailsText;
    [TextArea]public string[] weaponDetailsStr;           //contains info about starter weapons
    public TMP_InputField nameEntryField;

    //UI game objects.
    [Header("---Menu & HUD---")]
    public GameObject nameEntryMenuObject;
    public GameObject pointAllocationMenuObject;
    public GameObject mainHunterHudObject;
    public GameObject equipmentMenuObject;
    public GameObject weaponSelectMenuObject;
    public GameObject inventoryMenuObject;
    public GameObject rivalHunterMenuObject;            //screen for choosing number of opponents.
    public GameObject hunterMenuContainer;                 //used to show the other menus
    public GameObject hunterMenuObject_main;            //displays Move, attack, rest
    public GameObject hunterMenuObject_rollDiceToMove;
    public GameObject hunterMenuObject_showCards;
    public GameObject hunterMenuObject_actionSubmenu;
    public TextMeshProUGUI hunterMenu_hunterNameText;
    public TextMeshProUGUI hunterMenu_actionText;
    public GameObject[] hunterHudObjects;
    public List<ItemObject> inventory;                  //shows a hunter's collected items.
    public GameObject detailsWindow;
    public TextMeshProUGUI itemTypeText, itemDetailsText, moneyText;
    public TextMeshProUGUI activeCardText;      //displays the currently used card in the field.
    public TextMeshProUGUI deckCountText;

    //skills menu
    public GameObject skillMenuObject, skillDetailsWindow, selectTargetMenu;
    public TextMeshProUGUI skillDetailsText;
    public List<SkillObject> hunterSkills;

    //card menu
    public GameObject /*selectCardMenu,*/ cardDetailsWindow;
    public TextMeshProUGUI cardDetailsText, cardNameText;
    public List<CardObject> hunterCards;


    [Header("---Buttons---")]
    public Button moveButton;
    public Button actionButton;     //includes attacking and using item.
    public Button inventoryButton;  //used to access consumable items
    public Button restButton;  //used to access consumable items


    public static HunterUI instance;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        activeCardText.text = "";
        deckCountText.text = "";
        //Singleton.instance.HunterUI = this;
        //transform.SetParent(Singleton.instance.transform);
        //DontDestroyOnLoad(instance);
    }

    public void ShowPointAllocationMenu(bool toggle)
    {
        pointAllocationMenuObject.SetActive(toggle);
    }

    public void ShowWeaponSelectionMenu(bool toggle)
    {
        weaponSelectMenuObject.SetActive(toggle);
        if (toggle == true)
        {
            //set the weapon details field to default weapon
            TMP_Dropdown weaponDropdown = weaponSelectMenuObject.GetComponentInChildren<TMP_Dropdown>();
            weaponDetailsText.text = weaponDetailsStr[weaponDropdown.value];
        }
    }

    public void ShowNameEntryMenu(bool toggle)
    {
        nameEntryMenuObject.SetActive(toggle);
    }

    public void ShowRivalHunterMenu(bool toggle)
    {
        rivalHunterMenuObject.SetActive(toggle);
    }
    public int RivalDropdownValue()
    {
        TMP_Dropdown rivalDropdown = rivalHunterMenuObject.GetComponentInChildren<TMP_Dropdown>();
        return rivalDropdown.value;
    }

    public void OnWeaponDropdownValueChanged()
    {
        TMP_Dropdown weaponDropdown = weaponSelectMenuObject.GetComponentInChildren<TMP_Dropdown>();
        weaponDetailsText.text = weaponDetailsStr[weaponDropdown.value];
    }

    public int WeaponDropdownValue()
    {
        TMP_Dropdown weaponDropdown = weaponSelectMenuObject.GetComponentInChildren<TMP_Dropdown>();
        return weaponDropdown.value;
    }

    public void ShowHunterHuds(bool toggle)
    {
        //ensure the huds are turned off before the main object is turned off
        if (toggle == false)
        {
            for (int i = 0; i < hunterHudObjects.Length; i++)
            {
                hunterHudObjects[i].SetActive(false);
            }
        }

        mainHunterHudObject.SetActive(toggle);

        if (toggle == true)
        {
            //get number of hunters and activate hud for each one
            HunterManager hm = Singleton.instance.HunterManager;
            int i = 0;
            foreach(Hunter hunter in hm.hunters)
            {
                hunterHudObjects[i].SetActive(true);
                hunterHuds[i].hunterNameText.text = hunter.characterName;
                hunterHuds[i].hunterHpText.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
                hunterHuds[i].hunterSpText.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
                //TODO: Add super meter slider here
                hunterHuds[i].superMeterUI.value = 0;
                i++;
            }
        }
    }

    public void ShowHunterMenuContainer(bool toggle)
    {
        hunterMenuContainer.SetActive(toggle);
        //hunterMenuObject_main.SetActive(true);  //default menu to appear
        //hunterMenuObject_showCards.SetActive(false);
        //hunterMenuObject_rollDiceToMove.SetActive(false);
    }

    public void ShowHunterMenu_Main(bool toggle, Character character = null)
    {
        if (!hunterMenuContainer.activeSelf)
            return;

        hunterMenuObject_main.SetActive(toggle);
        if (toggle == true)
        {
            //display menu above character
            Vector3 menuPos = new Vector3(character.transform.position.x, character.transform.position.y + 6, character.transform.position.z);
            hunterMenuObject_main.transform.position = Camera.main.WorldToScreenPoint(menuPos);

            //get character info
            hunterMenu_hunterNameText.text = character.characterName + "'s Turn";

            //check if any buttons need to be disabled
            GameManager gm = Singleton.instance.GameManager;
            if (gm.characterActed)
                EnableButton(actionButton, false);
            else
                EnableButton(actionButton, true);

            if (gm.characterMoved)
                EnableButton(moveButton, false);
            else
                EnableButton(moveButton, true);

            //did character rest?
            if (gm.characterActed || gm.characterMoved)
                EnableButton(restButton, false);
            else
                EnableButton(restButton, true);
        }

    }

    public void ShowHunterMenu_RollDiceToMove(bool toggle, Character character = null)
    {
        hunterMenuObject_rollDiceToMove.SetActive(toggle);
        if (toggle == true)
        {
            Vector3 menuPos = new Vector3(character.transform.position.x, character.transform.position.y + 6, character.transform.position.z);
            hunterMenuObject_rollDiceToMove.transform.position = Camera.main.WorldToScreenPoint(menuPos);
        }
    }

    public void ShowHunterMenu_ActionSubmenu(bool toggle, Character character = null)
    {
        hunterMenuObject_actionSubmenu.SetActive(toggle);
        if (toggle == true)
        {
            Vector3 menuPos = new Vector3(character.transform.position.x, character.transform.position.y + 6, character.transform.position.z);
            hunterMenuObject_actionSubmenu.transform.position = Camera.main.WorldToScreenPoint(menuPos);
        }
    }

    public void ShowHunterMenu_DisplayCards(bool toggle, Character character = null)
    {
        hunterMenuObject_showCards.SetActive(toggle);
        if (toggle == true)
        {
            Vector3 menuPos = new Vector3(character.transform.position.x, character.transform.position.y + 6, character.transform.position.z);
            hunterMenuObject_showCards.transform.position = Camera.main.WorldToScreenPoint(menuPos);

            //get hunter cards
            GameManager gm = Singleton.instance.GameManager;
            int currentCardIndex = 0;
            if (gm.ActiveCharacter() is Hunter hunter)
            {
                for (int i = 0; i < hunter.cards.Count; i++)
                {
                    if (hunter.cards[i].cardType == Card.CardType.Field || hunter.cards[i].cardType == Card.CardType.Versatile)
                    {
                        hunterCards[currentCardIndex].ShowCard(true);
                        hunterCards[currentCardIndex].cardData = hunter.cards[i];
                        hunterCards[currentCardIndex].cardSprite = hunter.cards[i].cardSprite;
                        currentCardIndex++;
                    }
                    
                }
            }
        }
        else
        {
            foreach(CardObject card in hunterCards)
            {
                card.ShowCard(false);
            }
        }
    }

    public void EnableButton(Button button, bool toggle)
    {
        button.enabled = toggle;
        if (toggle == false)
        {
            button.image.color = button.colors.disabledColor;
        }
        else
        {
            button.image.color = button.colors.normalColor;
        }    
    }

    public void ShowInventory(bool toggle)
    {
        inventoryMenuObject.SetActive(toggle);
        ShowDetailsWindow(false);
        itemDetailsText.text = "";
        itemTypeText.text = "";
        if (toggle == true)
        {
            //populate inventory
            GameManager gm = Singleton.instance.GameManager;
            if (gm.ActiveCharacter() is Hunter hunter)
            {
                for (int i = 0; i < hunter.inventory.Count; i++)
                {
                    inventory[i].gameObject.SetActive(true);
                    inventory[i].item = hunter.inventory[i];

                    //consumables are marked for easy reading.
                    if (inventory[i].item is Consumable consumable)
                        inventory[i].itemNameText.text = hunter.inventory[i].itemName + "(Usable)";
                    else
                        inventory[i].itemNameText.text = hunter.inventory[i].itemName;
                    //inventory[i].GetDetails(hunter.inventory[i]);
                }

                //update money
                moneyText.text = "Money: " + hunter.credits + " CR";
            }

            
        }
        else
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                inventory[i].gameObject.SetActive(false);
            }
        }
    }



    public void ShowDetailsWindow(bool toggle)
    {
        /*if (toggle == false)
        {
            itemDetailsText.text = "";
            itemTypeText.text = "";
        }*/
        detailsWindow.SetActive(toggle);
        
    }

    public void ShowSkillsMenu(bool toggle)
    {
        skillMenuObject.SetActive(toggle);
        ShowSkillDetails(false);
        skillDetailsText.text = "";

        if (toggle == true)
        {
            //populate skills
            GameManager gm = Singleton.instance.GameManager;
            int currentSkillIndex = 0;
            if (gm.ActiveCharacter() is Hunter hunter)
            {
                for (int i = 0; i < hunter.skills.Count; i++)
                {
                    //only want to show active skills.
                    if (hunter.skills[i] is ActiveSkill skill)
                    {
                        hunterSkills[currentSkillIndex].gameObject.SetActive(true);
                        hunterSkills[currentSkillIndex].skill = skill;
                        hunterSkills[currentSkillIndex].skillNameText.text = skill.skillName;
                        hunterSkills[currentSkillIndex].costText.text = skill.skillCost.ToString();
                        hunterSkills[currentSkillIndex].cooldownText.text = skill.skillCooldown.ToString();

                        //formatting of skill range varies
                        if (skill.minRange > 0)
                        {
                            hunterSkills[currentSkillIndex].rangeText.text = skill.minRange + " - " + skill.maxRange;
                        }
                        else
                        {
                            hunterSkills[currentSkillIndex].rangeText.text = skill.maxRange.ToString();
                        }

                        currentSkillIndex++;                        
                    }
                }

            }
        }
        else
        {
            for (int i = 0; i < hunterSkills.Count; i++)
            {
                hunterSkills[i].gameObject.SetActive(false);
            }
        }
    }

    public void ShowSkillDetails(bool toggle)
    {
        skillDetailsWindow.SetActive(toggle);
    }

    public void ShowSelectingTargetMenu(bool toggle, Character character = null)
    {
        selectTargetMenu.SetActive(toggle);
        if (toggle == true)
        {
            Vector3 menuPos = new Vector3(character.transform.position.x, character.transform.position.y + 6, character.transform.position.z);
            selectTargetMenu.transform.position = Camera.main.WorldToScreenPoint(menuPos);
        }
    }

    /*public void ShowCardsMenu(bool toggle, Character character = null)
    {
        selectCardMenu.SetActive(toggle);
        if (toggle == true)
        {
            Vector3 menuPos = new Vector3(character.transform.position.x, character.transform.position.y + 6, character.transform.position.z);
            selectTargetMenu.transform.position = Camera.main.WorldToScreenPoint(menuPos);
        }
    }*/

    public void ShowCardDetails(bool toggle)
    {
        cardDetailsWindow.SetActive(toggle);
    }

    /*public void OnMoveButtonHover()
    {
        hunterMenu_actionText.text = "Roll a die to move to a new space";
    }
    public void OnActionButtonHover()
    {
        hunterMenu_actionText.text = "Use a skill or an item";
    }
    public void OnRestButtonHover()
    {
        hunterMenu_actionText.text = "Restore 25% HP & SP. Draw a card. This must be your first action";
    }
    public void OnExitHover()
    {
        hunterMenu_actionText.text = "";
    }*/
}

[System.Serializable]
public class HunterHUD
{
    public TextMeshProUGUI hunterNameText;
    public TextMeshProUGUI hunterHpText, hunterSpText;    //shows both current and max values
    public Slider superMeterUI;
}
