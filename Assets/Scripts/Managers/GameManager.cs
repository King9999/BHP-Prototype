using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameManager;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEditor.Progress;

/* handles the game state */
public class GameManager : MonoBehaviour
{
    //hunter stats
    //[Header("---Hunter---")]
    //public Hunter hunterPrefab;
    //public List<Hunter> hunters;

    [Header("---Dice---")]
    public Dice dice;
    public int attackerTotalRoll; //dice roll + ATP + any other bonuses
    public int defenderTotalRoll;   //single die roll + DFP + any other bonuses

    [Header("---UI---")]
    public TextMeshProUGUI hunterName;
    public TextMeshProUGUI hunterStr, hunterVit, hunterMnt, hunterSpd, hunterAtp, hunterDfp, hunterMnp, hunterRst, hunterEvd, hunterHp, hunterSp, hunterMov;
    public TextMeshProUGUI strPointsGUI, spdPointsGUI, vitPointsGUI, mntPointsGUI;
    public TextMeshProUGUI equippedWeaponText, equippedArmorText, equippedAccText;
    public TextMeshProUGUI monsterName;
    public TextMeshProUGUI monsterAtp, monsterDfp, monsterMnp, monsterRst, monsterEvd, monsterHp, monsterSp, monsterMov, monsterSpd;
    public TextMeshProUGUI attackerDieOneGUI, attackerDieTwoGUI, attackerAtp_total, attackerTotalAttackDamage;
    public TextMeshProUGUI defenderDieOneGUI, defenderDieTwoGUI, defenderDfp_total, defenderTotalDefense;

    [Header("---Inventory---")]
    public GameObject inventoryContainer;
    public GameObject skillContainer;
    public List<ItemObject> hunterInventory;
    public List<SkillObject> hunterSkills;

    [Header("---Combat---")]
    public Combat combatManager;

    [Header("---Loot Table---")]
    public LootTable lootTable;

    //states determine which UI is active
    public enum GameState { HunterSetup, Dungeon, Combat, Inventory}
    public GameState gameState;

    public static GameManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Singleton.instance.GameManager = this;      //master singleton captures
        Debug.Log("Hunter Manager status: " + Singleton.instance.HunterManager);
        gameState = GameState.HunterSetup;
        ChangeGameState(gameState);

        //MonsterManager mm = MonsterManager.instance;
        //CreateHunter();
        //mm.SpawnMonster(monsterLevel:1);
        //SetupMonsterUI(mm.activeMonsters[0]);

        //populate hunter inventory
        HunterManager hm = Singleton.instance.HunterManager;
        int i = 0;
        while (i < hm.hunters[0].inventory.Count)
        {
            hunterInventory[i].GetItemData(hm.hunters[0].inventory[i]);
            i++;
        }

        inventoryContainer.gameObject.SetActive(false);
        skillContainer.gameObject.SetActive(false);

        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeGameState(GameState gameState)
    {
        switch(gameState)
        {
            case GameState.HunterSetup:
                //show the setup screen where player allocates points.
                break;
        }
    }

    /*public void CreateHunter()
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
        //hunters[0].inventory.Add(im.GenerateWeapon());  //adding weapon as a test
    }*/

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
        monsterEvd.text = (monster.evd * 100) + "%";
        monsterMov.text = monster.mov.ToString();
        monsterSpd.text = monster.spd.ToString();
        monsterHp.text = monster.healthPoints + "/" + monster.maxHealthPoints;
        monsterSp.text = monster.skillPoints + "/" + monster.maxSkillPoints;
    }

    //Allocates a point to STR when clicked
    /*private void AllocatePoint_STR(Hunter hunter)
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
    }*/

    /*public void OnAllocateStrButtonPressed()
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
    }*/

    /* Rolls dice and displays results for hunter and monster */
    public void OnRollDiceButtonPressed()
    {
        MonsterManager mm = Singleton.instance.MonsterManager;
        HunterManager hm = Singleton.instance.HunterManager;
        combatManager.StartCombat(hm.hunters[0], mm.activeMonsters[0]);
        //get rolls from both attacker and defender
        /*MonsterManager mm = MonsterManager.instance;
        //int diceResult = dice.RollDice();
        attackerTotalRoll = GetTotalRoll_Attacker(hunters[0]);
        attackerDieOneGUI.text = dice.die1.ToString();
        attackerDieTwoGUI.text = dice.die2.ToString();
        attackerAtp_total.text = "ATP\n+" + hunters[0].atp;
        attackerTotalAttackDamage.text = attackerTotalRoll.ToString();

        //defender
        defenderTotalRoll = GetTotalRoll_Defender(mm.activeMonsters[0]);
        defenderDieOneGUI.text = dice.die1.ToString();
        defenderDieTwoGUI.text = dice.die2.ToString();
        defenderDfp_total.text = "DFP\n+" + mm.activeMonsters[0].dfp;
        defenderTotalDefense.text = defenderTotalRoll.ToString();*/
    }

    private int GetTotalRoll_Attacker(Character character)
    {

        int diceResult = dice.RollDice();

        if (character.TryGetComponent<Hunter>(out Hunter hunter))
        {
            return diceResult + (int)hunter.atp;
        }
        else if (character.TryGetComponent<Monster>(out Monster monster))
        {
            return diceResult + (int)monster.atp;
        }
        else
            return 0;
    }

    private int GetTotalRoll_Defender(Character character)
    {
        //roll 2 dice if character is defending, i.e. they forfeit their chance to counterattack.
        int dieResult = character.characterState == Character.CharacterState.Guarding ? dice.RollDice() : dice.RollSingleDie();


        if (character.TryGetComponent<Hunter>(out Hunter hunter))
        {
            return dieResult + (int)hunter.dfp;
        }
        else if (character.TryGetComponent<Monster>(out Monster monster))
        {
            return dieResult + (int)monster.dfp;
        }
        else
            return 0;
    }
}
