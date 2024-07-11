using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//mod's value must be generated randomly when this object comes into existence.
[CreateAssetMenu(menuName = "Item Mod/Increase HP Lv 1", fileName = "itemMod_increaseHP_1")]
public class ItemMod_IncreaseHP_One : ItemMod
{
    private int hp = 3;
    
    //once the object is created, a random HP value is generated.
    void Awake()
    {
        hp = Random.Range(hp, Mathf.CeilToInt(hp * 1.5f) + 1);
        //Debug.Log("Item mod's HP value is " + hp);
        modName = "HP + " + hp;
    }

    /*private void Reset()
    {
        modName = "HP + " + hp;
    }*/

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
