using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

//Base AI behaviours used mainly by monsters. Base method for moving and attacking is stored here. This can be used
//for when a monster doesn't have any special behaviours.
[CreateAssetMenu(menuName = "Monster AI/Basic", fileName = "monsterAI_basic")]
public class Monster_AI : ScriptableObject
{
    //choose a random skill. Used by CPU Hunters.
    ActiveSkill GetSkill(Monster user, Hunter target)
    {
        ActiveSkill skill = null;
        bool skillFound = false;
        List<ActiveSkill> activeSkills = new List<ActiveSkill>();

        //collect all of the active skills
        foreach (Skill skl in user.skills)
        {
            if (skl is ActiveSkill actSkl)
            {
                activeSkills.Add(actSkl);
            }
        }

        while (!skillFound)
        {
            int randSkill = Random.Range(0, activeSkills.Count);

            if (activeSkills[randSkill].skillCost <= user.skillPoints ||
                (activeSkills[randSkill].requiresCharges && activeSkills[randSkill].skillCharges > 0))
            {
                skill = activeSkills[randSkill];
                skillFound = true;
            }
        }


        return skill;
    }

    public virtual IEnumerator MoveMonster(Monster monster)
    {

        GameManager gm = Singleton.instance.GameManager;

        yield return new WaitForSeconds(1);

        //roll dice; show die UI and roll die after a second.
        gm.dice.ShowSingleDieUI(true);
        yield return new WaitForSeconds(1);

        //show move tiles. This code is identical to the code in the Update method of GameManager.
        int totalMove = Mathf.RoundToInt((monster.mov + gm.dice.RollSingleDie(Dice.DiceType.Move) + gm.movementMod) * monster.movMod);
        List<Room> moveRange = gm.ShowMoveRange(monster, totalMove);
        Debug.LogFormat("Total Move for {0}: {1}", monster.characterName, totalMove);

        //keep a list of characters and entities in range
        List<Hunter> huntersInRange = new List<Hunter>();
        List<Entity> entitiesInRange = new List<Entity>();

        foreach (Room pos in moveRange)
        {
            //if there are existing move tile objects, activate those first before instantiating new ones.
            if (gm.moveTileBin.Count > 0)
            {
                GameObject lastTile = gm.moveTileBin[0];
                lastTile.SetActive(true);
                lastTile.transform.position = new Vector3(pos.transform.position.x, 0.6f, pos.transform.position.z);
                gm.moveTileList.Add(lastTile);
                gm.moveTileBin.Remove(lastTile);
            }
            else
            {
                GameObject tile = Instantiate(gm.moveTilePrefab, gm.moveTileContainer.transform);
                tile.transform.position = new Vector3(pos.transform.position.x, 0.6f, pos.transform.position.z);
                gm.moveTileList.Add(tile);
            }

            //check each tile and look for entities or characters.
            if (pos.character != null && pos.character is Hunter hunter)
            {
                huntersInRange.Add(hunter);
            }

            if (pos.entity != null && !pos.entity.playerInteracted && pos.entity is not Entity_Spawner)
            {
                entitiesInRange.Add(pos.entity);
            }

        }

        if (gm.moveTileBin.Count <= 0)
        {
            gm.moveTileBin.TrimExcess();
        }

        Debug.LogFormat("Hunters in range: {0}", huntersInRange.Count);
        Debug.LogFormat("Entities in range: {0}", entitiesInRange.Count);

        /*******find a character to attack. *******/
        if (huntersInRange.Count > 0)
        {
            int randChar = Random.Range(0, huntersInRange.Count);
            monster.targetChar = huntersInRange[randChar];
        }

        //monster.targetChar = null;
        /******check for entities******/
        Entity targetEntity = null;
        if (entitiesInRange.Count > 0)
        {
            //pick the closest entity
            float shortestDistance = 0;
            for (int i = 0; i < entitiesInRange.Count; i++)
            {

                if (i <= 0 || shortestDistance <= 0)
                {
                    shortestDistance = Vector3.Distance(monster.transform.position, entitiesInRange[i].transform.position);
                    targetEntity = entitiesInRange[i];
                    continue;
                }

                float distance = Vector3.Distance(monster.transform.position, entitiesInRange[i].transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    targetEntity = entitiesInRange[i];
                }
            }

        }

        yield return new WaitForSeconds(1);

        //clear move tiles and start moving character
        for (int j = 0; j < gm.moveTileList.Count; j++)
        {
            gm.moveTileList[j].SetActive(false);
            gm.moveTileBin.Add(gm.moveTileList[j]);
        }
        gm.moveTileList.Clear();
        gm.moveTileList.TrimExcess();

        if (monster.targetChar == null && targetEntity == null)
        {
            //nothing of interest, move to a random spot. Move the full distance.
            //TODO: may make it so that CPU looks for an out of range target and move towards it.
            int randSpace = Random.Range(0, moveRange.Count);
            gm.MoveCPUCharacter(monster, moveRange[randSpace].transform.position);
        }
        else
        {
            //choose between moving towards an entity or towards a character. Monsters prefer targeting hunters over entities.
            if (monster.targetChar != null && targetEntity != null)
            {
                float roll = Random.value;
                Debug.LogFormat("{0} is choosing what to target. Rolled {1}", monster.characterName, roll);

                if (roll <= 0.25f)
                {
                    //target entity
                    Vector3 newPos = new Vector3(targetEntity.transform.position.x, 0, targetEntity.transform.position.z);
                    monster.targetChar = null;       //not targeting a hunter so must clear.
                    gm.MoveCPUCharacter(monster, newPos);
                    Debug.LogFormat("{0} moving to entity {1}", monster.characterName, targetEntity.name);
                }
                else
                {
                    //target hunter.
                    targetEntity = null;
                    monster.chosenSkill = GetSkill(monster, monster.targetChar as Hunter);
                    Vector3 newPos = new Vector3(monster.targetChar.transform.position.x, 0, monster.targetChar.transform.position.z);
                    gm.MoveCPUCharacter(monster, newPos, true);
                    Debug.LogFormat("{0} moving towards hunter {1}", monster.characterName, monster.targetChar.characterName);
                }
            }
            else if (monster.targetChar == null && targetEntity != null)
            {
                Vector3 newPos = new Vector3(targetEntity.transform.position.x, 0, targetEntity.transform.position.z);
                gm.MoveCPUCharacter(monster, newPos);
                Debug.LogFormat("{0} moving to entity", monster.characterName);
            }
            else if (monster.targetChar != null && targetEntity == null)
            {
                monster.chosenSkill = GetSkill(monster, monster.targetChar as Hunter);
                Vector3 newPos = new Vector3(monster.targetChar.transform.position.x, 0, monster.targetChar.transform.position.z);
                gm.MoveCPUCharacter(monster, newPos, true);
                Debug.LogFormat("{0} moving towards hunter {1}", monster.characterName, monster.targetChar.characterName);
            }

        }

    }

    public virtual IEnumerator UseSkill(Monster monster)
    {
        //display the skill's range
        GameManager gm = Singleton.instance.GameManager;
        List<Room> skillRange = gm.ShowSkillRange(monster, monster.chosenSkill.minRange, monster.chosenSkill.maxRange);
        gm.DisplaySkillTiles(skillRange);
        yield return new WaitForSeconds(1);

        for (int i = 0; i < gm.skillTileList.Count; i++)
        {
            gm.skillTileList[i].SetActive(false);
            gm.skillTileBin.Add(gm.skillTileList[i]);
        }
        gm.skillTileList.Clear();
        gm.skillTileList.TrimExcess();

        //Start combat
        gm.ChangeGameState(gm.gameState = GameManager.GameState.Combat);
    }

    /*public virtual IEnumerator CheckHuntersInRange(Monster monster)
    {
        //Before moving, check if the hunter is already in attack range by checking all applicable skills.
        //Pick basic attack for checking, will choose another skill later
        List<Room> skillRange = new List<Room>();
        List<Character> targetChars = new List<Character>();
        List<ActiveSkill> activeSkills = new List<ActiveSkill>();
        GameManager gm = Singleton.instance.GameManager;
        for (int i = 0; i < monster.skills.Count; i++)
        {
            if (monster.skills[i] is ActiveSkill activeSkill && activeSkill.skillCost <= monster.skillPoints)
            {
                skillRange = gm.ShowSkillRange(monster, activeSkill.minRange, activeSkill.maxRange);
                targetChars = monster.CPU_CheckCharactersInRange(skillRange);
                if (targetChars.Count > 0)
                {
                    activeSkills.Add(activeSkill);
                }
            }

        }

        //pick a random skill
        MonsterManager mm = Singleton.instance.MonsterManager;

        if (activeSkills.Count > 0)
        {
            int randSkill = Random.Range(0, activeSkills.Count);
            int randTarget = Random.Range(0, targetChars.Count);
            monster.chosenSkill = activeSkills[randSkill];
            monster.targetChar = targetChars[randTarget];
            mm.ChangeMonsterState(monster, mm.aiState = MonsterManager.MonsterState.UseSkill);
        }
        else
        {
            mm.ChangeMonsterState(monster, mm.aiState = MonsterManager.MonsterState.Moving);
        }
    }*/
}
