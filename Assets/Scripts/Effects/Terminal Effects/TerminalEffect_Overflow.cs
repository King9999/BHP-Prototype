using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Overflow heals hunter fully, and removes Injured debuff. Then, the amount of HP is doubled. Max HP is untouched. */
[CreateAssetMenu(menuName = "Effects/Terminal Effects/Overflow", fileName = "terminalEffect_Overflow")]
public class TerminalEffect_Overflow : TerminalEffect
{
    private void Reset()
    {
        effectID = EffectID.Overflow;
        effectMessage = "HP is doubled!";
        effectName = "Overflow";
    }

    public override void ActivateEffect(Hunter hunter)
    {
        //first, remove Injured status
        bool debuffFound = false;
        int i = 0;
        StatusEffect_Injured injured = hunter.GetStatusEffect(StatusEffect.Effect.Injured, hunter.debuffs) as StatusEffect_Injured;
        if (injured != null)
        {
            injured.CleanupEffect(hunter);
        }

        /*while(!debuffFound && i < hunter.debuffs.Count)
        {
            if (hunter.debuffs[i] is StatusEffect_Injured injured)
            {
                debuffFound = true;
                injured.CleanupEffect(hunter);
            }
            else
            {
                i++;
            }
        }*/

        //restore HP and double it.
        hunter.healthPoints = hunter.maxHealthPoints;
        hunter.healthPoints *= 2;

        HunterManager hm = Singleton.instance.HunterManager;
        hm.UpdateHunterHUD(hunter);

        //show message
        GameManager gm = Singleton.instance.GameManager;
        gm.DisplayTerminalBonus(hunter, effectMessage);
    }
}
