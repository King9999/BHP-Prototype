using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SuperAbility : Skill
{
    public float superMeter;
    public float chargePentalty, chargeRate;    //chargePenalty reduces the amount of meter gained.

    // Start is called before the first frame update
    void Reset()
    {
        chargePentalty = 1;
        chargeRate = 1;
    }


    public void AddMeter(float amount)
    {
        superMeter += amount * chargePentalty * chargeRate;

        if (superMeter < 0)
            superMeter = 0;

        if (superMeter > 1)
            superMeter = 1;
    }

    public override void ActivateSkill(Character user, Character target)
    {
        base.ActivateSkill(user, target);
    }

    public override void ActivateSkill(Character user, List<Character> targets)
    {
        base.ActivateSkill(user, targets);
    }
}
