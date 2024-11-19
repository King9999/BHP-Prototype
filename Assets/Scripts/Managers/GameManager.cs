using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

/* handles the game state */
public class GameManager : MonoBehaviour
{
    public GameObject gameViewController;   //used to hide/show all gameobjects in the scene. Is hidden when combat scene is active.
    [Header("---Camera---")]
    public Camera gameCamera;               //isometric camera. Use this to move camera around the scene.
    private Vector3 defaultCameraPos { get; } = new Vector3(0, 5, -10); //Z value has to be -10 so sprites aren't cut off at certain angles.
    //bool moveCameraToCharacter = false;

    [Header("---Dice---")]
    public Dice dice;
    public int attackerTotalRoll; //dice roll + ATP + any other bonuses
    public int defenderTotalRoll;   //single die roll + DFP + any other bonuses

    [Header("---UI---")]
    [SerializeField]private TextMeshProUGUI seedText;           //for debugging only

    [Header("---Turn order and count---")]
    public List<Character> turnOrder;
    public int turnCount;                       //tracks how many turns have passed in total since the game started.

    [Header("---Combat---")]
    public Combat combatManager;

    [Header("---Movement & Attack Tile---")]
    public GameObject moveTileContainer, skillTileContainer;
    public GameObject moveTilePrefab, skillTilePrefab;
    public GameObject selectTilePrefab;               //used to highlight move or attack tile.
    public GameObject selectTile;
    private int currentCharacter;
    public List<Room> movementPositions, attackPositions;     //holds valid positions for moving and attacking
    public List<GameObject> moveTileList, moveTileBin, skillTileList, skillTileBin;          //bin is used for recycling instantiated move tiles

    bool runMovementCheck, runSkillCheck, moveTilesActive, skillTilesActive;
    public bool characterMoved, characterActed;    //acted includes attacking, using a skill or item.
    public ActiveSkill selectedSkill;     //the active skill being used after being selected from skill menu.
    public int movementMod;             //value that's added to character's roll when moving. Altered by cards and skills.


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
       // gameState = GameState.Dungeon;
        //ChangeGameState(gameState);

        /**** USE THE NEXT 5 LINES TO GET SEED FOR BUG TESTING*****/
        System.Random random = new System.Random();
        int seed = 654978383; // random.Next();
        Random.InitState(seed);
        Debug.LogFormat("Seed: {0}", seed);
        seedText.text = string.Format("Seed: {0}", seed);

        //create dungeon. dungeon mods are activated before the dungeon is generated.
        Dungeon dungeon = Singleton.instance.Dungeon;
        ItemManager im = Singleton.instance.ItemManager;
        //Item item = im.lootTable.GetItem(Table.ItemType.DungeonMod, "dungeonMod_increaseCpuItemChance");
        //im.dungeonMods.Add((DungeonMod)item);
        im.ActivateDungeonMods();
        im.SortTableWeight(im.lootTable.itemTables);
        dungeon.CreateDungeon();

        //Show UI for each hunter.
        HunterManager hm = Singleton.instance.HunterManager;

        hm.ui.ShowHunterHuds(true);

        //add all hunters to turn order, then sort.
        foreach (Character hunter in hm.hunters)
        {
            turnOrder.Add(hunter);
        }

        turnOrder = turnOrder.OrderByDescending(x => x.spd).ToList();    //TODO: Check to make sure this worked
        
        //MonsterManager mm = MonsterManager.instance;
        //CreateHunter();
        //mm.SpawnMonster(monsterLevel:1);
        //SetupMonsterUI(mm.activeMonsters[0]);


        //tile setup
        moveTileContainer.name = "Move Tiles";
        skillTileContainer.name = "Skill Tiles";
        selectTile = Instantiate(selectTilePrefab);
        selectTile.SetActive(false);

        //Update card count
        CardManager cm = Singleton.instance.CardManager;
        cm.UpdateDeckCount();

        //focus camera on the first active character.
        gameCamera.transform.position = defaultCameraPos;
        //moveCameraToCharacter = true;
        currentCharacter = 0;
        //StartCoroutine(TakeTurn(ActiveCharacter()));
        dice.dieImages[0].sprite = dice.diceSprites[0];
        dice.dieImages[1].sprite = dice.diceSprites[0];
        //gameCamera.transform.position = new Vector3(newCamPos.x - 4, 5, newCamPos.z + 4);

        //combat setup
        combatManager.InitSetup();
        combatManager.gameObject.SetActive(false);

        //testing item swap
        /*for (int i = 0; i < hm.MaxInventorySize; i++)
        {
            hm.hunters[0].inventory.Add(im.lootTable.GetItem(Table.ItemType.Consumable));
        }*/
        ChangeGameState(gameState = GameState.Dungeon);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //testing combat transition.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (Character character in turnOrder)
            {
                if (character == ActiveCharacter())
                    continue;

                ActiveCharacter().targetChar = character;
                ActiveCharacter().chosenSkill = ActiveCharacter().skills[0] as ActiveSkill;
            }
            ChangeGameState(gameState = GameState.Combat);
            /*Singleton.instance.attacker = ActiveCharacter();
            Singleton.instance.defender = ActiveCharacter().targetChar;
            SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
            //SceneManager.UnloadSceneAsync("Battle");          //use this to unload combat when it's done.
            gameViewController.SetActive(false);*/
        }
    }

    private void Update()
    {
        if (runMovementCheck)
        {
            runMovementCheck = false;
            //HunterManager hm = Singleton.instance.HunterManager;

            //Dungeon dun = Singleton.instance.Dungeon;
            int totalMove = ActiveCharacter().mov + dice.RollSingleDie() + movementMod;
            List<Room> moveRange = ShowMoveRange(ActiveCharacter(), totalMove);
            Debug.LogFormat("Total Move: {0}", totalMove);

            foreach (Room pos in moveRange)
            {
                //if there are existing move tile objects, activate those first before instantiating new ones.
                if (moveTileBin.Count > 0)
                {
                    GameObject lastTile = moveTileBin[0];
                    lastTile.SetActive(true);
                    lastTile.transform.position = new Vector3(pos.transform.position.x, 0.6f, pos.transform.position.z);
                    moveTileList.Add(lastTile);
                    moveTileBin.Remove(lastTile);
                }
                else
                {
                    GameObject tile = Instantiate(moveTilePrefab, moveTileContainer.transform);
                    tile.transform.position = new Vector3(pos.transform.position.x, 0.6f, pos.transform.position.z);
                    moveTileList.Add(tile);
                }
                
            }

            if (moveTileBin.Count <= 0)
            {
                moveTileBin.TrimExcess();
            }
            moveTilesActive = true;
            selectTile.SetActive(true);
        }

        //use mouse to check for move/attack tiles.
        if (moveTilesActive)
        {
            //Debug.Log(gameCamera.ScreenToWorldPoint(Input.mousePosition));
            //Vector3 mousePos = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Ray mouseRay = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitTile) && hitTile.collider.CompareTag("Move Tile"))
            {
                Vector3 tilePos = hitTile.transform.position;
                selectTile.transform.position = new Vector3(tilePos.x, 0.61f, tilePos.z);
                //Debug.Log("Select Tile at position " + selectTile.transform.position);
                        
            }

            //if mouse button is clicked, move hunter to chosen tile.
            if (Input.GetMouseButtonDown(0))
            {
                moveTilesActive = false;
                //clear move tiles
                for (int i = 0; i < moveTileList.Count; i++)
                {
                    moveTileList[i].SetActive(false);
                    moveTileBin.Add(moveTileList[i]);
                }
                moveTileList.Clear();
                moveTileList.TrimExcess();
                Debug.LogFormat("New destination: {0}", selectTile.transform.position);
                Vector3 destinationPos = new Vector3(selectTile.transform.position.x, 0, selectTile.transform.position.z);
                StartCoroutine(MoveCharacter(ActiveCharacter(), destinationPos));
            }
        }

        //skill range check
        if (runSkillCheck == true)
        {
            runSkillCheck = false;

            //Dungeon dun = Singleton.instance.Dungeon;
            List<Room> skillRange = ShowSkillRange(ActiveCharacter(), selectedSkill.minRange, selectedSkill.maxRange);
            Debug.LogFormat("{0}'s range: {1} min, {2} max", selectedSkill.skillName, selectedSkill.minRange, selectedSkill.maxRange);

            DisplaySkillTiles(skillRange);
            /*foreach (Room pos in skillRange)
            {
                //if there are existing move tile objects, activate those first before instantiating new ones.
                if (skillTileBin.Count > 0)
                {
                    GameObject lastTile = skillTileBin[0];
                    lastTile.SetActive(true);
                    lastTile.transform.position = new Vector3(pos.transform.position.x, 0.6f, pos.transform.position.z);
                    skillTileList.Add(lastTile);
                    skillTileBin.Remove(lastTile);
                }
                else
                {
                    GameObject tile = Instantiate(skillTilePrefab, skillTileContainer.transform);
                    tile.transform.position = new Vector3(pos.transform.position.x, 0.6f, pos.transform.position.z);
                    skillTileList.Add(tile);
                }

            }

            if (skillTileBin.Count <= 0)
            {
                skillTileBin.TrimExcess();
            }*/
            skillTilesActive = true;
            selectTile.SetActive(true);
        }

        if (skillTilesActive)
        {
            Ray mouseRay = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitTile) && hitTile.collider.CompareTag("Skill Tile"))
            {
                Vector3 tilePos = hitTile.transform.position;
                selectTile.transform.position = new Vector3(tilePos.x, 0.61f, tilePos.z);
                //Debug.Log("Select Tile at position " + selectTile.transform.position);

            }

            //if mouse button is clicked, move hunter to chosen tile.
            if (Input.GetMouseButtonDown(0))
            {
                skillTilesActive = false;
                //clear skill tiles
                for (int i = 0; i < skillTileList.Count; i++)
                {
                    skillTileList[i].SetActive(false);
                    skillTileBin.Add(skillTileList[i]);
                }
                skillTileList.Clear();
                skillTileList.TrimExcess();
                Debug.LogFormat("Select Tile space: {0}", selectTile.transform.position);
                Vector3 destinationPos = new Vector3(selectTile.transform.position.x, 0, selectTile.transform.position.z);
                //get the room that was clicked, and check if a character was targeted.
                int j = 0;
                bool charFound = false;
                while (!charFound && j < turnOrder.Count)
                {
                    if (Mathf.Approximately(destinationPos.x, turnOrder[j].transform.position.x) && 
                        Mathf.Approximately(destinationPos.z, turnOrder[j].transform.position.z))
                    //if (destinationPos.x == turnOrder[j].transform.position.x && destinationPos.z == turnOrder[j].transform.position.z)
                    {
                        charFound = true;
                        ActiveCharacter().targetChar = turnOrder[j];
                        Debug.LogFormat(turnOrder[j].gameObject, "Targeting {0}", turnOrder[j]);
                    }
                    else
                    {
                        j++;
                    }
                }

                if (charFound)
                {
                    selectTile.SetActive(false);
                    ChangeGameState(gameState = GameState.Combat);
                    //StartCombat(ActiveCharacter(), turnOrder[j]);
                }
                //StartCoroutine(MoveCharacter(ActiveCharacter(), destinationPos));
                //TODO: Add coroutine to begin combat
            }
        }
    }

    public void DisplaySkillTiles(List<Room> skillRooms)
    {
        foreach (Room pos in skillRooms)
        {
            //if there are existing skill tile objects, activate those first before instantiating new ones.
            if (skillTileBin.Count > 0)
            {
                GameObject lastTile = skillTileBin[0];
                lastTile.SetActive(true);
                lastTile.transform.position = new Vector3(pos.transform.position.x, 0.6f, pos.transform.position.z);
                skillTileList.Add(lastTile);
                skillTileBin.Remove(lastTile);
            }
            else
            {
                GameObject tile = Instantiate(skillTilePrefab, skillTileContainer.transform);
                tile.transform.position = new Vector3(pos.transform.position.x, 0.6f, pos.transform.position.z);
                skillTileList.Add(tile);
            }

        }

        if (skillTileBin.Count <= 0)
        {
            skillTileBin.TrimExcess();
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

    public void GetSkillRange()
    {
        runSkillCheck = true;
    }

    public void ChangeGameState(GameState gameState)
    {
        switch(gameState)
        {
            
            case GameState.HunterSetup:
                //show the setup screen where player allocates points.
                break;

            case GameState.Dungeon:
                StartCoroutine(CheckCharacterState(ActiveCharacter()));
                break;

            case GameState.Combat:
                characterActed = true;
                Singleton s = Singleton.instance;
                s.attacker = ActiveCharacter();
                s.defender = ActiveCharacter().targetChar;
                s.attackerLastRoom = ActiveCharacter().room;
                s.defenderLastRoom = ActiveCharacter().targetChar.room;
                SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
                //SceneManager.UnloadSceneAsync("Battle");          //use this to unload combat when it's done.
                gameViewController.SetActive(false);
                //StartCombat(ActiveCharacter(), ActiveCharacter().targetChar);
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

        /*monsterName.text = monster.characterName;
        monsterAtp.text = monster.atp.ToString();
        monsterDfp.text = monster.dfp.ToString();
        monsterMnp.text = monster.mnp.ToString();
        monsterRst.text = monster.rst.ToString();
        monsterEvd.text = (monster.evd * 100) + "%";
        monsterMov.text = monster.mov.ToString();
        monsterSpd.text = monster.spd.ToString();
        monsterHp.text = monster.healthPoints + "/" + monster.maxHealthPoints;
        monsterSp.text = monster.skillPoints + "/" + monster.maxSkillPoints;*/
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

    public void StartCombat(Character attacker, Character defender)
    {
        characterActed = true;
        selectTile.SetActive(false);
        combatManager.gameObject.SetActive(true);
        combatManager.StartCombat(attacker, defender);
    }

    void CheckForEntities(Room room, Character character)
    {
        if (room.entity == null)
            return;

        if (room.entity is Entity_TreasureChest chest && character is Hunter hunter)
        {
            //if it hasn't been opened, then character takes it.
            chest.OpenChest(hunter);

            //TODO: if item is target item, show some feedback.
            /*if (!chest.playerInteracted)
            {
                hunter.inventory.Add(chest.item);
                
                chest.item = null;  //make sure by doing this, the item in inventory isn't also null
                chest.playerInteracted = true;
                
                
            }*/
        }

        //terminal check

        //exit check
        if (room.entity is Entity_Exit exit)
        {
            exit.TeleportCharacter(character);
        }
    }

    /// <summary>
    /// Displays the active character's movement range. This method takes into account any invalid spaces.
    /// </summary>
    /// <param name="character">The active character.</param>
    /// <param name="spaceCount">The total number of spaces the character can move.</param>
    /// <returns>The list of valid spaces the character can move.</returns>
    public List<Room> ShowMoveRange(Character character, int spaceCount)
    {
        List<Room>  validPositions = new List<Room>();
        Dungeon dungeon = Singleton.instance.Dungeon;
        char[,] grid = dungeon.dungeonGrid;

        int startRow = character.room.row;
        int startCol = character.room.col;

        //Debug.Log("Max Rows " + grid.GetLength(0));
        //Debug.Log("Max Cols " + grid.GetLength(1));

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
            if (grid[currentRow, currentCol].Equals('1'))
            {
                if (grid[currentRow, currentCol - 1].Equals('0'))
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
            }

            /* search up and down until we reach total moves or we go out of bounds */
            //search up
            currentRow = startRow;
            consecutiveGaps = 0;
            nextSpace = currentSpaceCount;  //track what current space is so it can be reset when searching in other direction.
            while (currentRow - 1 >= 0 && currentSpaceCount < spaceCount)
            {
                currentRow--;
                if (grid[currentRow, currentCol].Equals('1'))  //search up
                {
                    if (grid[currentRow + 1, currentCol].Equals('0'))
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
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    if (grid[currentRow - 1, currentCol].Equals('0'))
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
            if (grid[currentRow, currentCol].Equals('1'))
            {
                if (grid[currentRow, currentCol + 1].Equals('0'))
                {
                    currentSpaceCount += 3 + consecutiveGaps;
                    consecutiveGaps = 0;
                }
                else
                    currentSpaceCount++;
                //check if we still have space to move here 
                if (currentSpaceCount <= spaceCount)
                {
                    Room newPos = GetRoomPosition(currentRow, currentCol);
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
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    if (grid[currentRow + 1, currentCol].Equals('0'))
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Room newPos = GetRoomPosition(currentRow, currentCol);
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
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    if (grid[currentRow - 1, currentCol].Equals('0'))
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Room newPos = GetRoomPosition(currentRow, currentCol);
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
            if (grid[currentRow, currentCol].Equals('1'))
            {
                if (grid[currentRow + 1, currentCol].Equals('0'))
                    currentSpaceCount += 3 + consecutiveGaps;
                else
                    currentSpaceCount++;

                //check if we still have space to move here 
                if (currentSpaceCount <= spaceCount)
                {
                    Room newPos = GetRoomPosition(currentRow, currentCol);
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
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    if (grid[currentRow, currentCol - 1].Equals('0'))
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Room newPos = GetRoomPosition(currentRow, currentCol);
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
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    if (grid[currentRow, currentCol + 1].Equals('0'))
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Room newPos = GetRoomPosition(currentRow, currentCol);
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
            if (grid[currentRow, currentCol].Equals('1'))
            {
                if (grid[currentRow - 1, currentCol].Equals('0'))
                    currentSpaceCount += 3 + consecutiveGaps;
                else
                    currentSpaceCount++;

                //check if we still have space to move here 
                if (currentSpaceCount <= spaceCount)
                {
                    Room newPos = GetRoomPosition(currentRow, currentCol);
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
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    if (grid[currentRow, currentCol - 1].Equals('0'))
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Room newPos = GetRoomPosition(currentRow, currentCol);
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
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    if (grid[currentRow, currentCol + 1].Equals('0'))
                    {
                        currentSpaceCount += 3 + consecutiveGaps;
                        consecutiveGaps = 0;
                    }
                    else
                        currentSpaceCount++;

                    //check if we still have space to move here 
                    if (currentSpaceCount <= spaceCount)
                    {
                        Room newPos = GetRoomPosition(currentRow, currentCol);
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

        return validPositions;
    }


    public List<Room> ShowSkillRange(Character character, int minRange, int maxRange)
    {
        List<Room> validPositions = new List<Room>();
        Dungeon dungeon = Singleton.instance.Dungeon;
        char[,] grid = dungeon.dungeonGrid;

        int startRow = character.room.row;
        int startCol = character.room.col;

        //Debug.Log("Max Rows " + grid.GetLength(0));
        //Debug.Log("Max Cols " + grid.GetLength(1));

        //search in the cardinal directions. At each point in the grid, search surrounding points for targetable space.
        /*****search right******/
        int currentCol = startCol;
        int currentRow = startRow;
        int currentSpaceCount = 0;
        //int consecutiveGaps = 0;        //tracks number of invalid spaces.
        int nextSpace = 0;              //used to reset currentSpaceCount

        //the first condition checks if we're at the edge of the grid. If true, we can't move in that direction
        //and must check the next direction.
        while (currentCol + 1 < grid.GetLength(1) && currentSpaceCount < maxRange)
        {
            currentCol++;
            currentSpaceCount++;

            //if we haven't reached the minimum range, this current space isn't valid. But we must still check surrounding spaces.
            if (currentSpaceCount >= minRange && grid[currentRow, currentCol].Equals('1'))
            {
                validPositions.Add(GetRoomPosition(currentRow, currentCol));

            }


            /* search up and down until we reach total moves or we go out of bounds */
            //search up
            currentRow = startRow;
            nextSpace = currentSpaceCount;  //track what current space is so it can be reset when searching in other direction.
            while (currentRow - 1 >= 0 && currentSpaceCount < maxRange)
            {
                currentRow--;
                currentSpaceCount++;
                if (grid[currentRow, currentCol].Equals('1'))  //search up
                {
                    validPositions.Add(GetRoomPosition(currentRow, currentCol));
                }
               
            }

            //search down
            currentRow = startRow;
            currentSpaceCount = nextSpace;
            while (currentRow + 1 < grid.GetLength(0) && currentSpaceCount < maxRange)
            {
                currentRow++;
                currentSpaceCount++;
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    validPositions.Add(GetRoomPosition(currentRow, currentCol));
                }
               
            }

            //reset space count
            currentRow = startRow;
            currentSpaceCount = nextSpace;
        }

        /*****check left*****/
        currentCol = startCol;
        currentRow = startRow;
        currentSpaceCount = 0;
        while (currentCol - 1 >= 0 && currentSpaceCount < maxRange)
        {
            currentCol--;
            currentSpaceCount++;
            if (currentSpaceCount >= minRange && grid[currentRow, currentCol].Equals('1'))
            {
                Room newPos = GetRoomPosition(currentRow, currentCol);
                if (!validPositions.Contains(newPos))
                    validPositions.Add(newPos);
            }
            

            //check surrounding spaces and record their equivalent positions in world space
            //search up
            currentRow = startRow;
            nextSpace = currentSpaceCount;
            while (currentRow - 1 >= 0 && currentSpaceCount < maxRange)
            {
                currentRow--;
                currentSpaceCount++;
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    Room newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }

            }

            //search down
            currentRow = startRow;
            currentSpaceCount = nextSpace;
            while (currentRow + 1 < grid.GetLength(0) && currentSpaceCount < maxRange)
            {
                currentRow++;
                currentSpaceCount++;
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    Room newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }
               
            }

            //reset space count
            currentRow = startRow;
            currentSpaceCount = nextSpace;
        }

        /******check up (back)******/
        currentCol = startCol;
        currentRow = startRow;
        currentSpaceCount = 0;
        while (currentRow - 1 >= 0 && currentSpaceCount < maxRange)
        {
            currentRow--;
            currentSpaceCount++;
            //add new position
            if (currentSpaceCount >= minRange && grid[currentRow, currentCol].Equals('1'))
            {
                Room newPos = GetRoomPosition(currentRow, currentCol);
                if (!validPositions.Contains(newPos))
                    validPositions.Add(newPos);
            }

            //search right
            currentCol = startCol;
            nextSpace = currentSpaceCount;
            while (currentCol + 1 < grid.GetLength(1) && currentSpaceCount < maxRange)
            {
                currentCol++;
                currentSpaceCount++;
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    Room newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }

            }

            //search left
            currentCol = startCol;
            currentSpaceCount = nextSpace;
            while (currentCol - 1 >= 0 && currentSpaceCount < maxRange)
            {
                currentCol--;
                currentSpaceCount++;
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    Room newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }
            }

            //reset space count
            currentCol = startCol;
            currentSpaceCount = nextSpace;
        }

        /******check down (front)******/
        currentCol = startCol;
        currentRow = startRow;
        currentSpaceCount = 0;
        while (currentRow + 1 < grid.GetLength(0) && currentSpaceCount < maxRange)
        {
            currentRow++;
            currentSpaceCount++;
            //add new position
            if (currentSpaceCount >= minRange && grid[currentRow, currentCol].Equals('1'))
            {
                Room newPos = GetRoomPosition(currentRow, currentCol);
                if (!validPositions.Contains(newPos))
                    validPositions.Add(newPos);
            }

            //search right
            currentCol = startCol;
            nextSpace = currentSpaceCount;
            while (currentCol + 1 < grid.GetLength(1) && currentSpaceCount < maxRange)
            {
                currentCol++;
                currentSpaceCount++;
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    Room newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }

            }

            //search left
            currentCol = startCol;
            currentSpaceCount = nextSpace;
            while (currentCol - 1 >= 0 && currentSpaceCount < maxRange)
            {
                currentCol--;
                currentSpaceCount++;
                if (grid[currentRow, currentCol].Equals('1'))
                {
                    Room newPos = GetRoomPosition(currentRow, currentCol);
                    if (!validPositions.Contains(newPos))
                        validPositions.Add(newPos);
                }
            }

            //reset space count
            currentCol = startCol;
            currentSpaceCount = nextSpace;
        }

        return validPositions;
    }

    Room GetRoomPosition(int row, int col)
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

        if (i >= dungeon.dungeonRooms.Count)
            return null;
        else
            return room;
        //return room.transform.position;
    }

    /// <summary>
    /// Updates hunter's super meter by given amount
    /// </summary>
    /// <param name="hunter"></param>
    /// <param name="amount">The amount of meter to give. max value is 1.</param>
    private void UpdateSuperMeter(Hunter hunter, float amount)
    {
        if (amount < 0 || amount > 1)
        {
            Debug.LogError("Meter amount must be between 0 and 1!");
            return;
        }

        HunterManager hm = Singleton.instance.HunterManager;
        bool defenderFound = false;
        int i = 0;
        while (!defenderFound && i < hm.hunters.Count)
        {
            if (hm.hunters[i] == hunter)
            {
                defenderFound = true;
                hm.ui.hunterHuds[i].superMeterUI.value += amount;
            }
            else
            {
                i++;
            }
        }
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


    public void EndTurn()
    {
        HunterManager hm = Singleton.instance.HunterManager;
        hm.ui.ShowHunterMenuContainer(false);
        currentCharacter = currentCharacter + 1 >= turnOrder.Count ? 0 : currentCharacter + 1;

        //clean up any buffs/debuffs/other effects or mods
        movementMod = 0;
        CardManager cm = Singleton.instance.CardManager;
        cm.selectedCard = null; //in case a card was used but wasn't triggered.
        hm.ui.activeCardText.text = "";

        StartCoroutine(TakeTurn(ActiveCharacter()));
    }

    //runs CheckCharacterState coroutine from elsewhere
    public void CharacterState(Character character)
    {
        StartCoroutine(CheckCharacterState(character));
    }

    //used by CPU characters only.
    public void MoveCPUCharacter(Character character, Vector3 destination, bool cpuAttacking = false)
    {
        StartCoroutine(MoveCharacter(character, destination, cpuAttacking));
    }

    #region Coroutines
    IEnumerator CheckCharacterState(Character character)
    {
        if (characterActed && characterMoved)
        {
            EndTurn();
        }
        else if (!characterActed && !characterMoved)
        {
            StartCoroutine(TakeTurn(character));
        }
        else
        {
            HunterManager hm = Singleton.instance.HunterManager;
            //disable move button if moved, or other buttons if character took action.
            if (characterMoved && !characterActed)
            {
                yield return MoveCameraToCharacter(character);
                
                if (!character.cpuControlled)
                {
                    //check if hunter has too many items in inventory.
                    if (character is Hunter hunter && hunter.inventory.Count > hm.MaxInventorySize)
                    {
                        hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.TooManyItems);
                    }
                    else
                    {
                        hm.ui.EnableButton(hm.ui.moveButton, false);
                        hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.Default);
                    }
                }
                else
                {
                    //cpu takes action
                    //does hunter have too many items?
                    if (character is Hunter hunter && hunter.inventory.Count > hm.MaxInventorySize)
                    {
                        Debug.LogFormat("{0} is removing extra item", character.characterName);
                        hm.ChangeCPUHunterState(hm.aiState = HunterManager.HunterAIState.RemovingExtraItem, hunter);
                    }
                    else
                    {
                        if (character.targetChar != null)
                        {
                            //attack
                            Debug.LogFormat("{0} is attacking {1}!", character.characterName, character.targetChar.characterName);
                            //StartCombat(character, character.targetChar);
                            hm.ChangeCPUHunterState(hm.aiState = HunterManager.HunterAIState.UseSkill, (Hunter)character);
                        }
                        else
                        {
                            EndTurn();
                        }
                    }
                    
                }
            }
            else if (!characterMoved && characterActed)
            {
                //move character
                if (character.cpuControlled)
                    hm.ChangeCPUHunterState(hm.aiState = HunterManager.HunterAIState.Moving, (Hunter)character);
            }
        }
    }

    //Next character in the turn order takes action. The camera is centered on the active character and 
    //a menu is displayed if the character is controlled by a player. Otherwise, CPU takes action.
    IEnumerator TakeTurn(Character character)
    {
        characterActed = false;
        characterMoved = false;
        turnCount++;
        Debug.LogFormat("------Turn {0}------", turnCount);

        //TODO: if monster spawns, they take their turn immediately.
        HunterManager hm = Singleton.instance.HunterManager;
        MonsterManager mm = Singleton.instance.MonsterManager;
        if (mm.TimeToSpawnMonster())
        {
            Monster monster = mm.SpawnMonster(hm.AverageHunterLevel());
            Debug.LogFormat("Monster is spawning. {0} is taking their turn", monster.characterName);
            yield return TakeTurn(monster);
        }
        //move camera to the active character
        yield return MoveCameraToCharacter(character);

        Debug.LogFormat("{0}'s turn", character.characterName);

        //update buffs/debuffs
        for(int i = 0; i < character.buffs.Count; i++)
        {
            character.buffs[i].UpdateEffect(character);
            if (character.buffs[i].hasDuration && character.buffs[i].currentDuration >= character.buffs[i].totalDuration)
            {
                character.buffs.Remove(character.buffs[i]);
                i--;
            }
        }

        for (int i = 0; i < character.debuffs.Count; i++)
        {
            character.debuffs[i].UpdateEffect(character);
            if (character.debuffs[i].hasDuration && character.debuffs[i].currentDuration >= character.debuffs[i].totalDuration)
            {
                character.debuffs.Remove(character.debuffs[i]);
                i--;
            }
            /*else
            {
                character.debuffs[i].CleanupEffect(character);
                character.debuffs.Remove(character.debuffs[i]);
                i--;
            }*/
        }


        //once complete, show the menu if the active character is a player. Otherwise, CPU takes action.
        //draw a card
        if (character is Hunter hunter)
        {
            CardManager cm = Singleton.instance.CardManager;
            cm.DrawCard(hunter, cm.deck);
            Debug.LogFormat("Hunter {0} drew a card", hunter.characterName);

            //gain super meter
            UpdateSuperMeter(hunter, hm.SuperMeterGain_turnStart);

            if (!hunter.cpuControlled)
            {
                hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.Default);
            }
            else
            {
                //CPU takes action.
                //activate any behaviour-specific abilities.
                hunter.cpuBehaviour.ActivateAbility(hunter);

                //Before moving, check if the hunter is already in attack range by checking all applicable skills.
                //Pick basic attack for checking, will choose another skill later
                List<Room> skillRange = new List<Room>();
                List<Character> targetChars = new List<Character>();
                List<ActiveSkill> activeSkills = new List<ActiveSkill>();
                for (int i = 0; i < hunter.skills.Count; i++)
                {
                    if (hunter.skills[i] is ActiveSkill activeSkill && activeSkill.skillCost <= hunter.skillPoints)
                    {
                        skillRange = ShowSkillRange(hunter, activeSkill.minRange, activeSkill.maxRange);
                        targetChars = hunter.CPU_CheckCharactersInRange(skillRange);
                        if (targetChars.Count > 0)
                        {
                            activeSkills.Add(activeSkill);
                        }
                    }
                    
                }
                //ActiveSkill basicAttack = hunter.skills[0] as ActiveSkill;
                //List<Room> skillRange = ShowSkillRange(hunter, basicAttack.minRange, basicAttack.maxRange);
                //List<Character> targetChars = hunter.CPU_CheckCharactersInRange(skillRange);
                
                if (activeSkills.Count > 0)
                {
                    int randSkill = Random.Range(0, activeSkills.Count);
                    int randTarget = Random.Range(0, targetChars.Count);
                    hunter.chosenSkill = activeSkills[randSkill];
                    hunter.targetChar = targetChars[randTarget];
                    hm.ChangeCPUHunterState(hm.aiState = HunterManager.HunterAIState.UseSkill, hunter);
                }
                else
                {
                    hm.ChangeCPUHunterState(hm.aiState = HunterManager.HunterAIState.Moving, hunter);
                }
            }
        }
        else  //this is a monster, which is always CPU-controlled.
        {

        }
        //hm.ui.ShowHunterMenu(true, character);
    }

    IEnumerator MoveCharacter(Character character, Vector3 destination, bool cpuAttacking = false)
    {
        //get character's current position and search adjacent rooms.
        //find the room whose position is closest to the destination and record it.
        //if there are two or more rooms with the same distance to the destination, pick a random room and discard the others.
        //if the room is the destination, move the player using MoveTowards. Must move from one room to the next until
        //destination is reached.
        Dungeon dungeon = Singleton.instance.Dungeon;
        
        List<Room> destinationRooms = new List<Room>();
        List<Room> attackRange = new List<Room>();


        char[,] grid = dungeon.dungeonGrid;
        int currentRow = character.room.row;
        int currentCol = character.room.col;
        bool destinationFound = false;

        if (cpuAttacking && character.targetChar != null)
        {
            //skill's origin is the target char, and CPU must walk into skill range to attack target.
            attackRange = ShowSkillRange(character.targetChar, character.chosenSkill.minRange, character.chosenSkill.maxRange);
        }

        while(!destinationFound)
        {
            List<Room> adjacentRooms = new List<Room>();

            //search up
            if (currentRow - 1 >= 0 && grid[currentRow - 1, currentCol].Equals('1'))
                adjacentRooms.Add(dungeon.GetRoom(currentRow - 1, currentCol));

            //search down
            if (currentRow + 1 < grid.GetLength(0) && grid[currentRow + 1, currentCol].Equals('1'))
                adjacentRooms.Add(dungeon.GetRoom(currentRow + 1, currentCol));

            //search left
            if (currentCol - 1 >= 0 && grid[currentRow, currentCol - 1].Equals('1'))
                adjacentRooms.Add(dungeon.GetRoom(currentRow, currentCol - 1));

            //search right
            if (currentCol + 1 < grid.GetLength(1) && grid[currentRow, currentCol + 1].Equals('1'))
                adjacentRooms.Add(dungeon.GetRoom(currentRow, currentCol + 1));

            //if we haven't found the destination, find the room closest to the destination
            int i = 0;
            bool adjacentRoomsFound = false;
            Room closestRoom = adjacentRooms[0];
            float shortestDistance = Vector3.Distance(destination, adjacentRooms[0].transform.position);

            while (!adjacentRoomsFound && i < adjacentRooms.Count)
            {

                Vector3 roomPos = new Vector3(adjacentRooms[i].transform.position.x, 0, adjacentRooms[i].transform.position.z);
                if (destination == roomPos)
                {
                    destinationRooms.Add(adjacentRooms[i]);
                    adjacentRoomsFound = true;
                    destinationFound = true;
                }
                else
                {
                    //if there's only one room we don't need to worry about finding the closest one.
                    if (adjacentRooms.Count == 1)
                    {
                        destinationRooms.Add(adjacentRooms[i]);
                        currentRow = adjacentRooms[i].row;
                        currentCol = adjacentRooms[i].col;
                        adjacentRoomsFound = true;
                    }
                    else
                    {
                        //find the room closest to the destination
                        float distance = Vector3.Distance(destination, adjacentRooms[i].transform.position);
                        if (distance < shortestDistance)
                        {
                            shortestDistance = distance;
                            closestRoom = adjacentRooms[i];
                        }
                    }

                    i++;
                }

            }

            if (!adjacentRoomsFound && !destinationFound)
            {
                destinationRooms.Add(closestRoom);
                currentRow = closestRoom.row;
                currentCol = closestRoom.col;
            }

        }

        //display path here as a test
        /*Debug.Log("Destination path:\n");
        foreach (Room room in destinationRooms)
        {
            GameObject tile = Instantiate(moveTilePrefab);
            tile.transform.position = new Vector3(room.transform.position.x, 0.61f, room.transform.position.z);
            Debug.Log(room.transform.position + "\n");
        }*/

        //find the character's current location in the dungeon and remove reference
        int k = 0;
        bool charFound = false;
        while(!charFound && k < turnOrder.Count)
        {
            if (turnOrder[k].room == character.room)
            {
                charFound = true;
                turnOrder[k].room.character = null;
            }
            else
            {
                k++;
            }
        }

        //start moving character TODO: MUST UPDATE CODE TO INCLUDE FOG OF WAR CONDITIONS
        character.ChangeCharacterState(character.characterState = Character.CharacterState.Moving);
        int j = 0;
        float speed = 8;
        while(j < destinationRooms.Count && character.transform.position != destination)
        {
            //the room's Y position must match the character's since the character is above the room
            Vector3 nextPos = new Vector3(destinationRooms[j].transform.position.x, character.transform.position.y,
                destinationRooms[j].transform.position.z);
            
            while (character.transform.position != nextPos)
            {
                character.transform.position = Vector3.MoveTowards(character.transform.position,
                    nextPos, speed * Time.deltaTime);
                yield return null;
            }

            //update room reference to character.
            Debug.LogFormat("Previous room {0}", character.room.roomID);
            character.room.character = null;
            destinationRooms[j].character = character;
            character.room = destinationRooms[j];
            Debug.LogFormat("New room {0}", character.room.roomID);
            //character.room = destinationRooms[j];
            /*if (j == 0)
            {
                destinationRooms[j].character = character;
            }
            else
            {
                destinationRooms[j].character = character;
                destinationRooms[j - 1].character = null;
            }*/

            /******Check if CPU character is in attack range*****/
            if (cpuAttacking && character.chosenSkill != null)
            {
                //scan area using the chosen skill's attack range. If the target is in range, stop
                //character where they are.
                //List<Room> attackRange = ShowSkillRange(character, character.chosenSkill.minRange, character.chosenSkill.maxRange);
                bool targetFound = false;
                int i = 0;
                while (!targetFound && i < attackRange.Count)
                {
                    if (attackRange[i].roomID == character.room.roomID)
                    {
                        targetFound = true;
                        j = destinationRooms.Count; //want to end this loop so we don't move any further.
                        Debug.LogFormat("{0} found target {1} and is using skill {2}", character.characterName,
                            character.targetChar.characterName, character.chosenSkill.skillName);
                    }
                    else
                    {
                        i++;
                    }
                }

            }

            j++;
        }

        //change animation to idle
        character.ChangeCharacterState(character.characterState = Character.CharacterState.Idle);

        //hide select tile and dice UI
        selectTile.SetActive(false);
        dice.ShowSingleDieUI(false);

        //change CPU HUnter state
        if (character.cpuControlled)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            hm.ChangeCPUHunterState(hm.aiState = HunterManager.HunterAIState.Idle, character as Hunter);
        }

        //check if anything is on the space the hunter landed on
        CheckForEntities(character.room, character);

        //if character can still act, do so if necessary
        characterMoved = true;
        StartCoroutine(CheckCharacterState(character));
    }

    /// <summary>
    /// Moves isometric camera to the active character (hunter or monster).
    /// </summary>
    /// <param name="character">The character the camera will focus on.</param>
    IEnumerator MoveCameraToCharacter(Character character)
    {
        Vector3 newCamPos = new Vector3(character.transform.position.x - 4, 5, character.transform.position.z - 6);
        float speed = 16;

        while (gameCamera.transform.position != newCamPos)
        {
            gameCamera.transform.position = Vector3.MoveTowards(gameCamera.transform.position, newCamPos, speed * Time.deltaTime);
            yield return null;
        }

        //moveCameraToCharacter = false;
    }

    #endregion
}
