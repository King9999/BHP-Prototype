using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//mod's value must be generated randomly when this object comes into existence.
[CreateAssetMenu(menuName = "Item Mod/Increase HP Lv 2", fileName = "itemMod_increaseHP_2")]
public class ItemMod_IncreaseHP_Two : ItemMod
{
    private float hp = 8;
    
    //once the object is created, a random HP value is generated. Must use Awake(), not Start()!
    void Awake()
    {
        hp = Random.Range(hp, hp * 1.5f);
        //Debug.Log("Item mod's HP value before rounded: " + hp);
        hp = Mathf.Round(hp);
        
        modName = "HP + " + hp;
    }

    private void Reset()
    {
        modName = "HP + (8-12)";
        modID = "ItemMod_IncreaseHP_Two";
        modLevel = 2;
    }

    public override void ActivateOnEquip(Hunter hunter)
    {
        hunter.maxHealthPoints += hp;
        hunter.healthPoints += hp;
    }

    public override void DeactivateOnUnequip(Hunter hunter)
    {
        hunter.maxHealthPoints -= hp;
        if (hunter.healthPoints > hunter.maxHealthPoints)
            hunter.healthPoints = hunter.maxHealthPoints;
    }
}
