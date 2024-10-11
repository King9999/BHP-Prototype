using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

/* A dungeon consists of procedurally generated rooms. The creation of dungeons occurs in this script. 
 Any mods that affect the dungeon's objects are handled here.
 */
public class Dungeon : MonoBehaviour
{
    public List<Room> roomPrefabs;              //master list of room prefabs.
    public List<Room> dungeonRooms, roomBin;    //roomBin is used to reuse already instantiated rooms.
    public char[,] dungeonGrid;                  //used for various things such as determining movement range
    public int totalRows { get; set; }
    public int totalCols { get; set; }

    [Header("---Treasure Chests---")]
    public Entity_TreasureChest chestPrefab;
    public List<Entity_TreasureChest> treasureChests;
    public int chestCount;
    public float baseCreditsChance;             //determines whether a chest contains credits or an item
    public float creditsChanceMod;               //modified by dungeon mods.

    [Header("---Exit Point---")]
    public Entity_Exit exitPointPrefab;

    //private readonly Hashtable occupiedPositions = new();
    private Dictionary<Vector3, Room> occupiedPositions = new Dictionary<Vector3, Room>();
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

    //also updates character world position in the dungeon
    public void UpdateCharacterRoom(Character character, Room room)
    {
        Vector3 roomPos = room.transform.position;
        character.room.character = null;
        room.character = character;
        character.room = room;
        character.transform.position = new Vector3(roomPos.x, character.transform.position.y, roomPos.z);
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
        GameObject roomContainer = new GameObject();
        roomContainer.name = "Dungeon Rooms";
        roomContainer.transform.SetParent(this.transform);

        while (dungeonRooms.Count < roomCount)
        {
            Room room = Instantiate(roomPrefabs[0]);
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
        Debug.Log("Highest X: " + highestX + "\nLowestX: " + lowestX + "\nHighest Z: " + highestZ + "\nLowest Z: "
            + lowestZ);

        totalRows = 1 + ((int)(Mathf.Abs(highestZ) + Mathf.Abs(lowestZ)) / 2); //1 is added to get an accurate grid
        totalCols = 1 + ((int)(Mathf.Abs(highestX) + Mathf.Abs(lowestX)) / 2);
        dungeonGrid = new char[totalRows, totalCols];
        Debug.Log("Total Rows: " + totalRows + " total cols: " + totalCols);

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
                gridStr += dungeonGrid[i, j] + ", ";
            }
            gridStr += "\n";
        }

        Debug.Log("Dungeon Grid\n--------\n" + gridStr);

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
                        hunter.room = dungeonRooms[randRoom];
                        dungeonRooms[randRoom].character = hunter;
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
        chestCount = hm.hunters.Count + Random.Range(1, 4);
        ItemManager im = Singleton.instance.ItemManager;
        GameObject chestContainer = new GameObject();
        chestContainer.name = "Treasure Chests";
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
                            Debug.Log("Target item " + chest.item.itemName + " generated");
                        }
                        else
                        {
                            Debug.Log("Chance to generate credits: " + (baseCreditsChance + creditsChanceMod));
                            if (Random.value <= baseCreditsChance + creditsChanceMod)
                            {
                                chest.credits = 50 * averageHunterLevel;
                                chest.credits += Random.Range(0, (chest.credits / 2) + 1);
                                Debug.Log("Adding " + chest.credits + " CR to chest");
                            }
                            else
                            {
                                im.GenerateChestItem(chest);
                            }
                        }
                        dungeonRooms[randRoom].entity = chest;
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
        GameObject exitContainer = new GameObject();
        exitContainer.name = "Exit Point";
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
                    dungeonRooms[randRoom].entity = exitPoint;
                    occupiedLocations.Add(randRoom);
                }
                else
                {
                    Debug.Log("Exit Point too close to other objects, finding another location");
                    occupiedLocations.Add(randRoom);
                }
            }
        }

    }
}
