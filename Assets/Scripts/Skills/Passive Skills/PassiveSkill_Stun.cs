using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Skills/Passive Skills/Stun", fileName = "passiveSkill_Stun")]
public class PassiveSkill_Stun : PassiveSkill
{
    private float dizzyChance = 0.5f;
    // Start is called before the first frame update
    void Reset()
    {
        skillName = "Stun";
        skillID = "passiveSkill_Stun";
        skillDetails = "Chance to inflict Dizzy after target takes damage";
        triggerOnHit = true;
        skillEffectDuration = 1;
    }

    //after hitting a target, there's a chance the target may be dizzied
    public override void ActivateSkill(Character user, Character target)
    {
        base.ActivateSkill(user, target);

        if (Random.value <= dizzyChance - target.resistDizzy)
        {
            //target is dizzied
            EffectManager em = Singleton.instance.EffectManager;
            em.AddEffect(StatusEffect.Effect.Dizzy, target);
        }
    }
}
