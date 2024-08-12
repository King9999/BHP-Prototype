using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* script for all UI relating to Hunters. Is utilized by Hunter Manager */
public class HunterUI : MonoBehaviour
{
    //[Header("---UI---")]
    //public List<TextMeshProUGUI> hunterNameText;
    //public List<TextMeshProUGUI> hunterHpText, hunterSpText;    //shows both current and max values
    //public List<Slider> superMeterUI;
    public TextMeshProUGUI hunterStr, hunterVit, hunterMnt, hunterSpd, hunterAtp, hunterDfp, hunterMnp, hunterRst, hunterEvd, hunterMov;
    public TextMeshProUGUI strPointsUI, spdPointsUI, vitPointsUI, mntPointsUI;
    public TextMeshProUGUI equippedWeaponText, equippedArmorText, equippedAccText;
    public HunterHUD[] hunterHuds;

    //UI game objects.
    public GameObject pointAllocationMenuObject;
    public GameObject MainHunterHudObject;
    public GameObject[] HunterHudObjects;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPointAllocationMenu(bool toggle)
    {
        pointAllocationMenuObject.SetActive(toggle);
    }
}

[System.Serializable]
public class HunterHUD
{
    public TextMeshProUGUI hunterNameText;
    public TextMeshProUGUI hunterHpText, hunterSpText;    //shows both current and max values
    public Slider superMeterUI;
}
