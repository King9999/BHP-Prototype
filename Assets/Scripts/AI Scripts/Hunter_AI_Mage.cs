using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Mage is focused on using skills, so they have high MNT. If they run low on SP, they will rest or use items to recover it.

MAGE BEHAVIOUR
-----
* Always has an augmenter.
* Will always try to use skills.
* If they have less than 10% SP, they will use SP recovery items or rest.

*/ 
[CreateAssetMenu(menuName = "AI Behaviour/Hunter/Mage", fileName = "ai_mage")]
public class Hunter_AI_Mage : Hunter_AI
{
    // Start is called before the first frame update
    void Reset()
    {
        behaviourType = BehaviourType.Mage;       //internal info only. It tells me what kind of behaviour the hunter has.
        canAttackHunters = true;            
        canAttackMonsters = true;          //also includes bosses
        canOpenChests = true;
        canUseTerminals = true;
        rollStr = 2;
        rollVit = 3;
        rollSpd = 10;
        rollMnt = 85;
    }


}
