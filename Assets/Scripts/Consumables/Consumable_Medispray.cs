using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Consumable/Medispray", fileName = "consumable_medispray")]
public class Consumable_Medispray : Consumable
{
    private const float multiplier = 0.33f;
    // Start is called before the first frame update
    void Reset()
    {
        itemName = "Medispray";
        details = "Applies medicine that's absorbed through the skin. Restores 1/3 HP";
        itemID = "consumable_medispray";
        price = 100;
    }

    public override void ActivateEffect(Hunter user)
    {
        user.healthPoints += Mathf.Round(user.healthPoints * multiplier);
        Debug.Log("Restoring " + Mathf.Round(user.healthPoints * multiplier) + " HP");

        if (user.healthPoints > user.maxHealthPoints)
            user.healthPoints = user.maxHealthPoints;
    }
}
