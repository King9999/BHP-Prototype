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
        gameState = GameState.Dungeon;
        ChangeGameState(gameState);

        //create dungeon
        Dungeon dungeon = Singleton.instance.Dungeon;
        dungeon.CreateDungeon();

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

       //ShowMovementRange(hm.hunters[0], 1);


    }

    // Update is called once per frame
    void Update()
    {
        HunterManager hm = Singleton.instance.HunterManager;
        ShowMovementRange(hm.hunters[0], 1);
    }

    private void FixedUpdate()
    {
        //HunterManager hm = Singleton.instance.HunterManager;
        //  ShowMovementRange(hm.hunters[0], 1);
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

        if (character is Hunter hunter /*character.TryGetComponent<Hunter>(out Hunter hunter)*/)
        {
            return diceResult + (int)hunter.atp;
        }
        else if (character is Monster monster /*character.TryGetComponent<Monster>(out Monster monster)*/)
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

        if (character is Hunter hunter /*character.TryGetComponent<Hunter>(out Hunter hunter)*/)
        {
            return dieResult + (int)hunter.dfp;
        }
        else if (character is Monster monster /*character.TryGetComponent<Monster>(out Monster monster)*/)
        {
            return dieResult + (int)monster.dfp;
        }
        else
            return 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="character">The hunter or monster who is going to move.</param>
    /// <param name="spaceCount">The number of spaces the character can move.</param>
    /// <returns>The valid positions the character can move to.</returns>
    public List<Vector3> ShowMovementRange(Character character, int spaceCount)
    {
        List<Vector3> validPositions = new List<Vector3>();
        //search surrounding rooms for valid spaces, starting from character's location.
        Room currentRoom = character.room;
        Vector3 currentPos = currentRoom.transform.position;
        //Debug.Log("Current room pos: " + currentPos);
        //Vector3 leftRoomPos = new Vector3(currentPos.x - 2, currentPos.y, currentPos.z);
        //Vector3 rightRoomPos = new Vector3(currentPos.x + 2, currentPos.y, currentPos.z);
        //Vector3 backRoomPos = new Vector3(currentPos.x, currentPos.y, currentPos.z + 2);
        //Vector3 frontRoomPos = new Vector3(currentPos.x, currentPos.y, currentPos.z - 2);
        float distance = 1.9f;
        Vector3 xOffset = new Vector3 (0.1f, 0, 0);
        Vector3 zOffset = new Vector3(0, 0, 0.1f);
        Ray leftRay = new Ray(currentPos, /*currentRoom.nodes[currentRoom.LEFT].pos.transform.position - xOffset,*/ Vector3.left);
        //Debug.Log("Left Ray: " + leftRay);
        Ray rightRay = new Ray(currentPos, /*currentRoom.nodes[currentRoom.RIGHT].pos.transform.position + xOffset,*/ Vector3.right);
        Ray backRay = new Ray(currentPos, /*currentRoom.nodes[currentRoom.BACK].pos.transform.position - zOffset,*/ Vector3.back);
        Ray frontRay = new Ray(currentPos, /*currentRoom.nodes[currentRoom.FORWARD].pos.transform.position + zOffset,*/ Vector3.forward);
        //Collider[] rooms = Physics.OverlapSphere(currentPos, 1f, LayerMask.GetMask("Room"));
        //Debug.Log("rooms: " + rooms.Length);
        

        //does a room exist at these locations?
        //Debug.Log("Current Location: " + currentRoom.transform.position);
        //Debug.DrawRay(character.transform.position, Vector3.left, Color.blue, 10000000, false);
        
        Debug.DrawRay(currentPos, /*currentRoom.nodes[currentRoom.LEFT].pos.transform.position - xOffset,*/ Vector3.left * distance, Color.blue, 10000000);
        Debug.DrawRay(currentPos, Vector3.right * distance, Color.blue, 10000000);
        Debug.DrawRay(currentPos, Vector3.back * distance, Color.blue, 10000000);
        Debug.DrawRay(currentPos, Vector3.forward * distance, Color.blue, 10000000);
        int layerMask = 1 << 3;
        //Debug.Log("Layer Mask: " + LayerMask.GetMask("Room") + layerMask);
        if (Physics.Raycast(leftRay, out RaycastHit leftRoom, distance, LayerMask.GetMask("Room")))
        {
            Debug.Log("hit left " + leftRoom.collider + " at pos " + leftRoom.transform.position);
        }

        if (Physics.Raycast(rightRay, out RaycastHit rightRoom, distance, LayerMask.GetMask("Room")))
        {
            Debug.Log("hit right " + rightRoom.collider + " at pos " + rightRoom.transform.position);
        }

        if (Physics.Raycast(backRay, out RaycastHit backRoom, distance, LayerMask.GetMask("Room")))
        {
            Debug.Log("hit back" + backRoom.collider + " at pos " + backRoom.transform.position);
        }

        if (Physics.Raycast(frontRay, out RaycastHit frontRoom, distance, LayerMask.GetMask("Room")))
        {
            Debug.Log("hit front" + frontRoom.collider + " at pos " + frontRoom.transform.position);
        }
        //Debug.Log("left hit: " + leftPos + "\nright hit: " + rightPos + "\nback hit: " + backPos + "\nfront hit: " + frontPos);

        //display a blue tile to indicate where character can move.
        return validPositions;
    }
}
