using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/* This is the combat system. It takes two character objects and two pairs of dice. */
public class Combat : MonoBehaviour
{
    public Dice attackerDice, defenderDice;

    [Header("---UI---")]
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

        //attacker
        StartCoroutine(SimulateDiceRoll(attackerDice, defenderDice));
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
        int dieResult = character.characterState == Character.CharacterState.Defending ? defenderDice.RollDice() : defenderDice.RollSingleDie();


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

    private IEnumerator SimulateDiceRoll(Dice attackerDice, Dice defenderDice)
    {
        //show random values for a duration then get the rolled values
        float rollDuration = 0.3f;
        float currentTime = Time.time;
        int attackRollResult = 0;
        while (Time.time < currentTime + rollDuration)
        {
            attackRollResult = attackerDice.RollDice();
            attackerDieOneGUI.text = attackerDice.die1.ToString();
            attackerDieTwoGUI.text = attackerDice.die2.ToString();
            //currentTime += Time.deltaTime;
            yield return new WaitForSeconds(0.016f);    // 1/60 seconds
        }

        Debug.Log("Rolled " + attackRollResult);
        //display the final result
        /*int totalAttackRoll = GetTotalRoll_Attacker(attacker);
        attackerDieOneGUI.text = attackerDice.die1.ToString();
        attackerDieTwoGUI.text = attackerDice.die2.ToString();*/
    }
}
