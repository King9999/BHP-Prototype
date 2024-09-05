using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static GameManager;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;

/* handles the game state */
public class GameManager : MonoBehaviour
{
    //hunter stats
    //[Header("---Hunter---")]
    //public Hunter hunterPrefab;
    //public List<Hunter> hunters;
    [Header("---Camera---")]
    public Camera gameCamera;       //isometric camera. Use this to move camera around the scene.
    Vector3 defaultCameraPos { get; } = new Vector3(0, 5, 0);
    bool moveCameraToCharacter = false;

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

    [Header("---Movement & Attack Tile---")]
    public GameObject moveTileContainer;
    public GameObject moveTilePrefab;
    public GameObject selectTilePrefab;               //used to highligh move or attack tile.
    public GameObject selectTile;
    public List<Character> turnOrder;
    int currentCharacter;
    public List<Vector3> movementPositions, attackPositions;     //holds valid positions for moving and attacking
    public List<GameObject> moveTileList, moveTileBin;          //bin is used for recycling instantiated move tiles

    bool runMovementCheck, moveTilesActive;

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

        /**** USE THE NEXT 4 LINES TO GET SEED FOR BUG TESTING*****/
        //System.Random random = new System.Random();
        //int seed = 1232703070; // random.Next();
        //Random.InitState(seed);
        //Debug.Log("Seed: " + seed);

        //create dungeon
        Dungeon dungeon = Singleton.instance.Dungeon;
        dungeon.CreateDungeon();

        //TODO: Set up turn order based on characters' SPD.
        HunterManager hm = Singleton.instance.HunterManager;
        
        //add all hunters to turn order, then sort.
        foreach(Character hunter in hm.hunters)
        {
            turnOrder.Add(hunter);
        }

        turnOrder.OrderByDescending(x => x.spd);    //TODO: Check to make sure this worked
        
        //MonsterManager mm = MonsterManager.instance;
        //CreateHunter();
        //mm.SpawnMonster(monsterLevel:1);
        //SetupMonsterUI(mm.activeMonsters[0]);

        //populate hunter inventory
        //HunterManager hm = Singleton.instance.HunterManager;
        /*int i = 0;
        while (i < hm.hunters[0].inventory.Count)
        {
            hunterInventory[i].GetItemData(hm.hunters[0].inventory[i]);
            i++;
        }*/

        inventoryContainer.gameObject.SetActive(false);
        skillContainer.gameObject.SetActive(false);
        moveTileContainer.name = "Move Tiles";
        selectTile = Instantiate(selectTilePrefab);
        selectTile.SetActive(false);
        //ShowMovementRange(hm.hunters[0], 1);
        //runMovementCheck = true;

        //focus camera on the first active character.
        gameCamera.transform.position = defaultCameraPos;
        moveCameraToCharacter = true;
        currentCharacter = 0;
        StartCoroutine(TakeAction(turnOrder[currentCharacter]));
        dice.dieImages[0].sprite = dice.diceSprites[0];
        dice.dieImages[1].sprite = dice.diceSprites[0];
        //gameCamera.transform.position = new Vector3(newCamPos.x - 4, 5, newCamPos.z + 4);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*if (runMovementCheck)
        {
            runMovementCheck = false;
            HunterManager hm = Singleton.instance.HunterManager;
            //ShowMovementRange(hm.hunters[0], 4);

            Dungeon dun = Singleton.instance.Dungeon;
            List<Vector3> moveRange = ShowMoveRange(dun.dungeonGrid, ActiveCharacter(), 3);
            GameObject moveTileContainer = new GameObject("Move Tiles");
            foreach (Vector3 pos in moveRange)
            {
                GameObject tile = Instantiate(moveTilePrefab, moveTileContainer.transform);
                tile.transform.position = new Vector3(pos.x, 0.6f, pos.z);
            }
        }*/
    }

    private void Update()
    {
        /*if (moveCameraToCharacter == true)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            MoveCameraToCharacter(hm.hunters[0]);
        }*/
        if (runMovementCheck)
        {
            runMovementCheck = false;
            HunterManager hm = Singleton.instance.HunterManager;

            Dungeon dun = Singleton.instance.Dungeon;
            int totalMove = ActiveCharacter().mov + dice.RollSingleDie();
            List<Vector3> moveRange = ShowMoveRange(dun.dungeonGrid, ActiveCharacter(), totalMove);
            Debug.Log("Total Move: " + totalMove);

            moveTileList.Clear();
            foreach (Vector3 pos in moveRange)
            {
                //if there are existing move tile objects, activate those first before instantiating new ones.
                if (moveTileBin.Count > 0)
                {
                    GameObject lastTile = moveTileBin[moveTileBin.Count - 1];
                    lastTile.SetActive(true);
                    lastTile.transform.position = new Vector3(pos.x, 0.6f, pos.z);
                    moveTileList.Add(lastTile);
                    moveTileBin.Remove(lastTile);
                }
                else
                {
                    GameObject tile = Instantiate(moveTilePrefab, moveTileContainer.transform);
                    tile.transform.position = new Vector3(pos.x, 0.6f, pos.z);
                    moveTileList.Add(tile);
                }
                
            }
            moveTilesActive = true;
            selectTile.SetActive(true);
        }

        //use mouse to check for move/attack tiles.
        if (moveTilesActive)
        {
            //Debug.Log(gameCamera.ScreenToWorldPoint(Input.mousePosition));
            Vector3 mousePos = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            //foreach (GameObject tile in moveTileList)
            //{
            if (Physics.Raycast(mouseRay, out RaycastHit hitTile) && hitTile.collider.CompareTag("Move Tile"))
            {
                Vector3 tilePos = hitTile.transform.position;
                selectTile.transform.position = new Vector3(tilePos.x, 0.7f, tilePos.z);

                /*foreach (GameObject tile in moveTileList)
                {
                    if (hitTile.collider == tile)
                    {
                        //change colour of tile
                        Vector3 tilePos = tile.transform.position;

                        selectTile.transform.position = new Vector3(tilePos.x, 0.7f, tilePos.z);
                    }
                    
                }*/
                        
            }
            //}
        }
    }

    public Character ActiveCharacter()
    {
        return turnOrder[currentCharacter];
    }

    public void GetMoveRange()
    {
        runMovementCheck = true;
    }

    /// <summary>
    /// Moves isometric camera to the active character (hunter or monster).
    /// </summary>
    /// <param name="character">The character the camera will focus on.</param>
    IEnumerator MoveCameraToCharacter(Character character)
    {
        //if (moveCameraToCharacter == false)
            //return null;
        
        Vector3 newCamPos = new Vector3(character.transform.position.x - 4, 5, character.transform.position.z - 6);
        float speed = 16;

        while (gameCamera.transform.position != newCamPos)
        {
            gameCamera.transform.position = Vector3.MoveTowards(gameCamera.transform.position, newCamPos, speed * Time.deltaTime);
            yield return null;
        }

        moveCameraToCharacter = false;

        /*if (gameCamera.transform.position == newCamPos)
        {
            moveCameraToCharacter = false;
        }*/
       
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

    
    public List<Vector3> ShowMoveRange(char[,] grid, Character character, int spaceCount)
    {
        List<Vector3>  validPositions = new List<Vector3>();
        int spaceCost = 1;    //value changes if a gap needs to be crossed.

        int startRow = character.room.row;
        int startCol = character.room.col;

        Debug.Log("Max Rows " + grid.GetLength(0));
        Debug.Log("Max Cols " + grid.GetLength(1));

        //search in the cardinal directions. At each point in the grid, search surrounding points for walkable space.
        /*****search right******/
        int currentCol = startCol;
        int currentRow = startRow;
        int currentSpaceCount = 0;
        int consecutiveGaps = 0;        //tracks number of invalid spaces.
        int nextSpace = 0;              //used to reset currentSpaceCount

        //the first condition checks if we're at the edge of the grid. If true, we can't move in that direction
        //and must check the next direction.
        while (currentCol + 1 < grid.GetLength(1) && currentSpaceCount < spaceCount)
        {
            currentCol++;

            //add new position. If the previous space was invalid, we add 3 + number of invalid spaces to space count because
            //it takes that many spaces to get around them.
            if (grid[currentRow, currentCol] == '1')
            {
                if (grid[currentRow, currentCol - 1] == '0')
                {
                    currentSpaceCount += 3 + consecutiveGaps;
                    consecutiveGaps = 0;
                }
                else
                    currentSpaceCount++;

                //check if we still have space to move here 
                if (currentSpaceCount <= spaceCount)
                    validPositions.Add(GetRoomPosition(currentRow, currentCol));
            }
            else
            {
                //skip this space
                consecutiveGaps++;
                continue;
                //currentSpaceCount += 1;
            }

            /* search up and down until we reach total moves or we go out of bounds */
            //search up
            currentRow = startRow;
            consecutiveGaps = 0;
            nextSpace = currentSpaceCount;  //track what current space is so it can be reset when searching in other direction.
            while (currentRow - 1 >= 0 && currentSpaceCount < spaceCount)
            {
                currentRow--;
                if (grid[currentRow, currentCol] == '1')  //search up
                {
                    if (grid[currentRow + 1, currentCol] == '0')
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                        validPositions.Add(GetRoomPosition(currentRow, currentCol));
                }
                else
                {
                    consecutiveGaps++;
                    continue;
                }

            }

            //search down
            currentRow = startRow;
            consecutiveGaps = 0;
            currentSpaceCount = nextSpace;
            while (currentRow + 1 < grid.GetLength(0) && currentSpaceCount < spaceCount)
            {
                currentRow++;
                if (grid[currentRow, currentCol] == '1')
                {
                    if (grid[currentRow - 1, currentCol] == '0')
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                        validPositions.Add(GetRoomPosition(currentRow, currentCol));
                }
                else
                {
                    consecutiveGaps++;
                    continue;
                }
            }

            //reset space count
            currentRow = startRow;
            currentSpaceCount = nextSpace;
            consecutiveGaps = 0;
        }

        /*****check left*****/
        currentCol = startCol;
        currentRow = startRow;
        currentSpaceCount = 0;
        consecutiveGaps = 0;
        while (currentCol - 1 >= 0 && currentSpaceCount < spaceCount)
        {
            currentCol--;
            //add new position
            if (grid[currentRow, currentCol] == '1')
            {
                if (grid[currentRow, currentCol + 1] == '0')
                {
                    currentSpaceCount += 3 + consecutiveGaps;
                    consecutiveGaps = 0;
                }
                else
                    currentSpaceCount++;
                //check if we still have space to move here 
                if (currentSpaceCount <= spaceCount)
                {
                    Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }
            }
            else
            {
                consecutiveGaps++;
                continue;
            }

            //check surrounding spaces and record their equivalent positions in world space
            //search up
            currentRow = startRow;
            consecutiveGaps = 0;
            nextSpace = currentSpaceCount;
            while (currentRow - 1 >= 0 && currentSpaceCount < spaceCount)
            {
                currentRow--;
                if (grid[currentRow, currentCol] == '1')
                {
                    if (grid[currentRow + 1, currentCol] == '0')
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                        if (!validPositions.Contains(newPos))
                            validPositions.Add(newPos);
                    }
                }
                else
                {
                    consecutiveGaps++;
                    continue;
                }

            }

            //search down
            currentRow = startRow;
            consecutiveGaps = 0;
            currentSpaceCount = nextSpace;
            while(currentRow + 1 < grid.GetLength(0) && currentSpaceCount < spaceCount)
            {
                currentRow++;
                if (grid[currentRow, currentCol] == '1')
                {
                    if (grid[currentRow - 1, currentCol] == '0')
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                        if (!validPositions.Contains(newPos))
                            validPositions.Add(newPos);
                    }
                }
                else
                {
                    consecutiveGaps++;
                    continue;
                }
            }

            //reset space count
            currentRow = startRow;
            currentSpaceCount = nextSpace;
            consecutiveGaps = 0;
        }

        /******check up (back)******/
        currentCol = startCol;
        currentRow = startRow;
        currentSpaceCount = 0;
        consecutiveGaps = 0;
        while (currentRow - 1 >= 0 && currentSpaceCount < spaceCount)
        {
            currentRow--;
            //add new position
            if (grid[currentRow, currentCol] == '1')
            {
                if (grid[currentRow + 1, currentCol] == '0')
                    currentSpaceCount += 3 + consecutiveGaps;
                else
                    currentSpaceCount++;

                //check if we still have space to move here 
                if (currentSpaceCount <= spaceCount)
                {
                    Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }
            }
            else
            {
                consecutiveGaps++;
                continue;
            }

            //search right
            currentCol = startCol;
            consecutiveGaps = 0;
            nextSpace = currentSpaceCount;
            while (currentCol + 1 < grid.GetLength(1) && currentSpaceCount < spaceCount)
            {
                currentCol++;
                if (grid[currentRow, currentCol] == '1')
                {
                    if (grid[currentRow, currentCol - 1] == '0')
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                        if (!validPositions.Contains(newPos))
                            validPositions.Add(newPos);
                    }
                }
                else
                {
                    consecutiveGaps++;
                    continue;
                }

            }

            //search left
            currentCol = startCol;
            currentSpaceCount = nextSpace;
            consecutiveGaps = 0;
            while(currentCol - 1 >= 0 && currentSpaceCount < spaceCount)
            {
                currentCol--;
                if (grid[currentRow, currentCol] == '1')
                {
                    if (grid[currentRow, currentCol + 1] == '0')
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                        if (!validPositions.Contains(newPos))
                            validPositions.Add(newPos);
                    }
                }
                else
                {
                    consecutiveGaps++;
                    continue;
                }
            }

            //reset space count
            currentCol = startCol;
            currentSpaceCount = nextSpace;
            consecutiveGaps = 0;
        }

        /******check down (front)******/
        currentCol = startCol;
        currentRow = startRow;
        currentSpaceCount = 0;
        consecutiveGaps = 0;
        while (currentRow + 1 < grid.GetLength(0) && currentSpaceCount < spaceCount)
        {
            currentRow++;
            //add new position
            if (grid[currentRow, currentCol] == '1')
            {
                if (grid[currentRow - 1, currentCol] == '0')
                    currentSpaceCount += 3 + consecutiveGaps;
                else
                    currentSpaceCount++;

                //check if we still have space to move here 
                if (currentSpaceCount <= spaceCount)
                {
                    Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }
            }
            else
            {
                consecutiveGaps++;
                continue;
            }

            //search right
            currentCol = startCol;
            consecutiveGaps = 0;
            nextSpace = currentSpaceCount;
            while (currentCol + 1 < grid.GetLength(1) && currentSpaceCount < spaceCount)
            {
                currentCol++;
                if (grid[currentRow, currentCol] == '1')
                {
                    if (grid[currentRow, currentCol - 1] == '0')
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                        if (!validPositions.Contains(newPos))
                            validPositions.Add(newPos);
                    }
                }
                else
                {
                    consecutiveGaps++;
                    continue;
                }

            }

            //search left
            currentCol = startCol;
            currentSpaceCount = nextSpace;
            consecutiveGaps = 0;
            while (currentCol - 1 >= 0 && currentSpaceCount < spaceCount)
            {
                currentCol--;
                if (grid[currentRow, currentCol] == '1')
                {
                    if (grid[currentRow, currentCol + 1] == '0')
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                        if (!validPositions.Contains(newPos))
                            validPositions.Add(newPos);
                    }
                }
                else
                {
                    consecutiveGaps++;
                    continue;
                }
            }

            //reset space count
            currentCol = startCol;
            currentSpaceCount = nextSpace;
            consecutiveGaps = 0;
        }

        /* Next, we do diagonal checks to find any remaining available spaces. Iterate through cardinal directions
         * like before, except there's no extra cost for moving into invalid spaces. Also, we must check the character's
         starting position. */
        /*****check right & up*****/
        /*currentCol = startCol;
        currentRow = startRow;
        currentSpaceCount = 0;
        int nextCol = 0;  //tracks which column to start on after every loop.
        int nextSpace = 0;
        while (currentCol < grid.GetLength(1) && currentSpaceCount < spaceCount)
        {
            while (currentRow - 1 >= 0 && currentCol + 1 < grid.GetLength(1) && currentSpaceCount + 2 <= spaceCount)
            {
                currentRow--;
                currentCol++;
                currentSpaceCount += 2;     //2 is added because it takes 2 spaces to move diagonally.
                                            //add new position
                if (grid[currentRow, currentCol] == '1')
                {
                    Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }
            }
            //start over from the next column
            nextCol++;
            nextSpace++;
            currentSpaceCount = nextSpace;
            currentCol = startCol + nextCol;
            currentRow = startRow;
        }*/

        /*****check right & down*****/
        /*currentCol = startCol;
        currentRow = startRow;
        currentSpaceCount = 0;
        nextCol = 0;
        nextSpace = 0;
        while (currentCol < grid.GetLength(1) && currentSpaceCount < spaceCount)
        {
            while (currentRow + 1 < grid.GetLength(0) && currentCol + 1 < grid.GetLength(1) && currentSpaceCount + 2 <= spaceCount)
            {
                currentRow++;
                currentCol++;
                currentSpaceCount += 2;     //2 is added because it takes 2 spaces to move diagonally.
                                            //add new position
                if (grid[currentRow, currentCol] == '1')
                {
                    Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }
            }
            //start over from the next column
            nextCol++;
            nextSpace++;
            currentSpaceCount = nextSpace;
            currentCol = startCol + nextCol;
            currentRow = startRow;
        }*/

        /*****check left & up*****/
        /*currentCol = startCol;
        currentRow = startRow;
        currentSpaceCount = 0;
        nextCol = 0;
        nextSpace = 0;
        while (currentCol > 0 && currentSpaceCount < spaceCount)
        {
            while (currentRow - 1 >= 0 && currentCol - 1 >= 0 && currentSpaceCount + 2 <= spaceCount)
            {
                currentRow--;
                currentCol--;
                currentSpaceCount += 2;     //2 is added because it takes 2 spaces to move diagonally.
                                            //add new position
                if (grid[currentRow, currentCol] == '1')
                {
                    Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }
            }
            //start over from the next column
            nextCol--;
            nextSpace++;
            currentSpaceCount = nextSpace;
            currentCol = startCol + nextCol;
            currentRow = startRow;
        }*/

        /*****check left & down*****/
        /*currentCol = startCol;
        currentRow = startRow;
        currentSpaceCount = 0;
        nextCol = 0;
        nextSpace = 0;
        while (currentCol > 0 && currentSpaceCount < spaceCount)
        {
            while (currentRow + 1 < grid.GetLength(0) && currentCol - 1 >= 0 && currentSpaceCount + 2 <= spaceCount)
            {
                currentRow++;
                currentCol--;
                currentSpaceCount += 2;     //2 is added because it takes 2 spaces to move diagonally.
                                            //add new position
                if (grid[currentRow, currentCol] == '1')
                {
                    Vector3 newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }
            }
            //start over from the next column
            nextCol--;
            nextSpace++;
            currentSpaceCount = nextSpace;
            currentCol = startCol + nextCol;
            currentRow = startRow;
        }*/

        return validPositions;
    }

    Vector3 GetRoomPosition(int row, int col)
    {
        bool roomFound = false;
        int i = 0;
        Room room = null;
        Dungeon dungeon = Singleton.instance.Dungeon;
        while (!roomFound && i < dungeon.dungeonRooms.Count)
        {
            room = dungeon.dungeonRooms[i];
            if (room.row == row && room.col == col)
            {
                roomFound = true;
            }
            else
            {
                i++;
            }
        }

        return room.transform.position;
    }

    Vector3 GetRoomsInDirection(RaycastHit hit)
    {
        //List<Vector3> positions = new List<Vector3>();
       // for (int i = 0; i < hit.Length; i++)
        //{
            Vector3 roomPos = hit.transform.position;
            //positions.Add(roomPos);
            //Debug.Log("leftCast Room " + i + " pos: " + roomPos + "\n");
            GameObject tile = Instantiate(moveTilePrefab);
            tile.transform.position = new Vector3(roomPos.x, 0.6f, roomPos.z);
       //}

        return roomPos;
    }

    //Next character in the turn order takes action. The camera is centered on the active character and 
    //a menu is displayed if the character is controlled by a player. Otherwise, CPU takes action.
    IEnumerator TakeAction(Character character)
    {
        //move camera to the active character
        yield return MoveCameraToCharacter(character);

        //once complete, show the menu if the active character is a player. Otherwise, CPU takes action.
        HunterManager hm = Singleton.instance.HunterManager;
        hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.Default);
        //hm.ui.ShowHunterMenu(true, character);
    }
}
