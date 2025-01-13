using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Handles spawning monsters and destroying them. */
public class MonsterManager : MonoBehaviour
{
    public List<Monster> masterMonsterList;
    public List<Monster> activeMonsters;        //monsters currently in the dungeon
    public List<Monster> graveyard;             //killed monsters go here and are reused when necessary.
    //public List<Entity_Spawner> spawners;
    public static MonsterManager instance;
    private GameObject monsterObject;

    private int spawnTimer { get; } = 3;        //minimum value is 3. Any less than that and hunters never get a turn.
    public int spawnMod;                        //used by dungeon mod to adjust spawn timer. Value can never be more than 5.
    private int MaxMonsters { get; } = 8;       //doesn't include boss

    //monster states
    public enum MonsterState { Idle, Moving, UseSkill, CheckEnemiesInRange }    //CheckEnemiesInRange is used before moving, in case a hunter is in range to attack.
    public MonsterState aiState;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        monsterObject = new GameObject("Monsters");
        monsterObject.transform.SetParent(transform);
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

    /* NOTE: For some reason, I can't set the monster object's parent, or else the movement messes up. I don't know why this happens. */
    public Monster SpawnMonster(int monsterLevel)
    {
        //int randMonster = Random.Range(0, masterMonsterList.Count);
        //Monster monster = Instantiate(masterMonsterList[randMonster]/*, monsterObject.transform*/);
        //monster.InitializeStats(monsterLevel, monster.monsterData);


        //choose a spawner to spawn from. Monster cannot spawn from a point that's occupied by another character.
        //In the unlikely event that a monster can't spawn anywhere, the game will quit trying to spawn if the fail counter reaches 10.
        //a boss must be spawned no matter what, even if I have to kill whoever's sitting on a spawn point.
        Dungeon dun = Singleton.instance.Dungeon;
        int randSpawner;
        int failCounter = 0;
        do
        {
            randSpawner = Random.Range(0, dun.spawnPoints.Count);
            failCounter++;
        }
        while (dun.spawnPoints[randSpawner].room.character != null && failCounter < 10);

        //generate monster at spawn point
        if (failCounter < 10)
        {
            int randMonster = Random.Range(0, masterMonsterList.Count);
            Monster monster = Instantiate(masterMonsterList[randMonster]);
            monster.InitializeStats(monsterLevel, monster.monsterData);
            dun.UpdateCharacterRoom(monster, dun.spawnPoints[randSpawner].room);
            activeMonsters.Add(monster);
            return monster;
        }
        else
            return null;
    }

    public void MoveMonster(Monster monster)
    {
        StartCoroutine(monster.cpuBehaviour.MoveMonster(monster));
    }

    public void ChangeMonsterState(Monster monster, MonsterState state)
    {
        switch(state)
        {
            case MonsterState.Idle:
                break;

            case MonsterState.Moving:
                monster.MoveMonster();
                break;

            case MonsterState.UseSkill:
                monster.UseSkill();
                break;

            case MonsterState.CheckEnemiesInRange:
                monster.CheckHuntersInRange();
                break;
        }
    }

    public void ToggleMonster(Monster monster, bool toggle)
    {
        monster.gameObject.SetActive(toggle);
    }

    //removes monster from dungeon and sends them to graveyard.
    public void KillMonster(Monster monster)
    {
        graveyard.Add(monster);
        activeMonsters.Remove(monster);

        GameManager gm = Singleton.instance.GameManager;
        //if they are the active character, their turn ends.
        //if (monster == gm.ActiveCharacter())
          //  gm.EndTurn();

        gm.turnOrder.Remove(monster);


        monster.gameObject.SetActive(false);
    }

}
