using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Rooms are the building blocks for creating dungeons. Each room can have connect points, which are used to attach rooms together.
 * A room can have multiple points, but which ones are enabled is random. */
public class Room : MonoBehaviour
{
    [System.Serializable]
    public class Node
    {
        public Transform pos;
        public bool isConnected;        //if true, a room is attached to this point.
        public bool isSelected;         //if true, a room is going to be connected to this point.
        public Vector3 direction;       //the direction the node is facing. required for connecting other rooms.
    }
    public Node[] nodes;
    public int roomID;                  //easy way to identify a room
    public int row, col;                //room's position in 2D array
    public Entity entity;               //can be a chest, spawn point, exit, or terminal.
    public Character character;         //reference to a Hunter or monster.

    //constants for node direction
    public int FORWARD { get; } = 0;        //this is "up"
    public int LEFT { get; } = 1;
    public int RIGHT { get; } = 2;
    public int BACK { get; } = 3;      //this is "down"

    void Awake()
    {
        //ActivateConnectPoints();
    }


    //when a room is instantiated, this method must be run
    private void ActivateConnectPoints()
    {
        /*List<bool> activePoints = new List<bool>();
        foreach (Node t in nodes)
        {
            if (Random.value <= 0.35f)
                activePoints.Add(true);
            else
                activePoints.Add(false);
        }

        for (int i = 0; i < activePoints.Count; i++)
        {
            if (activePoints[i] == true)
            {
                nodes[i].pos.gameObject.SetActive(true);
            }
        }*/

        //there must be at least 2 active points. The remaining points are activated by chance
        int activeNodeCount = 0;
        List<int> usedValues = new List<int>();

        while (activeNodeCount < 2)
        { 
            int randPoint = Random.Range(0, nodes.Length);
            if (!usedValues.Contains(randPoint))
            {
                nodes[randPoint].pos.gameObject.SetActive(true);
                usedValues.Add(randPoint);
                activeNodeCount++;
            }
        }

        //Debug.Log("got here");

        //check for any more points
        for (int i = 0; i < nodes.Length; i++)
        {
            if (!nodes[i].pos.gameObject.activeSelf)
            {
                if (Random.value <= 0.5f)
                    nodes[i].pos.gameObject.SetActive(true);
            }
        }
        
    }

    public void ActivateConnectPoint(int point, bool toggle)
    {
        nodes[point].pos.gameObject.SetActive(toggle);
    }

    public bool ConnectPointActive(int point)
    {
        return nodes[point].pos.gameObject.activeSelf;
    }

    //checks for rooms with no connect points. The first half number of rooms generated must have at least one connect point.
    public bool DeadEnd()
    {
        int inactivePointsCount = 0;
        for(int i = 0; i < nodes.Length; i++)
        {
            if (!nodes[i].pos.gameObject.activeSelf)
            {
                inactivePointsCount++;
            }
           
        }

        if (inactivePointsCount >= nodes.Length)
            return true;
        else
            return false;
    }
}
