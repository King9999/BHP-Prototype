using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//not much to this. As long as this object is in debuffs, player can do nothing.
[CreateAssetMenu(menuName = "Effects/Character Effect/Dizzy", fileName = "charEffect_Dizzy")]
public class CharacterEffect_Dizzy : CharacterEffect
{
   void Reset()
    {
        effectName = "Dizzy";
        effectDetails = "Target cannot act. If target is damaged, dizzy is removed.";
        effectType = EffectType.Debuff;
    }
}
