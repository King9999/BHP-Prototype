using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*AGGRO BEHAVIOUR
-----
* Always attacks hunters and monsters, whichever is closer
* Ignores chests and terminals
* At the start of each turn, there's a 30% chance that the hunter gains Berserk debuff.
*/

[CreateAssetMenu(menuName = "AI Behaviour/Hunter/Aggro", fileName = "ai_aggro")]
public class Hunter_AI_Aggro : Hunter_AI
{
    // Start is called before the first frame update
    void Reset()
    {
        behaviourType = BehaviourType.Aggro;        //internal info only. It tells me what kind of behaviour the hunter has.
        canAttackHunters = true;
        canAttackMonsters = true;          //also includes bosses
        canOpenChests = false;
        canUseTerminals = false;
        rollStr = 85;
        rollVit = 5;
        rollSpd = 10;
        rollMnt = 0;
    }

    //Hunter has a chance to gain berserk debuff
    public override void ActivateAbility(Hunter hunter)
    {
        if (Random.value <= 0.3f)
        {
            //add berserk
            //StatusEffect_Berserk berserk = new();     //want to check if this code works
            //if (hunter.debuffs.Contains(berserk))
            bool berserkFound = false;
            int i = 0;
            while (!berserkFound && i < hunter.debuffs.Count)
            {
                if (hunter.debuffs[i].effect == StatusEffect.Effect.Berserk)
                    berserkFound = true;
                else
                    i++;
            }

            if (!berserkFound)
            {
                EffectManager em = Singleton.instance.EffectManager;
                em.AddEffect(StatusEffect.Effect.Berserk, hunter);
                Debug.Log("Berserk triggered for " + hunter.characterName);
            }
            else
            {
                Debug.Log(" Berserk already active for " + hunter.characterName);
            }

        }
    }

}
