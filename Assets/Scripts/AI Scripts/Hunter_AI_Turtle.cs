using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Turtles have high defense and HP, and they favour defending during combat. It will be difficult to get the target item from
 * them if they get it.

TURTLE BEHAVIOUR
-----
* If they have low HP (10%), they will rest.
* In combat, if they have the target item, they are more likely to defend.

*/ 
[CreateAssetMenu(menuName = "AI Behaviour/Hunter/Turtle", fileName = "ai_turtle")]
public class Hunter_AI_Turtle : Hunter_AI
{
    
    void Reset()
    {
        behaviourType = BehaviourType.Turtle;       //internal info only. It tells me what kind of behaviour the hunter has.
        canAttackHunters = true;              
        canAttackMonsters = true;          //also includes bosses
        canOpenChests = true;
        canUseTerminals = true;
        rollStr = 10;
        rollVit = 85;
        rollSpd = 3;
        rollMnt = 2;
    }


}
