using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* handles the game state */
public class GameManager : MonoBehaviour
{
    //hunter stats
    public TextMeshProUGUI hunterName;
    public TextMeshProUGUI hunterStr, hunterVit, hunterMnt, hunterSpd, hunterAtp, hunterDfp, hunterMnp, hunterRst, hunterEvd, hunterHp, hunterSp, hunterMov;
    public TextMeshProUGUI strPointsGUI, spdPointsGUI, vitPointsGUI, mntPointsGUI;
    public Hunter hunterPrefab, hunter;

    // Start is called before the first frame update
    void Start()
    {
        CreateHunter();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateHunter()
    {
        hunter = Instantiate(hunterPrefab);
        hunter.name = "King";
        hunter.InitializeStats();
        hunterStr.text = hunter.str.ToString();
        hunterSpd.text = hunter.spd.ToString();
        hunterVit.text = hunter.vit.ToString();
        hunterMnt.text = hunter.mnt.ToString();
        hunterName.text = hunter.name.ToString();
        hunterAtp.text = hunter.atp.ToString();
        hunterDfp.text = hunter.dfp.ToString();
        hunterMnp.text = hunter.mnp.ToString();
        hunterRst.text = hunter.rst.ToString();
        hunterEvd.text = (hunter.evd * 100) + "%";
        hunterMov.text = hunter.mov.ToString();
        hunterHp.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        hunterSp.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;

        //point allocation values
        strPointsGUI.text = hunter.strPoints.ToString();
        vitPointsGUI.text = hunter.vitPoints.ToString();
        mntPointsGUI.text = hunter.mntPoints.ToString();
        spdPointsGUI.text = hunter.spdPoints.ToString();
    }

    //Allocates a point to STR when clicked
    public void AllocatePoint_STR()
    {
        hunter.AllocateToStr(1);
        hunterStr.text = hunter.str.ToString();
        strPointsGUI.text = hunter.strPoints.ToString();
        hunterAtp.text = hunter.atp.ToString();
        hunterHp.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        hunterSp.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
    }

    public void AllocatePoint_SPD()
    {
        hunter.AllocateToSpd(1);
        hunterSpd.text = hunter.spd.ToString();
        spdPointsGUI.text = hunter.spdPoints.ToString();
        hunterMov.text = hunter.mov.ToString();
        hunterEvd.text = (hunter.evd * 100) + "%";
        hunterHp.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        hunterSp.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
    }

    public void AllocatePoint_VIT()
    {
        hunter.AllocateToVit(1);
        hunterVit.text = hunter.vit.ToString();
        vitPointsGUI.text = hunter.vitPoints.ToString();
        hunterDfp.text = hunter.dfp.ToString();
        hunterHp.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        hunterSp.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
    }

    public void AllocatePoint_MNT()
    {
        hunter.AllocateToMnt(1);
        hunterMnt.text = hunter.mnt.ToString();
        mntPointsGUI.text = hunter.mntPoints.ToString();
        hunterRst.text = hunter.rst.ToString();
        hunterMnp.text = hunter.mnp.ToString();
        hunterHp.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        hunterSp.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
    }
}
