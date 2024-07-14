using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : Character
{
    [Header("Allocation Points")]
    private int totalAllocationPoints;
    public int currentAllocationPoints;
    public int strPoints, spdPoints, vitPoints, mntPoints;  //AP is distributed to these values
    private int maxHunterLevel { get; } = 50;
    public int hunterLevel = 1;
    public bool isAI;

    /*** EQUIPMENT & SKILLS (uncomment these once the classes exist)
     * 
     * public List<ActiveSkill> activeSkills;   //
     * public List<PassiveSkill> passiveSkills;
     * public SuperAbility super;
     * 
     * ***/

    public Weapon equippedWeapon;
    public Armor equippedArmor;
    public Accessory equippedAccessory;

    private void Start()
    {
        //InitializeStats();
    }

    public void InitializeStats()
    {
        str = 1;
        vit = 1;
        spd = 1;
        mnt = 1;
        atp = 4;
        dfp = 1;
        mnp = 1;
        rst = 1;
        mov = 0;
        evd = 0.05f;
        healthPoints = 20; maxHealthPoints = 20;
        skillPoints = 4; maxSkillPoints = 4;
    }


    public void AddAllocationPoints(int amount)
    {
        if (hunterLevel >= maxHunterLevel || totalAllocationPoints >= maxHunterLevel || amount < 1)
            return;

        totalAllocationPoints += amount;
        currentAllocationPoints += amount;
    }

    public void AllocateToStr(int amount)
    {
        strPoints += amount;
        str = Mathf.FloorToInt(strPoints / 2);
    }

    public void AllocateToSpd(int amount)
    {
        spdPoints += amount;
        spd = Mathf.FloorToInt(spdPoints / 3);
    }

    public void AllocateToVit(int amount)
    {
        vitPoints += amount;
        vit += amount;
    }

    public void AllocateToMnt(int amount)
    {
        mntPoints += amount;
        mnt = Mathf.FloorToInt(mntPoints / 2);
    }
}
