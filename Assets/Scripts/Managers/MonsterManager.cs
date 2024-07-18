using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Handles spawning monsters and destroying them. */
public class MonsterManager : MonoBehaviour
{
    public List<Monster> masterMonsterList;
    public List<Monster> activeMonsters;        //monsters currently in the dungeon
    public List<Monster> graveyard;             //killed monsters go here and are reused when necessary.
    public static MonsterManager instance;

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

    public void SpawnMonster(int monsterLevel)
    {
        Monster monster = Instantiate(masterMonsterList[0]);
        monster.InitialzeStats(monsterLevel);
        activeMonsters.Add(monster);
    }
}
