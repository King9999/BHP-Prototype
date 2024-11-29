using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//handles all traps in the game.
public class TrapManager : MonoBehaviour
{
    [SerializeField]private List<Trap> masterTrapList;
    public List<Room> activeTrapList;
    //private GameObject trapObjectList;
    //public static TrapManager instance;

    private void Awake()
    {
        /*if (instance != null && instance != this)
         {
             Destroy(gameObject);
             return;
         }

         instance = this;*/
        //trapObjectList = new GameObject("Traps in Field");
        //trapObjectList.transform.SetParent(this.transform); //parent should be singleton.
    }


    private void Start()
    {
        
    }

    public Trap GetTrap(Trap.TrapID trapID)
    {
        Trap trap = null;
        bool trapFound = false;
        int i = 0;
        while (!trapFound && i < masterTrapList.Count)
        {
            if (masterTrapList[i].trapID == trapID)
            {
                trapFound = true;
                trap = Instantiate(masterTrapList[i]/*, trapObjectList.transform*/);
                //activeTrapList.Add(trap);
            }
            else
            {
                i++;
            }
        }

        return trap;
    }

    //removes trap from trap list.
    public void RemoveTrap(Trap trap)
    {
        int i = 0;
        bool trapFound = false;
        while (!trapFound && i < activeTrapList.Count)
        {
            if (activeTrapList[i].trap == trap)
            {
                trapFound = true;
                Debug.LogFormat("Removed trap {0} from room {1}", trap.trapName, activeTrapList[i].roomID);
                activeTrapList[i].trap = null;
                activeTrapList.Remove(activeTrapList[i]);
            }
            else
            {
                i++;
            }
        }

        if (!trapFound)
            Debug.Log("No trap to remove");
        else
            Object.Destroy(trap);
    }
}
