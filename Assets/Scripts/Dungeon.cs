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
    // Start is called before the first frame update
    void Start()
    {
        
        CreateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateDungeon()
    {
        int roomCount = 2;

        while (dungeonRooms.Count < roomCount)
        {
            Room room = Instantiate(roomPrefabs[0]);

            //room.connectPoints[0].gameObject.SetActive(true);
            //if this room has no active connect points, activate a random one
            /*if (dungeonRooms.Count <= 1 && room.DeadEnd())
            {
                int randPoint = Random.Range(0, room.connectPoints.Count);
                room.ActivateConnectPoint(randPoint);
                room.connectPoints[randPoint].isSelected = true;
            }
            else
            {*/
            //we don't want dead ends early on.
            if (dungeonRooms.Count < 1 /*&& room.DeadEnd()*/)
            {
                int randPoint = Random.Range(0, room.connectPoints.Count);
                room.ActivateConnectPoint(randPoint);
                room.connectPoints[randPoint].isSelected = true;
            }
            else
            {
                //check the last room that was added for a connect point, then add the new room there.
                bool pointFound = false;
                int i = 0;
                int lastRoom = dungeonRooms.Count - 1;
                while (!pointFound && i < dungeonRooms[lastRoom].connectPoints.Count)
                {
                    if (dungeonRooms[lastRoom].connectPoints[i].isSelected)
                    {
                        //connect room to this point.
                        pointFound = true;
                        room.gameObject.transform.position = dungeonRooms[lastRoom].connectPoints[i].point.position;
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

                while (!pointFound && i < room.connectPoints.Count)
                {
                    //int randPoint = Random.Range(0, room.connectPoints.Count);
                    //room.connectPoints[randPoint].isSelected = true;
                    if (room.connectPoints[i].point.gameObject.activeSelf)
                    {
                        pointFound = true;
                        room.connectPoints[i].isSelected = true;
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            //}

            dungeonRooms.Add(room);
            //i++;
        }
    }
}
