using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/* Base class for all usable items */
public abstract class Item : ScriptableObject
{
    public string itemName, details;
    public int price;
    public int salePrice;       //how much an item costs when selling it. Can be adjusted by bartering
    public bool isKeyItem;      //key items cannot be sold or dropped.
    public bool isTargetItem;   //the target item required to complete a dungeon.
    public Sprite sprite;
    [System.NonSerialized] protected bool isEquipped = false;     //applies to weapons, armor, and accessories. NonSerialized means Unity will reset the variable state

    public enum ItemType
    {
        Loot, Weapon, Armor, Accessory, Consumable, DunegonMod, DataLog
    }

    public ItemType itemType;

    //public ItemEffect effect; //uncomment this once I have a scriptable object

    public virtual void ActivateEffect() { }
    public bool IsEquipped() { return isEquipped; }
    public virtual void Equip(Hunter hunter)
    {
        if (isEquipped)
            return;
    }
    public virtual void Unequip(Hunter hunter)
    {
        if (!isEquipped)
            return;
    }
}
