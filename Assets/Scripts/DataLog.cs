using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Data Logs contain information about the world, leading up to the catastrophic event. These are key items that don't do anything other than provide information
 to the player.*/
[CreateAssetMenu(menuName = "Item/Data Log", fileName = "log_")]
public class DataLog : Item
{
    public AudioClip voiceLog;          //in case we can get a voice actor(s), I want to have voiced logs.

    private void Reset()    //Reset() is called by Unity when item is created in the inspector after pressing Enter
    {
        itemType = ItemType.DataLog;
        isKeyItem = true;
    }
   

}
