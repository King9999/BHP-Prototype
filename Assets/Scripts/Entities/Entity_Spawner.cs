using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* The Spawner spawns enemies after a certain number of turns have passed. The spawn timer is controlled by Monster Manager.
 */
public class Entity_Spawner : Entity
{
    [SerializeField] private Sprite spawnerSprite;
   

    public void SpawnMonster(Monster monster)
    {
        MonsterManager mm = Singleton.instance.MonsterManager;

        int randMonster = Random.Range(0, mm.masterMonsterList.Count);

        monster.transform.position = transform.position;
    }
}
