using System.Collections;
using System.Collections.Generic;
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
        Room room = Instantiate(roomPrefabs[0]);

        room.connectPoints[0].gameObject.SetActive(true);

        dungeonRooms.Add(room);
    }
}
