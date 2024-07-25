using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//mod's value must be generated randomly when this object comes into existence.
[CreateAssetMenu(menuName = "Item Mod/Life Ring (Unique)", fileName = "itemMod_unique_lifeRing")]
public class ItemMod_LifeRing : ItemMod
{
    const float hpBonus = 1.15f;

    private void Reset()
    {
        modName = "Max HP + 15%";
    }

    public override void ActivateOnEquip(Hunter hunter)
    {
        hunter.maxHealthPoints *= hpBonus;
        hunter.maxHealthPoints = Mathf.Round(hunter.maxHealthPoints);
    }

    public override void DeactivateOnUnequip(Hunter hunter)
    {
        hunter.maxHealthPoints /= hpBonus;
        hunter.maxHealthPoints = Mathf.Round(hunter.maxHealthPoints);
        if (hunter.healthPoints > hunter.maxHealthPoints)
            hunter.healthPoints = hunter.maxHealthPoints;
    }
}
