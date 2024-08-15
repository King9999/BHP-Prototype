using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/* A dungeon consists of procedurally generated rooms. The creation of dungeons occurs in this script. 
 Any mods that affect the dungeon's objects are handled here.
 */
public class Dungeon : MonoBehaviour
{
    public List<Room> roomPrefabs;         //master list of room prefabs.
    public List<Room> dungeonRooms;

    private Hashtable occupiedPositions = new();
    // Start is called before the first frame update
    void Start()
    {
        
        //CreateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateDungeon()
    {
        int roomCount = 150;
        GameObject roomContainer = new GameObject();
        roomContainer.name = "Dungeon Rooms";

        while (dungeonRooms.Count < roomCount)
        {
            Room room = Instantiate(roomPrefabs[0]);
            room.transform.SetParent(roomContainer.transform);

            if (dungeonRooms.Count < 1)
            {
                bool pointFound = false;
                while (!pointFound)
                {
                    int randPoint = Random.Range(0, room.nodes.Length);
                    if (room.nodes[randPoint].pos.gameObject.activeSelf)
                    {
                        pointFound = true;
                        room.nodes[randPoint].isSelected = true;
                    }
                }
                Debug.Log("Added 1 dungeon room");
            }
            else
            {
                //check the last room that was added for a connect point, then add the new room there.
                bool pointFound = false;
                int i = 0;
                int lastRoom = dungeonRooms.Count - 1;
                while (!pointFound && i < dungeonRooms[lastRoom].nodes.Length)
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
                            room.ActivateConnectPoint(room.LEFT);
                            room.nodes[room.LEFT].isConnected = true;   //this is done to prevent new rooms from conneting here
                        }
                        else if (dungeonRooms[lastRoom].nodes[i].direction == Vector3.left)
                        {
                            zDir = 0;
                            xDir = -1;
                            room.ActivateConnectPoint(room.RIGHT);
                            room.nodes[room.RIGHT].isConnected = true;
                        }
                        else if (dungeonRooms[lastRoom].nodes[i].direction == Vector3.forward)
                        {
                            xDir = 0;
                            room.ActivateConnectPoint(room.BACK);
                            room.nodes[room.BACK].isConnected = true;
                        }
                        else if (dungeonRooms[lastRoom].nodes[i].direction == Vector3.back)
                        {
                            xDir = 0;
                            zDir = -1;      //z is negative when going towards screen.
                            room.ActivateConnectPoint(room.FORWARD);
                            room.nodes[room.FORWARD].isConnected = true;
                        }
                        Vector3 nodePos = dungeonRooms[lastRoom].nodes[i].pos.transform.position;
                        
                        Vector3 roomScale = dungeonRooms[lastRoom].transform.localScale;
                        Vector3 newPos = new Vector3(nodePos.x + (xDir * roomScale.x / 2), nodePos.y, 
                            nodePos.z + (zDir * roomScale.z / 2));

                        Debug.Log("Adding room " + (dungeonRooms.Count - 1) + "at position " + newPos);

                        room.gameObject.transform.position = newPos;
                       
                        
                        //room.connectPoints[i].isSelected = true;
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

                /*while (!pointFound && i < room.nodes.Length)
                {
                    //int randPoint = Random.Range(0, room.connectPoints.Count);
                    //room.connectPoints[randPoint].isSelected = true;
                    if (room.nodes[i].pos.gameObject.activeSelf && !room.nodes[i].isConnected)
                    {
                        pointFound = true;
                        room.nodes[i].isSelected = true;
                    }
                    else
                    {
                        i++;
                    }
                }

                if (!pointFound)
                {
                    Debug.Log("error: no connect point found");
                }*/
            }

            //}

            dungeonRooms.Add(room);
            //i++;
        }
    }
}
