using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

/* handles the game state */
public class GameManager : MonoBehaviour
{
    //hunter stats
    [Header("---Hunter---")]
    public Hunter hunterPrefab;
    public List<Hunter> hunters;

    [Header("---Dice---")]
    public Dice dice;
    public int hunterTotalRoll; //dice roll + ATP + any other bonuses

    [Header("---UI---")]
    public TextMeshProUGUI hunterName;
    public TextMeshProUGUI hunterStr, hunterVit, hunterMnt, hunterSpd, hunterAtp, hunterDfp, hunterMnp, hunterRst, hunterEvd, hunterHp, hunterSp, hunterMov;
    public TextMeshProUGUI strPointsGUI, spdPointsGUI, vitPointsGUI, mntPointsGUI;
    public TextMeshProUGUI equippedWeaponText, equippedArmorText, equippedAccText;
    public TextMeshProUGUI monsterName;
    public TextMeshProUGUI monsterAtp, monsterDfp, monsterMnp, monsterRst, monsterEvd, monsterHp, monsterSp, monsterMov;
    public TextMeshProUGUI hunterDieOneGUI, hunterDieTwoGUI, hunterAtp_total, hunterTotalAttackDamage;

    // Start is called before the first frame update
    void Start()
    {
        MonsterManager mm = MonsterManager.instance;
        CreateHunter();
        mm.SpawnMonster(monsterLevel:1);
        SetupMonsterUI(mm.activeMonsters[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateHunter()
    {
        Hunter hunter = Instantiate(hunterPrefab);
        hunter.characterName = "King";
        hunter.name = "Test Hunter";
        hunter.InitializeStats();
        hunterStr.text = hunter.str.ToString();
        hunterSpd.text = hunter.spd.ToString();
        hunterVit.text = hunter.vit.ToString();
        hunterMnt.text = hunter.mnt.ToString();
        hunterName.text = hunter.characterName.ToString();
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

        //give hunter a weapon
        ItemManager im = ItemManager.instance;
        hunter.Equip(im.GenerateWeapon());
        equippedWeaponText.text = hunter.equippedWeapon.itemName;
        hunterAtp.text = hunter.atp.ToString();
        hunters.Add(hunter);
    }

    public void SetupMonsterUI(Monster monster)
    {
        if (monster == null)
        {
            Debug.Log("No monster to setup!");
            return;
        }

        monsterName.text = monster.characterName;
        monsterAtp.text = monster.atp.ToString();
        monsterDfp.text = monster.dfp.ToString();
        monsterMnp.text = monster.mnp.ToString();
        monsterRst.text = monster.rst.ToString();
        monsterEvd.text = monster.evd.ToString();
        monsterMov.text = monster.mov.ToString();
        monsterHp.text = monster.healthPoints + "/" + monster.maxHealthPoints;
        monsterSp.text = monster.skillPoints + "/" + monster.maxSkillPoints;
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

    /* Rolls dice and displays results for hunter and monster */
    public void OnRollDiceButtonPressed()
    {
        //int diceResult = dice.RollDice();
        hunterTotalRoll = GetTotalHunterRoll(hunters[0]);
        hunterDieOneGUI.text = dice.die1.ToString();
        hunterDieTwoGUI.text = dice.die2.ToString();
        hunterAtp_total.text = "ATP\n+" + hunters[0].atp;
        hunterTotalAttackDamage.text = hunterTotalRoll.ToString();
    }

    private int GetTotalHunterRoll(Hunter hunter)
    {
        int diceResult = dice.RollDice();
        return diceResult + (int)hunter.atp;
    }
}
