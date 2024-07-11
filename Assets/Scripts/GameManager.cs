using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* handles the game state */
public class GameManager : MonoBehaviour
{
    public List<ItemMod> mods;

    // Start is called before the first frame update
    void Start()
    {
        ItemMod hpMod = ScriptableObject.CreateInstance<ItemMod_IncreaseHP_One>();
        mods.Add(hpMod);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
