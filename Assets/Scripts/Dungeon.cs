using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

/* A dungeon consists of procedurally generated rooms. The creation of dungeons occurs in this script. 
 Any mods that affect the dungeon's objects are handled here.
 */
public class Dungeon : MonoBehaviour
{
    [SerializeField] private Room roomPrefab;              //master list of room prefabs.
    public List<Room> dungeonRooms, roomBin;    //roomBin is used to reuse already instantiated rooms.
    public char[,] dungeonGrid;                  //used for various things such as determining movement range
    private int totalRows { get; set; }
    private int totalCols { get; set; }

    [Header("---Treasure Chests---")]
    [SerializeField]private Entity_TreasureChest chestPrefab;
    [SerializeField]private List<Entity_TreasureChest> treasureChests;
    public float baseCreditsChance;             //determines whether a chest contains credits or an item
    public float creditsChanceMod;               //modified by dungeon mods.

    [Header("---Exit Point---")]
    [SerializeField]private Entity_Exit exitPointPrefab;
    public Room exitRoom;                       //reference to exit point, used by other scripts

    [Header("---Spawn Point---")]
    [SerializeField]private Entity_Spawner spawnerPrefab;
    public List<Entity_Spawner> spawnPoints;

    [Header("---Terminals----")]
    [SerializeField] private Entity_Terminal terminalPrefab;
    public List<Entity_Terminal> terminals;


    //private readonly Hashtable occupiedPositions = new();
    private readonly Dictionary<Vector3, Room> occupiedPositions = new Dictionary<Vector3, Room>();
    // Start is called before the first frame update
    void Start()
    {
        baseCreditsChance = 0.4f;
        //CreateDungeon();
    }

   
    /// <summary>
    /// Adds an adjacent room to the room specified.
    /// </summary>
    /// <param name="room">The room that will have an adjacent room generated.</param>
    Vector3 GenerateRoomPosition(Room room)
    {
        int randPoint = Random.Range(0, room.nodes.Length);
        float xDir = 1;
        float zDir = 1;
        if (room.nodes[randPoint].direction == Vector3.right)
        {
            zDir = 0;
        }
        else if (room.nodes[randPoint].direction == Vector3.left)
        {
            zDir = 0;
            xDir = -1;
        }
        else if (room.nodes[randPoint].direction == Vector3.forward)
        {
            xDir = 0;
        }
        else if (room.nodes[randPoint].direction == Vector3.back)
        {
            xDir = 0;
            zDir = -1;      //z is negative when going towards screen.
        }
        Vector3 nodePos = room.nodes[randPoint].pos.transform.position;
        Vector3 roomScale = room.transform.localScale;
        Vector3 newPos = new Vector3(nodePos.x + (xDir * roomScale.x / 2), nodePos.y,
                nodePos.z + (zDir * roomScale.z / 2));

        return newPos;  /*new Vector3(nodePos.x + (xDir * roomScale.x / 2), nodePos.y,
                nodePos.z + (zDir * roomScale.z / 2));*/
    }

    public Room GetRoom(int row, int col)
    {
        bool roomFound = false;
        int i = 0;
        Room room = null;
        while (!roomFound && i < dungeonRooms.Count)
        {
            room = dungeonRooms[i];
            if (room.row == row && room.col == col)
            {
                roomFound = true;
            }
            else
            {
                i++;
            }
        }

        return room;
    }

    /// <summary>
    /// Places character into a given room. Will also update their world position.
    /// </summary>
    /// <param name="character">The character to be moved.</param>
    /// <param name="room">The room to place the character.</param>
    public void UpdateCharacterRoom(Character character, Room room)
    {
        Vector3 roomPos = room.transform.position;
        if (character.room != null)
            character.room.character = null;
        room.character = character;
        character.room = room;
        character.transform.position = new Vector3(roomPos.x, character.transform.position.y, roomPos.z);
    }

    public void UpdateEntityRoom(Entity entity, Room room)
    {
        Vector3 roomPos = room.transform.position;
        if (entity.room != null)
            entity.room.entity = null;
        room.entity = entity;
        entity.room = room;
        entity.transform.position = new Vector3(roomPos.x, entity.transform.position.y, roomPos.z);
    }

    public void CreateDungeon()
    {
        //TODO: Room count scales up depending on hunter count. Need to figure out a good minimum room count for 2 hunters.
        //TODO 2: Need a way to recycle rooms, and only instantiate more rooms when necessary.

        HunterManager hm = Singleton.instance.HunterManager;
        bool roomFound;     //this will be used many times

        //get average hunter level of all player-controlled hunters to add CPU Hunters. Average hunter level
        //is used later for other calculations.
        int averageHunterLevel = 0;                         
        foreach (Hunter hunter in hm.hunters)
        {
            averageHunterLevel += hunter.hunterLevel;
        }

        //get average
        averageHunterLevel /= hm.hunters.Count;

        //should never happen, but just in case
        if (averageHunterLevel <= 0)
            averageHunterLevel = 1;

        //**************Add CPU Hunters*************/
        for (int i = 0; i < hm.rivalCount; i++)
        {
            hm.hunters.Add(hm.CreateCPUHunter(averageHunterLevel));
        }

        //get the average hunter level again for calculations of adding other objects
        averageHunterLevel = 0;
        foreach (Hunter hunter in hm.hunters)
        {
            averageHunterLevel += hunter.hunterLevel;
        }
        averageHunterLevel /= hm.hunters.Count;

        /*****************DUNGEON GENERATION********************/
        int roomCount = 50 * hm.hunters.Count;
        GameObject roomContainer = new GameObject("Dungeon Rooms");
        //roomContainer.name = "Dungeon Rooms";
        roomContainer.transform.SetParent(this.transform);

        while (dungeonRooms.Count < roomCount)
        {
            Room room = Instantiate(roomPrefab);
            room.transform.SetParent(roomContainer.transform);
            room.roomID = dungeonRooms.Count;

            if (dungeonRooms.Count < 1)
            {
                dungeonRooms.Add(room);
                occupiedPositions.Add(room.transform.position, room);
                //Debug.Log("Added 1 dungeon room");
            }
            else
            {
                //find a random point and add new room there. Must check for occupiped positions.
                //check the last room that was added for a connect point, then add the new room there.
                bool pointFound = false;
                
                //check for occupied positions
                while (!pointFound)
                {
                    int randRoom = Random.Range(0, dungeonRooms.Count);
                    Vector3 newPos = GenerateRoomPosition(dungeonRooms[randRoom]);

                    if (!occupiedPositions.ContainsKey(newPos))
                    {
                        pointFound = true;
                        room.transform.position = newPos;
                        dungeonRooms.Add(room);
                        occupiedPositions.Add(room.transform.position, room); //, room.roomID);
                    }
                }
            }

        }

        /* Create a grid that will be used for various things. We search for the highest and lowest Z values for
         * the rows, then the highest and lowest X values for the columns. */
        float highestZ = 0;
        float lowestZ = 0;
        float highestX = 0;
        float lowestX = 0;
        for (int i = 0; i < dungeonRooms.Count; i++)
        {
            Vector3 currentPos = dungeonRooms[i].transform.position;
            if (currentPos.z > highestZ)
                highestZ = currentPos.z;
            if (currentPos.z < lowestZ)
                lowestZ = currentPos.z;
            if (currentPos.x > highestX)
                highestX = currentPos.x;
            if (currentPos.x < lowestX)
                lowestX = currentPos.x;
        }

        //the number of rows and columns is determined by adding the highest and lowest, then dividing by 2.
        //negative sign is ignored.
        Debug.LogFormat("Highest X: {0}\nLowestX: {1}\nHighest Z: {2}\nLowest Z: {3}", highestX, lowestX, highestZ, lowestZ);

        totalRows = 1 + ((int)(Mathf.Abs(highestZ) + Mathf.Abs(lowestZ)) / 2); //1 is added to get an accurate grid
        totalCols = 1 + ((int)(Mathf.Abs(highestX) + Mathf.Abs(lowestX)) / 2);
        dungeonGrid = new char[totalRows, totalCols];
        Debug.LogFormat("Total Rows: {0} total cols: {1}", totalRows, totalCols);

        //populate the grid with the dungeon rooms. We start from row 0, col 0, which in world space would be 
        //the lowest X position and the highest Z position.
        float currentZ, currentX;  //used to find the rooms in world space.
        string gridStr = "";
        for (int i = 0; i < totalRows; i++)
        {
            currentZ = highestZ - (i * 2);    //this is the Z coordinate in world space.
            for (int j = 0; j < totalCols; j++)
            {
                currentX = lowestX + (j * 2);
                if (occupiedPositions.ContainsKey(new Vector3(currentX, 0, currentZ)))
                {
                    //found a room
                    dungeonGrid[i, j] = '1';
                    Room room = occupiedPositions[new Vector3(currentX, 0, currentZ)];  //returns the room located at this position
                    room.row = i;
                    room.col = j;
                }
                else
                {
                    dungeonGrid[i, j] = '0';
                }
                gridStr += string.Format("{0}, ", dungeonGrid[i, j]);
            }
            gridStr += "\n";
        }

        Debug.LogFormat("Dungeon Grid\n--------\n{0}", gridStr);

        /* populate the dungeon with objects, including hunters. */
        //add all hunters
        List<int> occupiedLocations = new List<int>();  //dungeon rooms that have an object in them.
        foreach(Hunter hunter in hm.hunters)
        {
            //pick a random room and place hunter there. Hunters should be placed in a way so that they aren't too close 
            //to each other.
            roomFound = false;
            while (!roomFound)
            {
                int randRoom = Random.Range(0, dungeonRooms.Count);

                if (!occupiedLocations.Contains(randRoom))
                {
                    //check if this object collides with nearby objects. If true, find another location.
                    Vector3 roomPos = dungeonRooms[randRoom].transform.position;
                    Collider[] colliders = Physics.OverlapSphere(roomPos, 2, 0, QueryTriggerInteraction.Collide);
                    if (colliders.Length <= 0)
                    {
                        roomFound = true;
                        hm.ToggleHunter(hunter, true);
                        //Vector3 roomPos = dungeonRooms[randRoom].transform.position;
                        //2 is added to Y so hunter is above the room and not falling through it
                        hunter.transform.position = new Vector3(roomPos.x, roomPos.y + 2, roomPos.z);
                        UpdateCharacterRoom(hunter, dungeonRooms[randRoom]);
                        //hunter.room = dungeonRooms[randRoom];
                        //dungeonRooms[randRoom].character = hunter;
                        occupiedLocations.Add(randRoom);
                    }
                    else
                    {
                        Debug.Log("Hunter too close to other objects, finding another location");
                        occupiedLocations.Add(randRoom);
                    }
                }
            }
        }

        //hm.hunters[1].transform.position = new Vector3(hm.hunters[0].transform.position.x + 4, hm.hunters[0].transform.position.y, hm.hunters[0].transform.position.z);
        //UpdateCharacterRoom(hm.hunters[1], dungeonRooms[dungeonRooms.IndexOf(hm.hunters[0].room) + 1]);

        //add chests. Number of chests = hunter count + random number between 1 and 3.
        int chestCount = hm.hunters.Count + Random.Range(1, 4);
        ItemManager im = Singleton.instance.ItemManager;
        GameObject chestContainer = new GameObject("Treasure Chests");
        //chestContainer.name = "Treasure Chests";
        chestContainer.transform.SetParent(this.transform);

        for (int i = 0; i < chestCount; i++)
        {
            roomFound = false;
            while (!roomFound)
            {
                int randRoom = Random.Range(0, dungeonRooms.Count);

                if (!occupiedLocations.Contains(randRoom))
                {
                    //check if this object collides with nearby objects. If true, find another location.
                    Vector3 roomPos = dungeonRooms[randRoom].transform.position;
                    Collider[] colliders = Physics.OverlapSphere(roomPos, 2, 0, QueryTriggerInteraction.Collide);
                    if (colliders.Length <= 0)
                    {

                        roomFound = true;
                        Entity_TreasureChest chest = Instantiate(chestPrefab, chestContainer.transform);
                        //chest.transform.SetParent(chestContainer.transform);

                        //TODO: If this is the first chest to be generated, it must contain the target item.


                        //1 is added to Y so chest is above the room
                        chest.transform.position = new Vector3(roomPos.x, roomPos.y + 1.2f, roomPos.z);
                        occupiedLocations.Add(randRoom);

                        //generate item TODO: first chest must contain target item.
                        if (i == 0)
                        {
                            //add target item
                            im.GenerateChestItem(chest, Table.ItemType.Valuable, targetItem: true);
                            Debug.LogFormat("Target item {0} generated", chest.item.itemName);
                        }
                        else
                        {
                            Debug.LogFormat("Chance to generate credits: {0}", baseCreditsChance + creditsChanceMod);
                            if (Random.value <= baseCreditsChance + creditsChanceMod)
                            {
                                chest.credits = 50 * averageHunterLevel;
                                chest.credits += Random.Range(0, (chest.credits / 2) + 1);
                                Debug.LogFormat("Adding {0} CR to chest", chest.credits);
                            }
                            else
                            {
                                im.GenerateChestItem(chest);
                            }
                        }
                        UpdateEntityRoom(chest, dungeonRooms[randRoom]);
                        //dungeonRooms[randRoom].entity = chest;
                        treasureChests.Add(chest);
                    }
                    else
                    {
                        Debug.Log("Chest too close to other objects, finding another location");
                        occupiedLocations.Add(randRoom);
                    }
                }
            }
        }

        /*******Exit Point*********/
        GameObject exitContainer = new GameObject("Exit Point");
        //exitContainer.name = "Exit Point";
        exitContainer.transform.SetParent(this.transform);

        roomFound = false;
        while (!roomFound)
        {
            int randRoom = Random.Range(0, dungeonRooms.Count);
            if (!occupiedLocations.Contains(randRoom))
            {
                //check if this object collides with nearby objects. If true, find another location.
                Vector3 roomPos = dungeonRooms[randRoom].transform.position;
                Collider[] colliders = Physics.OverlapSphere(roomPos, 2, 0, QueryTriggerInteraction.Collide);
                if (colliders.Length <= 0)
                {
                    roomFound = true;
                    Entity_Exit exitPoint = Instantiate(exitPointPrefab);
                    exitPoint.transform.SetParent(exitContainer.transform);
                    exitPoint.transform.position = new Vector3(roomPos.x, roomPos.y + 2f, roomPos.z);
                    UpdateEntityRoom(exitPoint, dungeonRooms[randRoom]);
                    //dungeonRooms[randRoom].entity = exitPoint;
                    exitRoom = dungeonRooms[randRoom];
                    occupiedLocations.Add(randRoom);
                }
                else
                {
                    Debug.Log("Exit Point too close to other objects, finding another location");
                    occupiedLocations.Add(randRoom);
                }
            }
        }

        /********Spawn Points**************/
        GameObject spawnPointContainer = new GameObject("Spawn Point");
        spawnPointContainer.transform.SetParent(transform);

        //# of spawn points = # of hunters + random number between 1 and 2
        int spawnerCount = hm.hunters.Count + Random.Range(1, 3);
        for (int i = 0; i < spawnerCount; i++)
        {
            roomFound = false;
            while (!roomFound)
            {
                int randRoom = Random.Range(0, dungeonRooms.Count);

                if (!occupiedLocations.Contains(randRoom))
                {
                    //check if this object collides with nearby objects. If true, find another location.
                    Vector3 roomPos = dungeonRooms[randRoom].transform.position;
                    Collider[] colliders = Physics.OverlapSphere(roomPos, 2, 0, QueryTriggerInteraction.Collide);
                    if (colliders.Length <= 0)
                    {

                        roomFound = true;
                        Entity_Spawner spawner = Instantiate(spawnerPrefab, spawnPointContainer.transform);

                        //0.61 is added to Y so spawner is above the room. It should lie above the move tiles.
                        spawner.transform.position = new Vector3(roomPos.x, roomPos.y + 0.61f, roomPos.z);
                        occupiedLocations.Add(randRoom);

                        UpdateEntityRoom(spawner, dungeonRooms[randRoom]);
                        //dungeonRooms[randRoom].entity = spawner;
                        spawnPoints.Add(spawner);
                    }
                    else
                    {
                        Debug.Log("Spawner too close to other objects, finding another location");
                        occupiedLocations.Add(randRoom);
                    }
                }
            }
        }

        /***************Terminals********************/
        GameObject terminalContainer = new GameObject("Terminals");
        terminalContainer.transform.SetParent(transform);

        //# of terminals = random number between 0 and (Hunter count / 2)
        int terminalCount = Random.Range(0, (hm.hunters.Count / 2) + 1);
        for (int i = 0; i < terminalCount; i++)
        {
            roomFound = false;
            while (!roomFound)
            {
                int randRoom = Random.Range(0, dungeonRooms.Count);

                if (!occupiedLocations.Contains(randRoom))
                {
                    //check if this object collides with nearby objects. If true, find another location.
                    Vector3 roomPos = dungeonRooms[randRoom].transform.position;
                    Collider[] colliders = Physics.OverlapSphere(roomPos, 2, 0, QueryTriggerInteraction.Collide);
                    if (colliders.Length <= 0)
                    {

                        roomFound = true;
                        Entity_Terminal terminal = Instantiate(terminalPrefab, terminalContainer.transform);

                        //1.45 is added to Y so terminal is above the room. It should lie above the move tiles.
                        terminal.transform.position = new Vector3(roomPos.x, roomPos.y + 1.45f, roomPos.z);
                        occupiedLocations.Add(randRoom);

                        UpdateEntityRoom(terminal, dungeonRooms[randRoom]);

                        //get random terminal effect
                        EffectManager em = Singleton.instance.EffectManager;
                        int randEffect = Random.Range(0, em.terminalEffects.Count);
                        terminal.terminalEffect = Instantiate(em.terminalEffects[randEffect]);
                        terminals.Add(terminal);
                    }
                    else
                    {
                        Debug.Log("Terminal too close to other objects, finding another location");
                        occupiedLocations.Add(randRoom);
                    }
                }
            }
        }
    }
}
