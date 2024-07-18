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
    public Hunter hunterPrefab;
    public List<Hunter> hunters;



    // Start is called before the first frame update
    void Start()
    {
        MonsterManager mm = MonsterManager.instance;
        CreateHunter();
        mm.SpawnMonster(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateHunter()
    {
        Hunter hunter = Instantiate(hunterPrefab);
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
        hunters.Add(hunter);
    }

    //Allocates a point to STR when clicked
    private void AllocatePoint_STR(Hunter hunter)
    {
        hunter.AllocateToStr(1);
        hunterStr.text = hunter.str.ToString();
        strPointsGUI.text = hunter.strPoints.ToString();
        hunterAtp.text = hunter.atp.ToString();
        hunterHp.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        hunterSp.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
    }

    private void AllocatePoint_SPD(Hunter hunter)
    {
        hunter.AllocateToSpd(1);
        hunterSpd.text = hunter.spd.ToString();
        spdPointsGUI.text = hunter.spdPoints.ToString();
        hunterMov.text = hunter.mov.ToString();
        hunterEvd.text = (hunter.evd * 100) + "%";
        hunterHp.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        hunterSp.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
    }

    private void AllocatePoint_VIT(Hunter hunter)
    {
        hunter.AllocateToVit(1);
        hunterVit.text = hunter.vit.ToString();
        vitPointsGUI.text = hunter.vitPoints.ToString();
        hunterDfp.text = hunter.dfp.ToString();
        hunterHp.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        hunterSp.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
    }

    private void AllocatePoint_MNT(Hunter hunter)
    {
        hunter.AllocateToMnt(1);
        hunterMnt.text = hunter.mnt.ToString();
        mntPointsGUI.text = hunter.mntPoints.ToString();
        hunterRst.text = hunter.rst.ToString();
        hunterMnp.text = hunter.mnp.ToString();
        hunterHp.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        hunterSp.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
    }

    public void OnAllocateStrButtonPressed()
    {
        AllocatePoint_STR(hunters[0]);
    }

    public void OnAllocateVitButtonPressed()
    {
        AllocatePoint_VIT(hunters[0]);
    }

    public void OnAllocateSpdButtonPressed()
    {
        AllocatePoint_SPD(hunters[0]);
    }

    public void OnAllocateMntButtonPressed()
    {
        AllocatePoint_MNT(hunters[0]);
    }
}
