using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//handles all traps in the game.
public class TrapManager : MonoBehaviour
{
    [SerializeField] private List<Trap> masterTrapList;

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
                trap = Instantiate(masterTrapList[i]);
            }
            else
            {
                i++;
            }
        }

        return trap;
    }
}
