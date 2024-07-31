using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Rooms are the building blocks for creating dungeons. Each room can have connect points, which are used to attach rooms together.
 * A room can have multiple points, but which ones are enabled is random. */
public class Room : MonoBehaviour
{
    public List<Transform> connectPoints;
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
        foreach (Transform t in connectPoints)
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
                connectPoints[i].gameObject.SetActive(true);
            }
        }
        
    }
}
