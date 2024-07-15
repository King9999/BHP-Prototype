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
    private const float baseHealthPoints = 20;      //max health
    private const float baseSkillPoints = 4;        //max Sp

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
        atp = baseAtp + str;
        dfp = baseDfp + Mathf.Floor(vit / 2);
        mnp = baseMnp + mnt;
        rst = baseRst + Mathf.Floor(mnt / 2);
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
        maxHealthPoints += amount * 2;
        maxSkillPoints += amount;
        //Debug.Log("STR: " + strPoints / 2);
        //str = Mathf.Floor(strPoints / 2);
        //if (Mathf.Floor(strPoints % 2) == 0)
        //{
            str += amount;
            atp = equippedWeapon == null ? baseAtp + Mathf.Floor(str / 2) : baseAtp + Mathf.Floor(str / 2) + equippedWeapon.atp;
            //TODO: must apply any other bonuses, such as accessories.
        //}
    }

    public void AllocateToSpd(int amount)
    {
        spdPoints += amount;
        maxHealthPoints += amount * 2;
        maxSkillPoints += amount;
        //spd = Mathf.FloorToInt(spdPoints / 3);
        //if (Mathf.Floor(spdPoints % 3) == 0)
        //{
        spd += amount;

            mov = baseMov + Mathf.Floor(spd / 3);
            evd = baseEvd + (Mathf.Floor(spd / 2) * 0.02f);
        //}

    }

    public void AllocateToVit(int amount)
    {
        vitPoints += amount;
        vit += amount;

        dfp = equippedArmor == null ? baseDfp + Mathf.Floor(vit / 2) : baseDfp + Mathf.Floor(vit / 2) + equippedArmor.dfp;
        maxHealthPoints += (amount * 2) + 1;    //regular increase + VIT bonus
        maxSkillPoints += amount;
        //TODO: must apply any other bonuses, such as accessories.
    }

    public void AllocateToMnt(int amount)
    {
        mntPoints += amount;
        mnt += amount;
        maxHealthPoints += amount * 2;
        maxSkillPoints += amount * 2;   //regular increase + MNT bonus
        mnp = equippedWeapon == null ? baseMnp + Mathf.Floor(mnt) : baseMnp + Mathf.Floor(mnt) + equippedWeapon.mnp;
        rst = equippedArmor == null ? baseRst + Mathf.Floor(mnt / 2) : baseRst + Mathf.Floor(mnt / 2) + equippedArmor.rst;
    }
}
