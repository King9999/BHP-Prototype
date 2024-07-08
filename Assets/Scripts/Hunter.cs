using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : Character
{
    private int totalAllocationPoints;
    public int currentAllocationPoints;
    private int maxHunterLevel { get; } = 50;
    public int hunterLevel = 1;
    public bool isAI;

    /*** EQUIPMENT & SKILLS (uncomment these once the classes exist)
     * public Weapon weapon;
     * public Armor armor;
     * public Accessory accessory;
     * 
     * public List<ActiveSkill> activeSkills;   //
     * public List<PassiveSkill> passiveSkills;
     * public SuperAbility super;
     * 
     * ***/
   

    public void AddAllocationPoints(int amount)
    {
        if (hunterLevel >= maxHunterLevel || totalAllocationPoints >= maxHunterLevel || amount < 1)
            return;

        totalAllocationPoints += amount;
        currentAllocationPoints += amount;
    }
}
