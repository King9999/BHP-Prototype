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

    //private readonly Hashtable occupiedPositions = new();
    private Dictionary<Vector3, Room> occupiedPositions = new Dictionary<Vector3, Room>();
    // Start is called before the first frame update
    void Start()
    {
        
        //CreateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void CreateDungeon()
    {
        //TODO: Room count scales up depending on hunter count. Need to figure out a good minimum room count for 2 hunters.
        //TODO 2: Need a way to recycle rooms, and only instantiate more rooms when necessary.

        HunterManager hm = Singleton.instance.HunterManager;
        int roomCount = 50 * hm.hunters.Count;
        //bool loopBreak = false;
        GameObject roomContainer = new GameObject();
        roomContainer.name = "Dungeon Rooms";

        while (/*!loopBreak &&*/ dungeonRooms.Count < roomCount)
        {
            Room room = Instantiate(roomPrefabs[0]);
            room.transform.SetParent(roomContainer.transform);
            room.roomID = dungeonRooms.Count;

            if (dungeonRooms.Count < 1)
            {
                dungeonRooms.Add(room);
                occupiedPositions.Add(room.transform.position, room);
                Debug.Log("Added 1 dungeon room");
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
        List<int> occupiedLocations = new List<int>();  //dungeon rooms that have an object in them.

        foreach(Hunter hunter in hm.hunters)
        {
            //pick a random room and place hunter there. Hunters should be placed in a way so that they aren't too close 
            //to each other.
            bool roomFound = false;
            while (!roomFound)
            {
                int randRoom = Random.Range(0, dungeonRooms.Count);

                if (!occupiedLocations.Contains(randRoom))
                {
                    roomFound = true;
                    hm.ToggleHunter(hunter, true);
                    Vector3 roomPos = dungeonRooms[randRoom].transform.position;
                    //2 is added to Y so hunter is above the room and not falling through it
                    hunter.transform.position = new Vector3(roomPos.x, roomPos.y + 2, roomPos.z);
                    hunter.room = dungeonRooms[randRoom];
                    occupiedLocations.Add(randRoom);
                }
            }
            
        }

        //add chests. Number of chests = hunter count * 2 + random number between 0 and 3.
        chestCount = hm.hunters.Count * 2 + Random.Range(0, 4);
        ItemManager im = Singleton.instance.ItemManager;
        GameObject chestContainer = new GameObject();
        chestContainer.name = "Treasure Chests";

        for (int i = 0; i < chestCount; i++)
        {
            bool roomFound = false;
            while (!roomFound)
            {
                int randRoom = Random.Range(0, dungeonRooms.Count);

                if (!occupiedLocations.Contains(randRoom))
                {
                    roomFound = true;
                    Entity_TreasureChest chest = Instantiate(chestPrefab, chestContainer.transform);
                    //chest.transform.SetParent(chestContainer.transform);

                    //TODO: If this is the first chest to be generated, it must contain the target item.

                    Vector3 roomPos = dungeonRooms[randRoom].transform.position;
                    //1 is added to Y so chest is above the room
                    chest.transform.position = new Vector3(roomPos.x, roomPos.y + 1.2f, roomPos.z);
                    occupiedLocations.Add(randRoom);

                    //generate item TODO: first chest must contain target item.
                    /*if (i == 0)
                    {
                        //add target item
                    }*/
                    im.GenerateChestItem(chest);
                    //chest.room = dungeonRooms[randRoom];
                    dungeonRooms[randRoom].entity = chest;
                    treasureChests.Add(chest);
                }
            }
        }
    }
}
