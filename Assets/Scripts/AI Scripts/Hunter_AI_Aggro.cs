using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*AGGRO BEHAVIOUR
-----
* Always attacks hunters and monsters, whichever is closer
* Ignores chests and terminals
* 
*/

[CreateAssetMenu(menuName = "AI Behaviour/Hunter/Aggro", fileName = "ai_aggro")]
public class Hunter_AI_Aggro : Hunter_AI
{
    // Start is called before the first frame update
    void Reset()
    {
        behaviourType = "Aggro";        //internal info only. It tells me what kind of behaviour the hunter has.
        canAttackHunters = true;
        canAttackMonsters = true;          //also includes bosses
        canOpenChests = false;
        canUseTerminals = false;
        rollStr = 0.85f;
        rollVit = 0.05f;
        rollSpd = 0.1f;
        rollMnt = 0;
    }


}
