using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Handles spawning monsters and destroying them. */
public class MonsterManager : MonoBehaviour
{
    public List<Monster> masterMonsterList;
    public List<Monster> activeMonsters;        //monsters currently in the dungeon
    public List<Monster> graveyard;             //killed monsters go here and are reused when necessary.
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
