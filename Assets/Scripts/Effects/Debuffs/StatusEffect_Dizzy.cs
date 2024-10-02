using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//not much to this. As long as this object is in debuffs, player can do nothing.
[CreateAssetMenu(menuName = "Effects/Status Effects/Dizzy", fileName = "statEffect_Dizzy")]
public class StatusEffect_Dizzy : StatusEffect
{
   void Reset()
    {
        effectName = "Dizzy";
        effectDetails = "Target cannot act. If target is damaged, dizzy is removed.";
        effectType = EffectType.Debuff;
        hasDuration = true;
    }

    //remove dizzy status
    public override void CleanupEffect(Character user)
    {
        Debug.Log("Removing dizzy status from " + user.characterName);
        base.CleanupEffect(user);   
    }
}
