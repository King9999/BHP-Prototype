using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/* This script handles hunter creation. The UI for hunter setup is here. */
public class HunterManager : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class HunterUI : MonoBehaviour
{
    //[Header("---UI---")]
    public List<TextMeshProUGUI> hunterNameText;
    public List<TextMeshProUGUI> hunterHpText, hunterSpText;    //shows both current and max values
    public List<Slider> superMeterUI;
    public TextMeshProUGUI hunterStr, hunterVit, hunterMnt, hunterSpd, hunterAtp, hunterDfp, hunterMnp, hunterRst, hunterEvd, hunterMov;
    public TextMeshProUGUI strPointsUI, spdPointsUI, vitPointsUI, mntPointsUI;
    public TextMeshProUGUI equippedWeaponText, equippedArmorText, equippedAccText;
}
