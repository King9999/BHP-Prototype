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

    public override void ApplyEffect(Character user)
    {
        Debug.LogFormat("{0} is stunned. Turn is over", user.characterName);
        GameManager gm = Singleton.instance.GameManager;
        gm.ForceStop = true;
    }

    public override void UpdateEffect(Character user)
    {
        //if we get here, apply effect
        Debug.LogFormat("{0} is stunned. Turn is passed", user.characterName);
        GameManager gm = Singleton.instance.GameManager;
        gm.ForceStop = true;
        base.UpdateEffect(user);
    }

    //remove dizzy status
    public override void CleanupEffect(Character user)
    {
        Debug.LogFormat("Removing dizzy status from {0}", user.characterName);
        base.CleanupEffect(user);   
    }
}
