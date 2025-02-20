using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
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
    //public bool isAI;
    public Hunter_AI cpuBehaviour;

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
    public SuperAbility super;

    public List<Item> inventory;          //10 items max
    public Item targetItem;             //easy way to check if hunter has the target.
    //public List<Item> stash;              //100 items max. THIS SHOULD NOT BE ATTACHED TO HUNTER, ONLY 1 STASH SHARED BETWEEN ALL HUNTERS
    public int credits;                     //money on hand
    public int MaxInventorySize { get; } = 10;
    public int HudID { get; set; }                       //used to quickly identify hunter's HUD in game.
    //public int maxStashCount { get; } = 100;

    [Header("---Cards---")]
    public List<Card> cards;
    public Card combatCard;                 //card that's selected during combat.

    [Header("---Terminal Effects---")]
    public List<TerminalEffect> terminalEffects;

    //conditions
    public bool ForceTeleport { get; set; }             //if true, hunter is teleported either due to injury or after running/surrendering.
    public bool CanDrawCard { get; set; } = true;     //affected by card drain debuff.


    public void InitializeStats()
    {
        hunterLevel = 1;
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

        //update 'Basic Attack' skill
        if (skills[0] is ActiveSkill basicAttack)
        {
            basicAttack.minRange = equippedWeapon.minRange;
            basicAttack.maxRange = equippedWeapon.maxRange;
            
            //change range based on weapon type
            switch(equippedWeapon.weaponType)
            {
                case Weapon.WeaponType.BeamSword:
                    basicAttack.skillRange = ActiveSkill.SkillRange.Melee;
                    break;

                case Weapon.WeaponType.Railgun:
                    basicAttack.skillRange = ActiveSkill.SkillRange.Ranged;
                    break;

                case Weapon.WeaponType.Augmenter:
                    basicAttack.skillRange = ActiveSkill.SkillRange.Versatile;
                    break;
            }
        }

        if (weapon.itemMods.Count > 0)
        {
            //apply effects of mods
            foreach (ItemMod mod in weapon.itemMods)
            {
                mod.ActivateOnEquip(this);
            }
        }

        //TODO: if there's a skill, add it to hunter's inventory.
        if (weapon.itemSkill != null)
            skills.Add(weapon.itemSkill);

       
    }

    public void Unequip(Weapon weapon)
    {
        if (equippedWeapon == null) return;

        equippedWeapon = null;
        atp -= weapon.atp;
        mnp -= weapon.mnp;

        //update 'Basic Attack' skill
        if (skills[0] is ActiveSkill basicAttack)
        {
            basicAttack.minRange = 0;
            basicAttack.maxRange = 1;
            basicAttack.skillRange = ActiveSkill.SkillRange.Melee;
        }

        if (weapon.itemMods.Count > 0)
        {
            //remove effects of item mods
            foreach (ItemMod mod in weapon.itemMods)
            {
                mod.DeactivateOnUnequip(this);
            }
        }

        //TODO: remove skill from inventory
        if (weapon.itemSkill != null)
            skills.Remove(weapon.itemSkill);
    }

    public void Equip(Armor armor)
    {
        //item can only be equipped if the player meets the level requirement
        if (hunterLevel < armor.itemLevel || armor.isEquipped)
            return;

        armor.isEquipped = true;
        equippedArmor = armor;
        dfp = vit + armor.dfp;
        rst = Mathf.Round(mnt / 2) + armor.rst;

        if (armor.itemMods.Count > 0)
        {
            //apply effects of mods
            foreach (ItemMod mod in armor.itemMods)
            {
                mod.ActivateOnEquip(this);
            }
        }

        //TODO: if there's a skill, add it to hunter's inventory.
        if (armor.itemSkill != null)
            skills.Add(armor.itemSkill);
    }

    public void Unequip(Armor armor)
    {
        //item can only be equipped if the player meets the level requirement
        if (equippedArmor == null || !armor.isEquipped)
            return;

        armor.isEquipped = false;
        equippedArmor = null;
        dfp = vit - armor.dfp;
        rst = Mathf.Round(mnt / 2) - armor.rst;

        if (armor.itemMods.Count > 0)
        {
            //apply effects of mods
            foreach (ItemMod mod in armor.itemMods)
            {
                mod.DeactivateOnUnequip(this);
            }
        }

        //remove skill from inventory
        if (armor.itemSkill != null)
            skills.Remove(armor.itemSkill);
    }

    public void Equip(Accessory acc)
    {
        //item can only be equipped if the player meets the level requirement
        if (hunterLevel < acc.itemLevel || acc.isEquipped)
            return;

        acc.isEquipped = true;
        equippedAccessory = acc;
        atp += acc.atp;
        mnp += acc.mnp;
        dfp += acc.dfp;
        rst += acc.rst;
        str += acc.str;
        vit += acc.vit;
        spd += acc.spd;
        mnt += acc.mnt;
        evd += acc.evd;
        mov += acc.mov;

        if (acc.itemMods.Count > 0)
        {
            //apply effects of mods
            foreach (ItemMod mod in acc.itemMods)
            {
                mod.ActivateOnEquip(this);
            }
        }

        //TODO: if there's a skill, add it to hunter's inventory.
        if (acc.itemSkill != null)
            skills.Add(acc.itemSkill);
    }
    public void Unequip(Accessory acc)
    {
        if (equippedAccessory == null || !acc.isEquipped)
            return;

        acc.isEquipped = false;
        equippedAccessory = null;
        atp -= acc.atp;
        mnp -= acc.mnp;
        dfp -= acc.dfp;
        rst -= acc.rst;
        str -= acc.str;
        vit -= acc.vit;
        spd -= acc.spd;
        mnt -= acc.mnt;
        evd -= acc.evd;
        mov -= acc.mov;

        if (acc.itemMods.Count > 0)
        {
            //apply effects of mods
            foreach (ItemMod mod in acc.itemMods)
            {
                mod.DeactivateOnUnequip(this);
            }
        }

        //remove skill
        if (acc.itemSkill != null)
            skills.Remove(acc.itemSkill);
    }

    public void Rest() 
    {
        //restore 25% HP and SP, and draw a card
        float hpAmount = Mathf.Round(maxHealthPoints * 0.25f);
        float spAmount = Mathf.Round(maxSkillPoints * 0.25f);

        healthPoints = healthPoints + hpAmount > maxHealthPoints ? maxHealthPoints : healthPoints + hpAmount;
        skillPoints = skillPoints + spAmount > maxSkillPoints ? maxSkillPoints : skillPoints + spAmount;
        Debug.Log(characterName + " is resting! Restored " + hpAmount + " HP and " + spAmount + "SP.");

        CardManager cm = Singleton.instance.CardManager;
        cm.DrawCard(this, cm.deck);
    }

    public bool HasTargetItem()
    {
        return targetItem != null;
        /*bool targetFound = false;
        int i = 0;
        while (!targetFound && i < inventory.Count)
        {
            if (inventory[i].isTargetItem)
                targetFound = true;
            else
                i++;
        }

        return targetFound;*/
    }
}
