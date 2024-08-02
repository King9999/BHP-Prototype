using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Rooms are the building blocks for creating dungeons. Each room can have connect points, which are used to attach rooms together.
 * A room can have multiple points, but which ones are enabled is random. */
public class Room : MonoBehaviour
{
    [System.Serializable]
    public class ConnectPoint
    {
        public Transform point;
        public bool isConnected;        //if true, a room is attached to this point.
        public bool isSelected;         //if true, a room is going to be connected to this point.
    }
    public List<ConnectPoint> connectPoints;
    // Start is called before the first frame update
    void Start()
    {
        ActivateConnectPoints();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //when a room is instantiated, this method must be run
    private void ActivateConnectPoints()
    {
        List<bool> activePoints = new List<bool>();
        foreach (ConnectPoint t in connectPoints)
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
                connectPoints[i].point.gameObject.SetActive(true);
            }
        }
        
    }

    public void ActivateConnectPoint(int point)
    {
        connectPoints[point].point.gameObject.SetActive(true);
    }

    //checks for rooms with no connect points. The first half number of rooms generated must have at least one connect point.
    public bool DeadEnd()
    {
        int inactivePointsCount = 0;
        for(int i = 0; i < connectPoints.Count; i++)
        {
            if (!connectPoints[i].point.gameObject.activeSelf)
            {
                inactivePointsCount++;
            }
           
        }

        if (inactivePointsCount >= connectPoints.Count)
            return true;
        else
            return false;
    }
}
