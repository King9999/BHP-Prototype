using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Manages all items in the game. Used to generate items dynamically */
public class ItemManager : MonoBehaviour
{
    public List<Weapon> masterWeaponList;
    public static ItemManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Weapon GenerateWeapon()
    {
        Weapon weapon = Instantiate(masterWeaponList[0]);
        Debug.Log("Weapon ATP for " + weapon.itemName + ": " + weapon.atp);
        return weapon;
    }
}
