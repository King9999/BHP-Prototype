using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Ninjas have high SPD and ATP, making them deadly if they can get the first strike.

NINJA BEHAVIOUR
-----
* They don't target monsters.
* Favour using trap cards and skills.

*/ 
[CreateAssetMenu(menuName = "AI Behaviour/Hunter/Ninja", fileName = "ai_ninja")]
public class Hunter_AI_Ninja : Hunter_AI
{
    
    void Reset()
    {
        behaviourType = BehaviourType.Ninja;       //internal info only. It tells me what kind of behaviour the hunter has.
        canAttackHunters = true;              
        canAttackMonsters = false;          //also includes bosses
        canOpenChests = true;
        canUseTerminals = true;
        rollStr = 35;
        rollVit = 10;
        rollSpd = 40;
        rollMnt = 15;
    }


}
