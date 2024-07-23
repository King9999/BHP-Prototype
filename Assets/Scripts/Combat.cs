using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* This is the combat system. It takes two character objects and two pairs of dice. */
public class Combat : MonoBehaviour
{
    public Dice dice;

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

    public void StartCombat(Character attacker, Character defender)
    {
        //the attacker always attacks first since they began the combat.
        //after the attacker finishes their turn, the attacker and defender switch roles.
        //the defender counterattacks with a basic attack with reduced power.

        //attacker
        int totalAttackRoll = GetTotalRoll_Attacker(attacker);
        attackerDieOneGUI.text = dice.die1.ToString();
        attackerDieTwoGUI.text = dice.die2.ToString();
        attackerAtp_total.text = "ATP\n+" + attacker.atp;
        attackerTotalAttackDamage.text = totalAttackRoll.ToString();

        //defender
        int totalDefendRoll = GetTotalRoll_Defender(defender);
        defenderDieOneGUI.text = dice.die1.ToString();
        defenderDieTwoGUI.text = dice.die2.ToString();
        defenderDfp_total.text = "DFP\n+" + defender.dfp;
        defenderTotalDefense.text = totalDefendRoll.ToString();
    }

    private int GetTotalRoll_Attacker(Character character)
    {

        int diceResult = dice.RollDice();

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
        int dieResult = character.characterState == Character.CharacterState.Defending ? dice.RollDice() : dice.RollSingleDie();


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
}
