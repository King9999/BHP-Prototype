using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static HunterManager;

/* This is the combat system. It takes two character objects and two pairs of dice. Combat lasts for 1 round.
 In cases where one character uses a ranged attack and the other character uses a melee counterattack, there will be a 
gap in the battlefield to indicate the melee character can't counterattack. */
public class Combat : MonoBehaviour
{
    [SerializeField] private Dice attackerDice, defenderDice;
    [SerializeField] private int attackRollResult, defendRollResult;
    [SerializeField] private bool defenderCounterattacking, attackerTurnOver;
    public byte perfectDefenseMod;
    public float chanceToRun;               //affected by attacker and defender SPD

    [Header("---Modifiers---")]
    public float runMod, runPreventionMod;   //modifier to run chance. runPreventionMod is used by attacker to reduce chance to escape.


    [Header("---UI---")]
    [SerializeField] private TextMeshProUGUI attackerNameText, defenderNameText;
    [SerializeField] private TextMeshProUGUI attackerHealthPoints, defenderHealthPoints, attackerSkillPoints, defenderSkillPoints;
    [SerializeField] private TextMeshProUGUI attackerDieOneGUI, attackerDieTwoGUI, attackerAtp_total, attackerTotalAttackDamage;
    [SerializeField] private TextMeshProUGUI defenderDieOneGUI, defenderDieTwoGUI, defenderDfp_total, defenderTotalDefense;
    [SerializeField] private TextMeshProUGUI damageText;      //displays damage dealt or amount healed.
    [SerializeField] private TextMeshProUGUI runChanceText;
    private Color damageColor, reducedColor, healColor;              //red = damage, blue = reduced damage, green = heal
    [SerializeField] private TextMeshProUGUI statusText;      //used for buffs/debuffs
    [SerializeField] private List<TextMeshProUGUI> damageValues;      //used for displaying lots of damage values at a time.
    //[SerializeField] private GameObject cardMenu;
    [SerializeField] private GameObject defenderMenu;
    [SerializeField] private List<CardObject> hunterCards;      //used by both attacker and defender
    [SerializeField] private CardMenu cardMenu;

    [Header("---Combat Grid---")]
    [SerializeField] private Room roomPrefab;
    private GameObject battlefieldContainer;
    private Room[,] fieldGrid;        //used to layout the battlefield. In a ranged fight, there will be a gap to show that melee counters are ineffective.
    [SerializeField] private Room attackerRoom, defenderRoom; //where the combatants are positioned.
    private bool attackersTurn;         //used to keep track of who's taking their turn

    //combat states
    private enum CombatState { AttackerTurn, DefenderTurn, BeginCombat}
    [Header("---Combat State---")]
    [SerializeField] private CombatState combatState;
    private Coroutine combatCoroutine;
    

    // Start is called before the first frame update
    /*void Start()
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

        fieldGrid = new Room[4, 5];
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

        //get the rooms for combatants. They should be the middle column.
        attackerRoom = fieldGrid[0, 2];
        defenderRoom = fieldGrid[3, 2];
    }*/

    private void OnEnable()
    {
        damageText.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
        runChanceText.gameObject.SetActive(false);
    }

    public void InitSetup()
    {
        damageColor = Color.red;
        reducedColor = Color.blue;
        healColor = Color.green;
        damageText.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
        runChanceText.gameObject.SetActive(false);
        cardMenu.ShowMenu(false);
        //ShowCardMenu(false);
        ShowDefenderMenu(false);
        //damageValues.Add(Instantiate(damageText));  //Does this work?

        //populate grid
        battlefieldContainer = new GameObject("Battlefield");
        battlefieldContainer.transform.SetParent(this.transform);

        fieldGrid = new Room[4, 5];
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

        //get the rooms for combatants. They should be the middle column.
        attackerRoom = fieldGrid[0, 2];
        defenderRoom = fieldGrid[3, 2];
    }

    private void ShowCardMenu(bool toggle, Character character = null)
    {
        cardMenu.gameObject.SetActive(toggle);
        if (toggle == true)
        {
            Vector3 menuPos = new Vector3(character.transform.position.x, character.transform.position.y - 6, character.transform.position.z);
            cardMenu.transform.position = Camera.main.WorldToScreenPoint(menuPos);

            //get hunter cards
            if (character is Hunter hunter)
            {
                for (int i = 0; i < hunter.cards.Count; i++)
                {
                    hunterCards[i].ShowCard(true);
                    hunterCards[i].cardData = hunter.cards[i];
                    if (hunter.cards[i].cardType == Card.CardType.Combat || hunter.cards[i].cardType == Card.CardType.Versatile)
                    { 
                        hunterCards[i].UpdateCardSprite(hunterCards[i].cardSprite);
                        hunterCards[i].cardInvalid = false;
                    }
                    else
                    {
                        //can't use this card
                        hunterCards[i].cardInvalid = true;
                        hunterCards[i].UpdateCardSprite(hunterCards[i].invalidCardSprite);
                    }

                }

                //sort cards NOTE: will this help with sorting the cards consistently?
                /*var sorted = from card in hunterCards
                             orderby card descending
                             select card;
                foreach (var card in sorted)
                {
                    
                }*/
                hunterCards = hunterCards.OrderByDescending(x => !x.cardInvalid).ToList();  //sorting isn't consistent for some reason
            }
        }
        else
        {
            foreach (CardObject card in hunterCards)
            {
                card.ShowCard(false);
            }
        }
    }

    private void ShowDefenderMenu(bool toggle, Character character = null)
    {
        defenderMenu.gameObject.SetActive(toggle);
        if (toggle == true)
        {
            Vector3 menuPos = new Vector3(character.transform.position.x, character.transform.position.y + 6, character.transform.position.z);
            defenderMenu.transform.position = Camera.main.WorldToScreenPoint(menuPos);
        }
    }

    private void ChangeCombatState(CombatState state)
    {
        switch (state)
        {
            case CombatState.AttackerTurn:
                cardMenu.ShowMenu(true, attackerRoom.character, Card.CardType.Combat);
                //ShowCardMenu(true, attackerRoom.character);
                break;

            case CombatState.DefenderTurn:
                cardMenu.ShowMenu(false);
                //ShowCardMenu(false);
                ShowDefenderMenu(true, defenderRoom.character);
                break;

            case CombatState.BeginCombat:
                //run coroutine here to handle combat resolution, animations, etc.
                break;
        }
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

        //position the combatants
        SetCharacterPosition(attacker, attackerRoom);
        SetCharacterPosition(defender, defenderRoom);
        attacker.isAttacker = true;
        defender.isDefender = true;
        attackersTurn = true;

        //position the camera so that it's centered on the battlefield.
        GameManager gm = Singleton.instance.GameManager;
        //Vector3 newPos = fieldGrid[2, 2].transform.position;
        gm.gameCamera.transform.position = new Vector3(-1, 5, -2);  //not sure if the camera will remain consistent with these values.

        //setup UI
        attackerNameText.text = attacker.characterName;
        attackerHealthPoints.text = attacker.healthPoints + "/" + attacker.maxHealthPoints;
        attackerSkillPoints.text = attacker.skillPoints + "/" + attacker.maxSkillPoints;

        defenderNameText.text = defender.characterName;
        defenderHealthPoints.text = defender.healthPoints + "/" + defender.maxHealthPoints;
        defenderSkillPoints.text = defender.skillPoints + "/" + defender.maxSkillPoints;

        perfectDefenseMod = 1;  //default value
        runPreventionMod = 0;
        runMod = 0;

        //run chance. This is displayed when the defender hovers over the run button.
        /*chanceToRun = 1 - (attacker.spd * 0.01f * 2 - runPreventionMod) + (defender.spd * 0.01f + runMod);
        if (chanceToRun < 0)
            chanceToRun = 0;*/

        UpdateRunChance(attacker, defender, runPreventionMod, runMod);

        //attacker takes their turn first. Attack would've already been chosen, so all attacker does is pick a card.
        attackerTurnOver = false;
        defenderCounterattacking = false;

        //attacker goes first, display card menu
        ChangeCombatState(combatState = CombatState.AttackerTurn);
        
        //StartCoroutine(SimulateDiceRoll(attackerDice, defenderDice, attacker, defender));
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

    public void UpdateRunChance(Character attacker, Character defender, float runPreventionMod, float runMod)
    {
        chanceToRun = 1 - (attacker.spd * 0.01f * 2 + runPreventionMod) + (defender.spd * 0.01f + runMod);
        if (chanceToRun < 0)
            chanceToRun = 0;
    }

    //sets character's position to a room on the battlefield.
    private void SetCharacterPosition(Character character, Room room)
    {
        room.character = character;
        character.transform.position = new Vector3(room.transform.position.x, character.transform.position.y, room.transform.position.z);
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

    public void OnSelectCardButtonPressed()
    {
        //cannot proceed if no card was selected.
        CardManager cm = Singleton.instance.CardManager;
        if (cm.selectedCard == null)
            return;

        if (combatState == CombatState.AttackerTurn)
            ChangeCombatState(combatState = CombatState.DefenderTurn);
        else
            ChangeCombatState(combatState = CombatState.BeginCombat);
    }

    public void OnSkipCardButtonPressed()
    {
        CardManager cm = Singleton.instance.CardManager;
        cm.selectedCard = null;

        if (combatState == CombatState.AttackerTurn)
            ChangeCombatState(combatState = CombatState.DefenderTurn);
        else
            ChangeCombatState(combatState = CombatState.BeginCombat);
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
