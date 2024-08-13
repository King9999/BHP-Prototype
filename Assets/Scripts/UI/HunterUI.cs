using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

/* script for all UI relating to Hunters. Is utilized by Hunter Manager */
public class HunterUI : MonoBehaviour
{
    //[Header("---UI---")]
    //public List<TextMeshProUGUI> hunterNameText;
    //public List<TextMeshProUGUI> hunterHpText, hunterSpText;    //shows both current and max values
    //public List<Slider> superMeterUI;
    public TextMeshProUGUI hunterStrText, hunterVitText, hunterMntText, hunterSpdText, hunterAtpText, 
        hunterDfpText, hunterMnpText, hunterRstText, hunterEvdText, hunterMovText;
    public TextMeshProUGUI strPointsText, spdPointsText, vitPointsText, mntPointsText;
    public TextMeshProUGUI equippedWeaponText, equippedArmorText, equippedAccText;
    public HunterHUD[] hunterHuds;
    public TextMeshProUGUI allocationPointsText;
    public TextMeshProUGUI weaponDetailsText;
    [TextArea]public string[] weaponDetailsStr;           //contains info about starter weapons

    //UI game objects.
    [Header("---Menu & HUD---")]
    public GameObject pointAllocationMenuObject;
    public GameObject mainHunterHudObject;
    public GameObject equipmentMenuObject;
    public GameObject weaponSelectMenuObject;
    public GameObject inventoryMenuObject;
    public GameObject[] hunterHudObjects;

    // Start is called before the first frame update
    void Start()
    {
        //ShowHunterHuds(false);
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void OnWeaponDropdownValueChanged()
    {
        TMP_Dropdown weaponDropdown = weaponSelectMenuObject.GetComponentInChildren<TMP_Dropdown>();
        weaponDetailsText.text = weaponDetailsStr[weaponDropdown.value];
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
                i++;
            }
        }
    }

    //public void Show
}

[System.Serializable]
public class HunterHUD
{
    public TextMeshProUGUI hunterNameText;
    public TextMeshProUGUI hunterHpText, hunterSpText;    //shows both current and max values
    public Slider superMeterUI;
}
