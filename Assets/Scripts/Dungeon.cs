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

    [Header("---Treasure Chests---")]
    public Entity_TreasureChest chestPrefab;
    public List<Entity_TreasureChest> treasureChests;
    public int chestCount;

    private readonly Hashtable occupiedPositions = new();
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

        return new Vector3(nodePos.x + (xDir * roomScale.x / 2), nodePos.y,
                nodePos.z + (zDir * roomScale.z / 2));
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
                //bool pointFound = false;
                //while (!pointFound)
                //{
                //int randPoint = Random.Range(0, room.nodes.Length);
                //if (room.nodes[randPoint].pos.gameObject.activeSelf)
                //{
                //pointFound = true;
                //room.nodes[randPoint].isSelected = true;
                dungeonRooms.Add(room);
                occupiedPositions.Add(room.transform.position, room.roomID);
                //}
                //}
                Debug.Log("Added 1 dungeon room");
            }
            else
            {
                //find a random point and add new room there. Must check for occupiped positions.


                //check the last room that was added for a connect point, then add the new room there.
                bool pointFound = false;
                //int i = 0;
                //int lastRoom = dungeonRooms.Count - 1;
                

                //check for occupied positions
                //int loopCount = 0;
                while (/*loopCount < 100 &&*/ !pointFound)
                {
                    /*int randPoint = Random.Range(0, dungeonRooms[lastRoom].nodes.Length);
                    float xDir = 1;
                    float zDir = 1;
                    if (dungeonRooms[lastRoom].nodes[randPoint].direction == Vector3.right)
                    {
                        zDir = 0;
                    }
                    else if (dungeonRooms[lastRoom].nodes[randPoint].direction == Vector3.left)
                    {
                        zDir = 0;
                        xDir = -1;
                    }
                    else if (dungeonRooms[lastRoom].nodes[randPoint].direction == Vector3.forward)
                    {
                        xDir = 0;
                    }
                    else if (dungeonRooms[lastRoom].nodes[randPoint].direction == Vector3.back)
                    {
                        xDir = 0;
                        zDir = -1;      //z is negative when going towards screen.
                    }
                    Vector3 nodePos = dungeonRooms[lastRoom].nodes[randPoint].pos.transform.position;
                    Vector3 roomScale = dungeonRooms[lastRoom].transform.localScale;
                    Vector3 newPos = new Vector3(nodePos.x + (xDir * roomScale.x / 2), nodePos.y,
                            nodePos.z + (zDir * roomScale.z / 2));*/
                    int randRoom = Random.Range(0, dungeonRooms.Count);
                    Vector3 newPos = GenerateRoomPosition(dungeonRooms[randRoom]);

                    if (!occupiedPositions.ContainsKey(newPos))
                    {
                        pointFound = true;
                        room.transform.position = newPos;
                        dungeonRooms.Add(room);
                        occupiedPositions.Add(room.transform.position, room.roomID);
                        //loopCount = 0;
                    }
                    /*else
                    {
                        loopCount++;
                    }*/
                }

                /*if (loopCount >= 100)
                {
                    //if we get here, that means we ran into a situation where a room couldn't find available
                    //space. Now we much search elsewhere to add new room.
                    Debug.Log("Hit an infinite loop");
                    loopCount = 0;
                    //search for a random room that can accomodate a new room.
                    while (loopCount < 100 && !pointFound)
                    {
                        int randRoom = Random.Range(0, dungeonRooms.Count);
                        Vector3 newPos = GenerateRoomPosition(dungeonRooms[randRoom]);
                        if (!occupiedPositions.ContainsKey(newPos))
                        {
                            pointFound = true;
                            room.transform.position = newPos;
                            dungeonRooms.Add(room);
                            occupiedPositions.Add(room.transform.position, room.roomID);
                            loopCount = 0;
                        }
                        else
                        {
                            loopCount++;
                        }
                    }
                    if (loopCount >= 100)
                    {
                        loopBreak = true;   //if we get here, then something is seriously wrong
                        Debug.Log("Main loop broken");
                    }
                    //loopBreak = true;
                }*/
            }

                /*while (!pointFound && i < dungeonRooms[lastRoom].nodes.Length)
                {
                    if (dungeonRooms[lastRoom].nodes[i].isSelected && !dungeonRooms[lastRoom].nodes[i].isConnected)
                    {
                        //connect room to this point.
                        pointFound = true;
                        dungeonRooms[lastRoom].nodes[i].isConnected = true;

                        //get the direction of this node
                        float xDir = 1; 
                        float zDir = 1;
                        if (dungeonRooms[lastRoom].nodes[i].direction == Vector3.right) 
                        {
                            zDir = 0;
                            room.ActivateConnectPoint(room.LEFT, true);
                            room.nodes[room.LEFT].isConnected = true;   //this is done to prevent new rooms from conneting here
                        }
                        else if (dungeonRooms[lastRoom].nodes[i].direction == Vector3.left)
                        {
                            zDir = 0;
                            xDir = -1;
                            room.ActivateConnectPoint(room.RIGHT, true);
                            room.nodes[room.RIGHT].isConnected = true;
                        }
                        else if (dungeonRooms[lastRoom].nodes[i].direction == Vector3.forward)
                        {
                            xDir = 0;
                            room.ActivateConnectPoint(room.BACK, true);
                            room.nodes[room.BACK].isConnected = true;
                        }
                        else if (dungeonRooms[lastRoom].nodes[i].direction == Vector3.back)
                        {
                            xDir = 0;
                            zDir = -1;      //z is negative when going towards screen.
                            room.ActivateConnectPoint(room.FORWARD, true);
                            room.nodes[room.FORWARD].isConnected = true;
                        }
                        Vector3 nodePos = dungeonRooms[lastRoom].nodes[i].pos.transform.position;
                        
                        Vector3 roomScale = dungeonRooms[lastRoom].transform.localScale;
                        Vector3 newPos = new Vector3(nodePos.x + (xDir * roomScale.x / 2), nodePos.y, 
                            nodePos.z + (zDir * roomScale.z / 2));

                        Debug.Log("Adding room " + (dungeonRooms.Count - 1) + " at position " + newPos);

                        room.gameObject.transform.position = newPos;
                    }
                    else
                    {
                        i++;
                    }
                }

                //select the next connect point
                pointFound = false;
                i = 0;

                while(!pointFound)
                {
                    int randPoint = Random.Range(0, room.nodes.Length);
                    if (room.nodes[randPoint].pos.gameObject.activeSelf && !room.nodes[randPoint].isConnected)
                    {
                        pointFound = true;
                        room.nodes[randPoint].isSelected = true;
                    }
                }

            }*/
            //dungeonRooms.Add(room);
        }

        /* populate the dungeon with objects, including hunters. */
        List<int> occupiedLocations = new List<int>();  //dungeon rooms that have an object in them.

        //HunterManager hm = Singleton.instance.HunterManager;

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

        //add chests
        chestCount = hm.hunters.Count * 2;
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
                    Entity_TreasureChest chest = Instantiate(chestPrefab);
                    chest.transform.SetParent(chestContainer.transform);

                    //TODO: If this is the first chest to be generated, it must contain the target item.

                    Vector3 roomPos = dungeonRooms[randRoom].transform.position;
                    //1 is added to Y so chest is above the room
                    chest.transform.position = new Vector3(roomPos.x, roomPos.y + 1, roomPos.z);
                    occupiedLocations.Add(randRoom);

                    //generate item
                    im.GenerateChestItem(chest);
                    treasureChests.Add(chest);
                }
            }
        }
    }
}
