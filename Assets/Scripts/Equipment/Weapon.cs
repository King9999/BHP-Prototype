using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Weapons raise either ATP or MNP, and can come with mods. */
[CreateAssetMenu(menuName = "Item/Equipment/Weapon", fileName = "weapon_")]
public class Weapon : Item
{
    public float atp, mnp;
    public int minRange, maxRange;
    
    public List<ItemMod> itemMods;  //if there's a chip slot, there can only be 1 item mod.
    public Skill itemSkill;         //the skill used if chip is inserted into the item.
    public bool hasChipSlot;        //if true, itemSkill is available.
    public bool isUniqueItem;       //if true, chip slot counts as 1 item mod instead of 2.
    public int modCount = 3;        //default is 3. If item is not unique and has a chip slot, this value is 1.
                                    //If item is unique, this value is 2.
    public bool isEquipped;
    public enum WeaponType { BeamSword, Railgun, Augmenter }
    public WeaponType weaponType;
    public enum DamageType { Physical, Psychic }
    public DamageType damageType;

    void Reset()
    {
        itemType = ItemType.Weapon; //default type
        itemLevel = 1;
        //isEquipped = false;
    }

    //only applies when augmenter is used.
    public void ApplyAugmenterDebuff(Character character)
    {
        EffectManager em = Singleton.instance.EffectManager;

        StatusEffect.Effect[] augmenterDebuffs = { StatusEffect.Effect.Dizzy, StatusEffect.Effect.CardDrain, 
            StatusEffect.Effect.Weakened, StatusEffect.Effect.Blind };

        int randEffect = Random.Range(0, augmenterDebuffs.Length + 1);

        if (!em.DebuffResisted(augmenterDebuffs[randEffect], character))
            em.AddEffect(augmenterDebuffs[randEffect], character);   
    }
   
}
