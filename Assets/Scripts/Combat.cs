using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/* This is the combat system. It takes two character objects and two pairs of dice. Combat lasts for 1 round.
 In cases where one character uses a ranged attack and the other character uses a melee counterattack, there will be a 
gap in the battlefield to indicate the melee character can't counterattack. */
public class Combat : MonoBehaviour
{
    public Dice attackerDice, defenderDice;
    public int attackRollResult, defendRollResult;
    public bool defenderCounterattacking, attackerTurnOver;
    public byte perfectDefenseMod;
    public float chanceToRun;               //affected by attacker and defender SPD

    [Header("---Modifiers---")]
    public float runMod;                    //modifier to run chance.


    [Header("---UI---")]
    public TextMeshProUGUI attackerNameText, defenderNameText;
    public TextMeshProUGUI attackerHealthPoints, defenderHealthPoints, attackerSkillPoints, defenderSkillPoints;
    public TextMeshProUGUI attackerDieOneGUI, attackerDieTwoGUI, attackerAtp_total, attackerTotalAttackDamage;
    public TextMeshProUGUI defenderDieOneGUI, defenderDieTwoGUI, defenderDfp_total, defenderTotalDefense;
    public TextMeshProUGUI damageText;      //displays damage dealt or amount healed.
    private Color damageColor, reducedColor, healColor;              //red = damage, blue = reduced damage, green = heal
    public TextMeshProUGUI statusText;      //used for buffs/debuffs
    public List<TextMeshProUGUI> damageValues;      //used for displaying lots of damage values at a time.

    [Header("---Combat Grid---")]
    public Room roomPrefab;
    private GameObject battlefieldContainer;
    private Room[,] fieldGrid;        //used to layout the battlefield. In a ranged fight, there will be a gap to show that melee counters are ineffective.
    public Room attackerRoom, defenderRoom; //where the combatants are positioned.

    // Start is called before the first frame update
    void Start()
    {
        damageColor = Color.red;
        reducedColor = Color.blue;
        healColor = Color.green;
        damageText.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
        //damageValues.Add(Instantiate(damageText));  //Does this work?

        //populate grid
        battlefieldContainer = new GameObject("Battlefield");
        battlefieldContainer.transform.SetParent(this.transform);

        fieldGrid = new Room[4, 4];
        for (int i = 0; i < fieldGrid.GetLength(0); i++)
        {
            for (int j = 0; j < fieldGrid.GetLength(1); j++)
            {
                Room newRoom = Instantiate(roomPrefab, battlefieldContainer.transform);
                Vector3 roomScale = newRoom.transform.localScale;
                newRoom.transform.position = new Vector3(newRoom.transform.position.x + ((i * 2) * roomScale.x / 2), newRoom.transform.position.y,
                    newRoom.transform.position.z + ((j * 2) * roomScale.z / 2));
                fieldGrid[i, j] = newRoom;
            }
        }
    }

    private void OnEnable()
    {
        damageText.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
    }

  

    /*public void OnRollDiceButton()
    {
        GameManager gm = GameManager.instance;
        StartCombat(gm.hunters[0], defender);
    }*/

    public void StartCombat(Character attacker, Character defender)
    {
        //the attacker always attacks first since they began the combat.
        //after the attacker finishes their turn, the attacker and defender switch roles.
        //the defender counterattacks with a basic attack with reduced power.

        //hide Hunter UI and dungeon layout
        SetNonCombatUI(false);

        //setup UI
        attackerNameText.text = attacker.characterName;
        attackerHealthPoints.text = attacker.healthPoints + "/" + attacker.maxHealthPoints;
        attackerSkillPoints.text = attacker.skillPoints + "/" + attacker.maxSkillPoints;

        defenderNameText.text = defender.characterName;
        defenderHealthPoints.text = defender.healthPoints + "/" + defender.maxHealthPoints;
        defenderSkillPoints.text = defender.skillPoints + "/" + defender.maxSkillPoints;

        perfectDefenseMod = 1;  //default value

        //run chance
        chanceToRun = 1 - (attacker.spd * 0.01f * 2) + (defender.spd * 0.01f) + runMod;
        if (chanceToRun < 0)
            chanceToRun = 0;


        //attacker takes their turn first
        attackerTurnOver = false;
        defenderCounterattacking = false;
        StartCoroutine(SimulateDiceRoll(attackerDice, defenderDice, attacker, defender));
        /*int totalAttackRoll = GetTotalRoll_Attacker(attacker);
        attackerDieOneGUI.text = attackerDice.die1.ToString();
        attackerDieTwoGUI.text = attackerDice.die2.ToString();
        attackerAtp_total.text = "ATP\n+" + attacker.atp;
        attackerTotalAttackDamage.text = totalAttackRoll.ToString();

        //defender
        int totalDefendRoll = GetTotalRoll_Defender(defender);
        defenderDieOneGUI.text = defenderDice.die1.ToString();
        defenderDieTwoGUI.text = defenderDice.die2.ToString();
        defenderDfp_total.text = "DFP\n+" + defender.dfp;
        defenderTotalDefense.text = totalDefendRoll.ToString();*/

        //calculate damage, starting with attacker dealing damage to defender.
        //use coroutine to 
    }

    private int GetTotalRoll_Attacker(Character character)
    {

        int diceResult = attackerDice.RollDice();

        if (character.TryGetComponent<Hunter>(out Hunter hunter))
        {
            return diceResult + (int)hunter.atp;
        }
        else if (character.TryGetComponent<Monster>(out Monster monster))
        {
            return diceResult + (int)monster.atp;
        }
        else
            return 0;
    }

    private int GetTotalRoll_Defender(Character character)
    {
        //roll 2 dice if character is defending, i.e. they forfeit their chance to counterattack.
        int dieResult = character.characterState == Character.CharacterState.Guarding ? defenderDice.RollDice() : defenderDice.RollSingleDie();


        if (character.TryGetComponent<Hunter>(out Hunter hunter))
        {
            return dieResult + (int)hunter.dfp;
        }
        else if (character.TryGetComponent<Monster>(out Monster monster))
        {
            return dieResult + (int)monster.dfp;
        }
        else
            return 0;
    }

    /* toggles non-releavant UI on or off */
    private void SetNonCombatUI(bool toggle)
    {
        HunterManager hm = Singleton.instance.HunterManager;
        Dungeon dun = Singleton.instance.Dungeon;

        if (toggle == true)
        {
            hm.ui.gameObject.SetActive(true);
            dun.gameObject.SetActive(true);
        }
        else
        {
            hm.ui.gameObject.SetActive(false);
            dun.gameObject.SetActive(false);
        }
    }

    #region Coroutines

    /* State is used to determine how the defender's rolls are simulated. */
    private IEnumerator SimulateDiceRoll(Dice attackerDice, Dice defenderDice, Character attacker, Character defender)
    {
        if (!attackerTurnOver && !defenderCounterattacking)
        {
            //show random values for a duration then get the rolled values
            float rollDuration = 0.4f;
            float currentTime = Time.time;
            attackerTotalAttackDamage.text = "";
            defenderTotalDefense.text = "";
            //int attackRollResult = 0;
            //int defendRollResult = 0;
            while (Time.time < currentTime + rollDuration)
            {
                attackRollResult = attackerDice.RollDice();
                attackerDieOneGUI.text = attackerDice.die1.ToString();
                attackerDieTwoGUI.text = attackerDice.die2.ToString();

                //Debug.Log("state: " + state);
                //state = Character.CharacterState.Guarding;
                //defendRollResult = defender.characterState == Character.CharacterState.Guarding ? defenderDice.RollDice() : defenderDice.RollSingleDie();

                //roll dice for defender. check the defender's roll for perfect defense
                if (defender.characterState == Character.CharacterState.Guarding)
                {
                    defenderCounterattacking = false;
                    defendRollResult = defenderDice.RollDice();
                    if (defenderDice.die1 + defenderDice.die2 >= 12)
                    {
                        //rolled a natural 12, all damage is blocked
                        perfectDefenseMod = 0;
                    }
                    else
                    {
                        perfectDefenseMod = 1;
                    }
                }
                else
                {
                    defendRollResult = defenderDice.RollSingleDie();
                    defenderCounterattacking = true;
                    perfectDefenseMod = 1;
                }

                defenderDieOneGUI.text = defenderDice.die1.ToString();
                defenderDieTwoGUI.text = defenderDice.die2.ToString();

                yield return new WaitForSeconds(0.016f);    // 1/60 seconds
            }

            //display the final result
            attackerAtp_total.text = "ATP\n+" + attacker.atp;
            attackRollResult += Mathf.RoundToInt(attacker.atp * attacker.atpMod);
            attackerTotalAttackDamage.text = attackRollResult.ToString();

            defenderDfp_total.text = "DFP\n+" + defender.dfp;
            defendRollResult += Mathf.RoundToInt(defender.dfp * defender.dfpMod);
            defenderTotalDefense.text = defendRollResult.ToString();

            //attackerTurnOver = true;
            yield return ResolveDamage(attacker, defender);
        }
        else  //defender attacks their attacker
        {
            //show random values for a duration then get the rolled values
            perfectDefenseMod = 1;          //attacker cannot get perfect defense
            float rollDuration = 0.4f;
            float currentTime = Time.time;
            attackerTotalAttackDamage.text = "";
            defenderTotalDefense.text = "";
            while (Time.time < currentTime + rollDuration)
            {
                attackRollResult = defenderDice.RollDice();
                defenderDieOneGUI.text = defenderDice.die1.ToString();
                defenderDieTwoGUI.text = defenderDice.die2.ToString();

                //Debug.Log("state: " + state);
                //state = Character.CharacterState.Guarding;
                defendRollResult = attackerDice.RollSingleDie();
                attackerDieOneGUI.text = attackerDice.die1.ToString();
                attackerDieTwoGUI.text = attackerDice.die2.ToString();

                yield return new WaitForSeconds(0.016f);    // 1/60 seconds
            }

            //display the final result
            defenderDfp_total.text = "ATP\n+" + defender.atp;   //ATP is shown here since defender is always on the right side
            attackRollResult += (int)defender.atp;
            defenderTotalDefense.text = attackRollResult.ToString();

            attackerAtp_total.text = "DFP\n+" + attacker.dfp;
            defendRollResult += (int)attacker.dfp;
            attackerTotalAttackDamage.text = defendRollResult.ToString();

            defenderCounterattacking = false;
            yield return ResolveDamage(defender, attacker);
        }
    }

    private IEnumerator ResolveDamage(Character attacker, Character defender)
    {
        int damage = (attackRollResult - defendRollResult) * perfectDefenseMod;
        if (damage < 0)
            damage = 0;

        defender.healthPoints -= damage;
        if (defender.healthPoints < 0)
            defender.healthPoints = 0;

        damageText.color = damageColor;
        damageText.text = damage.ToString();
        damageText.gameObject.SetActive(true);

        //update HP
        GameManager gm = Singleton.instance.GameManager;

        if (!attackerTurnOver)
            defenderHealthPoints.text = defender.healthPoints + "/" + defender.maxHealthPoints;
        else
            //updating attacker's HP since defender is counterattacking. defender is still referenced because the attacker is currently defending
            attackerHealthPoints.text = defender.healthPoints + "/" + defender.maxHealthPoints;

        //animate damage
        float duration = 1;
        float currentTime = Time.time;
        Vector3 textPos = damageText.transform.position;
        Vector3 origPos = textPos;

        while (Time.time < currentTime + duration)
        {
            textPos = new Vector3(textPos.x, textPos.y + 50 * Time.deltaTime, textPos.z);
            damageText.transform.position = textPos;
            yield return null;
        }

        damageText.transform.position = origPos;
        damageText.gameObject.SetActive(false);

        //check if defender counterattacks, otherwise combat is over.
        
        attackerTurnOver = true;
        if (attackerTurnOver && defenderCounterattacking)
        {
            yield return new WaitForSeconds(1);
            yield return SimulateDiceRoll(attackerDice, defenderDice, attacker, defender);
        }

        //if we get here, combat has ended.
    }
    #endregion
}
