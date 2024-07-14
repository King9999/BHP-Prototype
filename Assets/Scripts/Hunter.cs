using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : Character
{
    [Header("Allocation Points")]
    private int totalAllocationPoints;
    public int currentAllocationPoints;
    public float strPoints, spdPoints, vitPoints, mntPoints;  //AP is distributed to these values
    private int maxHunterLevel { get; } = 50;
    public int hunterLevel = 1;
    public bool isAI;

    //base values
    private const float baseAtp = 4;
    private const float baseDfp = 1;
    private const float baseMnp = 1;
    private const float baseEvd = 0.05f;
    private const float baseRst = 1;
    private const float baseMov = 0;

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
        atp = baseAtp;
        dfp = baseDfp;
        mnp = baseMnp;
        rst = baseRst;
        mov = baseMov;
        evd = baseEvd;
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
        Debug.Log("STR: " + strPoints / 2);
        //str = Mathf.Floor(strPoints / 2);
        if (Mathf.Floor(strPoints % 2) == 0)
        {
            str += 1;
            //atp += 1;
            atp = equippedWeapon == null ? baseAtp + Mathf.Floor(str / 2) : baseAtp + Mathf.Floor(str / 2) + equippedWeapon.atp;
        }
    }

    public void AllocateToSpd(int amount)
    {
        spdPoints += amount;
        //spd = Mathf.FloorToInt(spdPoints / 3);
        if (Mathf.Floor(spdPoints % 3) == 0)
            spd += 1;
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
