using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//increases max HP by 15%
[CreateAssetMenu(menuName = "Item/Equipment/Life Ring", fileName = "acc_lifeRing")]
public class Accessory_LifeRing : Accessory
{
    const float hpBonus  = 1.15f;

    private void Reset()
    {
        itemName = "Life Ring";
        details = "Increases max HP by 15%";
    }

    public override void Equip(Hunter hunter)
    {
        base.Equip(hunter);

        hunter.maxHealthPoints *= hpBonus;
        hunter.maxHealthPoints = Mathf.Round(hunter.maxHealthPoints);
    }

    public override void Unequip(Hunter hunter)
    {
        base.Unequip(hunter);

        hunter.maxHealthPoints /= hpBonus;
        hunter.maxHealthPoints = Mathf.Round(hunter.maxHealthPoints);
        if (hunter.healthPoints > hunter.maxHealthPoints)
            hunter.healthPoints = hunter.maxHealthPoints;
    }
}
