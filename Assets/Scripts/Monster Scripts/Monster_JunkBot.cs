using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Junk Bot is a basic enemy with no standout features. At higher levels they become a little more threatening. */
public class Monster_JunkBot : Monster
{
    // Start is called before the first frame update
    void Reset()
    {
        characterName = "Junk Bot";
        baseHealthPoints = 30;
        baseSkillPoints = 4;
        baseAtp = 6;
        baseDfp = 0;
        baseEvd = 0.05f;
        baseMnp = 0;
        baseRst = 0;
        baseSpd = 2;
        minAttackRange = 1;
        maxAttackRange = 1;
        baseMoney = 10;
        //InitialzeStats(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
