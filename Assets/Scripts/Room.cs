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
        //public Vector3 direction;       //the direction the node is facing. required for connecting other rooms.
    }
    public Node[] nodes;
    // Start is called before the first frame update
    void Start()
    {
        ActivateConnectPoints();
    }


    //when a room is instantiated, this method must be run
    private void ActivateConnectPoints()
    {
        List<bool> activePoints = new List<bool>();
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
        }
        
    }

    public void ActivateConnectPoint(int point)
    {
        nodes[point].pos.gameObject.SetActive(true);
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
