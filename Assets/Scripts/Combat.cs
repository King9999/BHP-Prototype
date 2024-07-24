using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/* This is the combat system. It takes two character objects and two pairs of dice. */
public class Combat : MonoBehaviour
{
    public Dice attackerDice, defenderDice;
    public int attackRollResult, defendRollResult;
    public bool defenderCounterattacking;

    [Header("---UI---")]
    public TextMeshProUGUI attackerNameText, defenderNameText;
    public TextMeshProUGUI attackerHealthPoints, defenderHealthPoints, attackerSkillPoints, defenderSkillPoints;
    public TextMeshProUGUI attackerDieOneGUI, attackerDieTwoGUI, attackerAtp_total, attackerTotalAttackDamage;
    public TextMeshProUGUI defenderDieOneGUI, defenderDieTwoGUI, defenderDfp_total, defenderTotalDefense;
    public TextMeshProUGUI damageText;      //displays damage dealt or amount healed.
    private Color damageColor, reducedColor, healColor;              //red = damage, blue = reduced damage, green = heal
    public TextMeshProUGUI statusText;      //used for buffs/debuffs

    // Start is called before the first frame update
    void Start()
    {
        damageColor = Color.red;
        reducedColor = Color.blue;
        healColor = Color.green;
        damageText.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        damageText.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
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

        //setup UI
        attackerNameText.text = attacker.characterName;
        attackerHealthPoints.text = attacker.healthPoints + "/" + attacker.maxHealthPoints;
        attackerSkillPoints.text = attacker.skillPoints + "/" + attacker.maxSkillPoints;

        defenderNameText.text = defender.characterName;
        defenderHealthPoints.text = defender.healthPoints + "/" + defender.maxHealthPoints;
        defenderSkillPoints.text = defender.skillPoints + "/" + defender.maxSkillPoints;

        //attacker
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

    /* State is used to determine how the defender's rolls are simulated. */
    private IEnumerator SimulateDiceRoll(Dice attackerDice, Dice defenderDice, Character attacker, Character defender)
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
            defendRollResult = defender.characterState == Character.CharacterState.Guarding ? defenderDice.RollDice() : defenderDice.RollSingleDie();
            defenderDieOneGUI.text = defenderDice.die1.ToString();
            defenderDieTwoGUI.text = defenderDice.die2.ToString();

            yield return new WaitForSeconds(0.016f);    // 1/60 seconds
        }

        //display the final result
        attackerAtp_total.text = "ATP\n+" + attacker.atp;
        attackRollResult += (int)attacker.atp;
        attackerTotalAttackDamage.text = attackRollResult.ToString();

        defenderDfp_total.text = "DFP\n+" + defender.dfp;
        defendRollResult += (int)defender.dfp;
        defenderTotalDefense.text = defendRollResult.ToString();

        yield return ResolveDamage(attacker, defender);
    }

    private IEnumerator ResolveDamage(Character attacker, Character defender)
    {
        int damage = attackRollResult - defendRollResult;
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
        defenderHealthPoints.text = defender.healthPoints + "/" + defender.maxHealthPoints;

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

        //if (defender.characterState != Character.CharacterState.Guarding)
            //yield return SimulateDiceRoll()
    }
}
