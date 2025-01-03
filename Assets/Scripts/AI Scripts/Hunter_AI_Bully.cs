using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*bullies will pick a hunter and follow them and attack them until they're dead. Once the target is dead, the bully 
picks another target, which can also include the same hunter. The bully's stat spread is totally random.

BULLY BEHAVIOUR
-----
* Always attacks hunters
* Avoids monsters
* Ignores chests and terminals
* Has an innate ability that causes them to target a hunter and to always attack them until they're dead.

*/ 
[CreateAssetMenu(menuName = "AI Behaviour/Hunter/Bully", fileName = "ai_bully")]
public class Hunter_AI_Bully : Hunter_AI
{
    public Hunter bullyTarget;      //bully will always target this hunter when possible.

    // Start is called before the first frame update
    void Reset()
    {
        behaviourType = BehaviourType.Bully;        //internal info only. It tells me what kind of behaviour the hunter has.
        canAttackHunters = true;
        canAttackMonsters = false;          //also includes bosses
        canOpenChests = false;
        canUseTerminals = false;
        rollStr = 25;
        rollVit = 25;
        rollSpd = 25;
        rollMnt = 25;
    }

    public override void ActivateAbility(Hunter hunter)
    {
        //find a hunter to target.
        if (bullyTarget == null)
        {
            HunterManager hm = Singleton.instance.HunterManager;

            //first check if one of the hunters has the target item
            bool hunterFound = false;
            int i = 0;
            while (!hunterFound && i < hm.hunters.Count)
            {
                if (hm.hunters[i] == hunter)
                {
                    i++;
                    continue;
                }

                if (hm.hunters[i].HasTargetItem())
                {
                    hunterFound = true;
                    bullyTarget = hm.hunters[i];
                }
                else
                {
                    i++;
                }

            }

            //if we get here, no hunters have the target item. Pick a random hunter to bully.
            while (bullyTarget == null)
            {
                int randHunter = Random.Range(0, hm.hunters.Count);

                if (hm.hunters[randHunter] == hunter)
                    continue;

                bullyTarget = hm.hunters[randHunter];

            }
        }
    }

}
