using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ActiveSkill : Skill
{
    public int skillCost, skillCharges;  //cost = SP cost, skillCharges = fixed number of uses before skill can't be used anymore.
                                         
}
