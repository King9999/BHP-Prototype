using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//mod's value must be generated randomly when this object comes into existence.
[CreateAssetMenu(menuName = "Item Mod/Increase HP Lv 1", fileName = "itemMod_increaseHP_1")]
public class ItemMod_IncreaseHP_One : ItemMod
{
    private float hp = 3;
    
    //once the object is created, a random HP value is generated. Must use Awake(), not Start()!
    void Awake()
    {
        hp = Random.Range(hp, hp * 1.6f);
        //Debug.Log("Item mod's HP value before rounded: " + hp);
        hp = Mathf.Round(hp);
        
        modName = "HP + " + hp;
    }

    private void Reset()
    {
        modName = "HP + (3-5)";
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
