using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Opportunists will seek out treasure and try to get the target item before anyone else. Once they have it, they will avoid combat 
 * as much as possible. High SPD helps them to avoid combat.

Opportunist BEHAVIOUR
-----
* Will attack hunters until they have the target item.
* Avoids monsters
* Prioritizes chests and terminals over hunters if the hunter doesn't have the target item.
* In combat, if they have the target item, they will surrender immediately, or they will run away.

*/ 
[CreateAssetMenu(menuName = "AI Behaviour/Hunter/Opportunist", fileName = "ai_opportunist")]
public class Hunter_AI_Opportunist : Hunter_AI
{
    // Start is called before the first frame update
    void Reset()
    {
        behaviourType = BehaviourType.Opportunist;       //internal info only. It tells me what kind of behaviour the hunter has.
        canAttackHunters = true;            //this changes to false if the hunter has the target item.
        canAttackMonsters = false;          //also includes bosses
        canOpenChests = true;
        canUseTerminals = true;
        rollStr = 10;
        rollVit = 10;
        rollSpd = 70;
        rollMnt = 10;
    }


}
