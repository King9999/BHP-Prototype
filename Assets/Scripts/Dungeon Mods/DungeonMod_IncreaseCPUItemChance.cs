using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//raies the likelihood of CPU Hunters carrying items.
[CreateAssetMenu(menuName = "Item/Dungeon Mod/Increase CPU Item Chance", fileName = "dungeonMod_increaseCpuItemChance")]
public class DungeonMod_IncreaseCPUItemChance : DungeonMod
{
    public float itemChanceMod;
    void Reset()
    {
        itemName = "Increase CPU Item Chance";
        itemID = "dungeonMod_increaseCpuItemChance";
        details = "Makes CPU Hunters more likely to carry items";
        itemType = ItemType.DunegonMod;
        price = 2000;
    }

    void Awake()    //this will execute when instantiated
    {
        GenerateModValue();
    }


    private void GenerateModValue()
    {
        itemChanceMod = Mathf.Round(Random.Range(0.5f, 2.5f) * 100f) / 100f;    //round to 2 decimal places.

        //mod goes up in increments of .05. Add an amount based on whether ones digit is less than or greater than 5.
        float onesDigit = itemChanceMod % 1;
        //Debug.Log(onesDigit);
        if (onesDigit > 0.5f)
        {
            itemChanceMod += 1 - onesDigit;
        }
        else if (onesDigit < 0.5f)
        {
            itemChanceMod += 0.5f - onesDigit;
        }

        itemName = "Increase CPU Item Chance +" + itemChanceMod * 100 + "%";
    }


    public override void ActivateMod()
    {
        HunterManager hm = Singleton.instance.HunterManager;
        hm.itemChanceMod = itemChanceMod;
        Debug.Log("Item Chance Mod: " + hm.itemChanceMod);
    }

    public override void DeactivateMod()
    {
        HunterManager hm = Singleton.instance.HunterManager;
        hm.itemChanceMod = 0;
    }
}
