using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Handles spawning monsters and destroying them. */
public class MonsterManager : MonoBehaviour
{
    public List<Monster> masterMonsterList;
    public List<Monster> activeMonsters;        //monsters currently in the dungeon
    public List<Monster> graveyard;             //killed monsters go here and are reused when necessary.
    public List<Entity_Spawner> spawners;
    public static MonsterManager instance;

    private int spawnTimer { get; } = 8;
    public int spawnMod;                        //used by dungeon mod to adjust spawn timer.
    private int MaxMonsters { get; } = 8;       //doesn't include boss

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public int GetMonsterLimit()
    {
        HunterManager hm = Singleton.instance.HunterManager;
        return MaxMonsters - hm.hunters.Count;
    }

    public bool TimeToSpawnMonster()
    {
        GameManager gm = Singleton.instance.GameManager;
        return gm.turnCount % (spawnTimer - spawnMod) == 0;
    }

    public Monster SpawnMonster(int monsterLevel)
    {
        int randMonster = Random.Range(0, masterMonsterList.Count);
        Monster monster = Instantiate(masterMonsterList[randMonster]);
        monster.InitializeStats(monsterLevel);
        

        //TODO choose a spawner to spawn from.


        activeMonsters.Add(monster);
        return monster;
    }
}
