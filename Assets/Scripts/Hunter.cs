using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Hunter : Character
{
    [Header("Allocation Points")]
    private int totalAllocationPoints;
    public int currentAllocationPoints;
    public int startingAllocationPoints;    //free points player gets when creating a hunter
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
    private const int baseMov = 0;
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

    public List<Item> inventory;          //10 items max
    public List<Item> stash;              //100 items max
    public int credits;                     //money on hand
    public int maxInventoryCount { get; } = 10;
    public int maxStashCount { get; } = 100;

    [Header("---Cards---")]
    public List<CardObject> cards;

    private void Start()
    {
        //InitializeStats();
    }

    public void InitializeStats()
    {
        hunterLevel = 5;
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
        startingAllocationPoints = 16;
    }


    public void AddAllocationPoints(int amount)
    {
        if (hunterLevel >= maxHunterLevel || totalAllocationPoints >= maxHunterLevel || amount < 1)
            return;

        totalAllocationPoints += amount;
        currentAllocationPoints += amount;
    }

    //newHunter = newly created hunter, don't apply HP or SP bonus since levels aren't gained during creation phase.
    public void AllocateToStr(int amount, bool newHunter = false)
    {
        if (strPoints + amount < 0)     //can't go below 0 in case of deallocation
            return;

        hunterLevel += newHunter == false ? amount : 0;
        strPoints += amount;
        maxHealthPoints += newHunter == false ? amount * 2 : 0;
        maxSkillPoints += newHunter == false ? amount : 0;
        //Debug.Log("STR: " + strPoints / 2);
        
        str += amount;
        atp = equippedWeapon == null ? baseAtp + Mathf.Floor(str / 2) : baseAtp + Mathf.Floor(str / 2) + equippedWeapon.atp;
            //TODO: must apply any other bonuses, such as accessories.
       
    }

    public void AllocateToSpd(int amount, bool newHunter = false)
    {
        if (spdPoints + amount < 0)
            return;

        hunterLevel += newHunter == false ? amount : 0;
        spdPoints += amount;
        maxHealthPoints += newHunter == false ? amount * 2 : 0;
        maxSkillPoints += newHunter == false ? amount : 0;
        spd += amount;

        mov = baseMov + Mathf.FloorToInt(spd / 5);
        evd = baseEvd + spd * 0.01f;
    }

    public void AllocateToVit(int amount, bool newHunter = false)
    {
        if (vitPoints + amount < 0)
            return;

        hunterLevel += newHunter == false ? amount : 0;
        vitPoints += amount;
        vit += amount;

        dfp = equippedArmor == null ? baseDfp + Mathf.Floor(vit / 2) : baseDfp + Mathf.Floor(vit / 2) + equippedArmor.dfp;
        maxHealthPoints += newHunter == false ? (amount * 2) + 1 : amount;    //regular increase + VIT bonus
        maxSkillPoints += newHunter == false ? amount : 0;
        //TODO: must apply any other bonuses, such as accessories.
    }

    public void AllocateToMnt(int amount, bool newHunter = false)
    {
        if (mntPoints + amount < 0)
            return;

        hunterLevel += newHunter == false ? amount : 0;
        mntPoints += amount;
        mnt += amount;
        maxHealthPoints += newHunter == false ? amount * 2 : 0;
        maxSkillPoints += newHunter == false ? amount * 2 : amount;   //regular increase + MNT bonus
        mnp = equippedWeapon == null ? baseMnp + Mathf.Floor(mnt) : baseMnp + Mathf.Floor(mnt) + equippedWeapon.mnp;
        rst = equippedArmor == null ? baseRst + Mathf.Floor(mnt / 2) : baseRst + Mathf.Floor(mnt / 2) + equippedArmor.rst;
    }

    /** equip gear ***/
    public void Equip(Weapon weapon)
    {
        //item can only be equipped if the player meets the level requirement
        if (hunterLevel < weapon.itemLevel || equippedWeapon == weapon)
            return;

        //if weapon is already equipped, remove that weapon first
        if (equippedWeapon != null)
        {
            Unequip(equippedWeapon);
        }

        equippedWeapon = weapon;
        atp += weapon.atp;
        mnp += weapon.mnp;

        if (weapon.itemMods.Count > 0)
        {
            //apply effects of mods
            foreach (ItemMod mod in weapon.itemMods)
            {
                mod.ActivateOnEquip(this);
            }
        }

        //TODO: if there's a skill, add it to hunter's inventory.
    }

    public void Unequip(Weapon weapon)
    {
        if (equippedWeapon == null) return;

        equippedWeapon = null;
        atp -= weapon.atp;
        mnp -= weapon.mnp;

        if (weapon.itemMods.Count > 0)
        {
            //remove effects of item mods
            foreach (ItemMod mod in weapon.itemMods)
            {
                mod.DeactivateOnUnequip(this);
            }
        }

        //TODO: remove skill from inventory
    }

    public void Equip(Armor armor)
    {

    }

    public void Unequip(Armor armor)
    {

    }

    public void Equip(Accessory accessory)
    {

    }
    public void Unequip(Accessory accessory)
    {

    }

    public void Rest() 
    {
        //restore 25% HP and SP, and draw a card
        float hpAmount = Mathf.Round(maxHealthPoints * 0.25f);
        float spAmount = Mathf.Round(maxSkillPoints * 0.25f);

        healthPoints = healthPoints + hpAmount > maxHealthPoints ? maxHealthPoints : healthPoints + hpAmount;
        skillPoints = skillPoints + spAmount > maxSkillPoints ? maxSkillPoints : skillPoints + spAmount;
        Debug.Log(characterName + " is resting! Restored " + hpAmount + " HP and " + spAmount + "SP.");

        //TODO: draw a card
    }
}
