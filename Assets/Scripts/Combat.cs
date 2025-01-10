using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Compilation;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

/* This is the combat system. It takes two character objects and two pairs of dice. Combat lasts for 1 round.
 In cases where one character uses a ranged attack and the other character uses a melee counterattack, there will be a 
gap in the battlefield to indicate the melee character can't counterattack. */
public class Combat : MonoBehaviour
{
    [SerializeField] private Dice attackerDice, defenderDice;
    [SerializeField] private int attackRollResult, defendRollResult;
    [SerializeField] private bool defenderCounterattacking, attackerTurnOver;
    [SerializeField] private Camera combatCamera;
    Vector3 defaultCameraPos { get; } = new Vector3(0, 5, -10);

    [Header("---Modifiers---")]
    public float runMod, runPreventionMod;   //modifier to run chance. runPreventionMod is used by attacker to reduce chance to escape.
    public int perfectDefenseMod, criticalDamageMod;          //defense mod is 0 when defender rolls a 12
    public float runChance;                //affected by attacker and defender SPD
    public float counterAttackMod;
    public float defenseBoost;              //used by defender when guarding.
    public bool hasAugmenter;       //when true, apply debuff instead of crit damage.



    [Header("---UI---")]
    [SerializeField] private TextMeshProUGUI attackerNameText, defenderNameText;
    [SerializeField] private TextMeshProUGUI attackerHealthPoints, defenderHealthPoints, attackerSkillPoints, defenderSkillPoints;
    [SerializeField] private TextMeshProUGUI attackerDieOneGUI, attackerDieTwoGUI, attackerAtp_total, attackerTotalAttackDamage;
    [SerializeField] private TextMeshProUGUI defenderDieOneGUI, defenderDieTwoGUI, defenderDfp_total, defenderTotalDefense;
    [SerializeField] private TextMeshProUGUI damageText;      //displays damage dealt or amount healed.
    [SerializeField] private GameObject tooltipUI;              //context sensitive UI for defender menu buttons
    [SerializeField] private TextMeshProUGUI tooltipText;
    private Color damageColor, reducedColor, healColor;              //red = damage, blue = reduced damage, green = heal
    [SerializeField] private TextMeshProUGUI statusText;      //used for buffs/debuffs
    [SerializeField] private List<TextMeshProUGUI> damageValues;      //used for displaying lots of damage values at a time.
    //[SerializeField] private GameObject cardMenu;
    [SerializeField] private GameObject defenderMenu;
    [SerializeField] private List<CardObject> hunterCards;      //used by both attacker and defender
    [SerializeField] private CardMenu cardMenu;
    [SerializeField] private TextMeshProUGUI activeCard_attackerText, activeCard_defenderText;
    [SerializeField] private Inventory inventory;               //used by defender to surrender an item.
    [SerializeField] private GameObject skillNameUI;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private Image skillNameBackground;
    [SerializeField] private TextMeshProUGUI droppedItemText;   //UI for when an item drops from monster
    [SerializeField] private GameObject droppedItemUI;


    [Header("---Combat Grid---")]
    [SerializeField] private Room roomPrefab;
    private GameObject battlefieldContainer, damageValueContainer;
    private Room[,] fieldGrid;        //used to layout the battlefield. In a ranged fight, there will be a gap to show that melee counters are ineffective.
    [SerializeField] private Room attackerRoom, defenderRoom; //where the combatants are positioned.
    private bool attackersTurn, defendersTurn;         //used to keep track of who's taking their turn
    public Card attackersCard, defendersCard;   //used by CardObject to get reference to chosen cards        

    //combat states
    public enum CombatState { AttackerTurn, DefenderTurn, DefenderChooseCard, BeginCombat, Surrendering, RunAway, WinnerTakesItemFromLoser,
        WinnerTooManyItems }
    [Header("---Combat State---")]
    public CombatState combatState;
    private Coroutine combatCoroutine;
    bool CharacterDefeated { get; set; }                //if true, then combat doesn't end normally until a condition is met.
    public Item ItemTaken { get; set; }                  //reference to the item the winner takes from loser.


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

    private void Start()
    {
        Singleton.instance.Combat = this;
        InitSetup();
        StartCombat(Singleton.instance.attacker, Singleton.instance.defender);
    }

    private void OnEnable()
    {
        damageText.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
        EnableTooltipUI(false);
        activeCard_attackerText.text = "";
        activeCard_defenderText.text = "";
        inventory.ShowInventory(false);
        skillNameUI.SetActive(false);
        droppedItemUI.SetActive(false);

        //set up card menu buttons.
        CardMenu cm = Singleton.instance.CardMenu;
        //cm.backButton.onClick.AddListener();
        cm.selectCardButton.onClick.AddListener(OnSelectCardButtonPressed);
        cm.skipButton.onClick.AddListener(OnSkipCardButtonPressed);
    }


    public void InitSetup()
    {
        damageColor = Color.red;
        reducedColor = Color.blue;
        healColor = Color.green;
        //damageText.gameObject.SetActive(false);
        //statusText.gameObject.SetActive(false);
        //runChanceText.gameObject.SetActive(false);
        //cardMenu.ShowMenu(false);
        //inventory.ShowInventory(false);
        //ShowCardMenu(false);
        ShowDefenderMenu(false);

        //set up damage values. There should be at least 1.
        damageValueContainer = new GameObject("Dmg Values");
        damageValueContainer.transform.SetParent(GetComponentInChildren<Canvas>().transform);    //gets combat UI canvas
        damageValues.Add(Instantiate(damageText, damageValueContainer.transform));
        damageValues[0].gameObject.SetActive(false);

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
                newRoom.transform.position = new Vector3(newRoom.transform.position.x + (i * 2 * roomScale.x / 2), newRoom.transform.position.y,
                    newRoom.transform.position.z + (j * 2 * roomScale.z / 2));
                fieldGrid[i, j] = newRoom;
            }
        }

        //get the rooms for combatants. They should be the middle column.
        attackerRoom = fieldGrid[0, 2];
        defenderRoom = fieldGrid[3, 2];

        combatCamera.transform.position = defaultCameraPos;

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
        defenderMenu.SetActive(toggle);
        if (toggle == true)
        {
            Vector3 menuPos = new Vector3(character.transform.position.x + 11, character.transform.position.y - 4f, character.transform.position.z);
            defenderMenu.transform.position = Camera.main.WorldToScreenPoint(menuPos);
        }
    }

    private void ChangeCombatState(CombatState state)
    {
        Character attacker = attackerRoom.character;
        Character defender = defenderRoom.character;
        CardMenu cardMenu = Singleton.instance.CardMenu;
        switch (state)
        {
            case CombatState.AttackerTurn:
                cardMenu.ShowMenu(true, attacker, Card.CardType.Combat);
                //ShowCardMenu(true, attackerRoom.character);
                break;

            case CombatState.DefenderTurn:
                cardMenu.ShowMenu(false);
                //ShowCardMenu(false);
                ShowDefenderMenu(true, defender);
                break;

            case CombatState.DefenderChooseCard:
                ShowDefenderMenu(false);
                EnableTooltipUI(false);
                cardMenu.ShowMenu(true, defender, Card.CardType.Combat);
                break;

            case CombatState.BeginCombat:
                cardMenu.ShowMenu(false);
                //run coroutine here to handle combat resolution, animations, etc.
                StartCoroutine(SimulateDiceRoll(attackerDice, defenderDice, attacker, defender));
                break;

            case CombatState.RunAway:
                cardMenu.ShowMenu(false);
                //run couroutine to handle running away
                break;

            case CombatState.WinnerTakesItemFromLoser:
                //winner chooses an item to take. Grabbing reference for use by ItemObject's OnItemSelected method.
                Singleton s = Singleton.instance;
                //s.winner = winner;
                //s.loser = loser;
                inventory.ShowInventory(true, s.loser as Hunter, hideBackButton: true);
                break;

            case CombatState.WinnerTooManyItems:
                Hunter winner = Singleton.instance.winner as Hunter;
                inventory.ShowInventory(true, winner);
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
        attacker.isDefender = false;
        defender.isAttacker = false;
        attackersTurn = true;

        //position the camera so that it's centered on the battlefield.
        GameManager gm = Singleton.instance.GameManager;
        //Vector3 newPos = fieldGrid[2, 2].transform.position;
        //gm.gameCamera.transform.position = new Vector3(-1, 5, -2);  //not sure if the camera will remain consistent with these values.
        combatCamera.transform.position = new Vector3(-1, 5, -2);

        //setup dice
        //attackerDice.InitializeDice(Dice.DiceType.Attack);
        //defenderDice.InitializeDice(Dice.DiceType.Defend);

        //setup UI
        attackerNameText.text = attacker.characterName;
        attackerHealthPoints.text = string.Format("{0}/{1}", attacker.healthPoints, attacker.maxHealthPoints);
        attackerSkillPoints.text = string.Format("{0}/{1}", attacker.skillPoints, attacker.maxSkillPoints);

        defenderNameText.text = defender.characterName;
        defenderHealthPoints.text = string.Format("{0}/{1}",defender.healthPoints, defender.maxHealthPoints);
        defenderSkillPoints.text = string.Format("{0}/{1}", defender.skillPoints, defender.maxSkillPoints);

        //set up mod values
        perfectDefenseMod = 1;  //default value
        runPreventionMod = 0;
        runMod = 0;
        criticalDamageMod = 1;
        counterAttackMod = 1;
        defenseBoost = 0.5f;

        //run chance. This is displayed when the defender hovers over the run button.
        UpdateRunChance(attacker, defender, runPreventionMod, runMod);

        //attacker takes their turn first. Attack would've already been chosen, so all attacker does is pick a card.
        attackerTurnOver = false;
        defenderCounterattacking = true;

        //attacker goes first, display card menu
        ChangeCombatState(combatState = CombatState.AttackerTurn);
        
    }

    //clean up. Must restore Game Manager and update UI. The order of events here matters.
    private void EndCombat()
    {
        Singleton s = Singleton.instance;
        GameManager gm = s.GameManager;
        Room attackerLastRoom = s.attackerLastRoom;
        Room defenderLastRoom = s.defenderLastRoom;
        Dungeon dun = Singleton.instance.Dungeon;
        SetNonCombatUI(true);

        //update character states
        Character attacker = attackerRoom.character;
        Character defender = defenderRoom.character;

        

        //update hunter UI
        HunterManager hm = Singleton.instance.HunterManager;
        if (attacker is Hunter hunterAttacker)
            hm.UpdateHunterHUD(hunterAttacker);
        if (defender is Hunter hunterDefender)
            hm.UpdateHunterHUD(hunterDefender);

        //TODO: must add UI updates for non-hunters.
        /*for (int i = 0; i < hm.hunters.Count; i++)
        {
            hm.ui.hunterHuds[i].hunterHpText.text = string.Format("{0}/{1}", hm.hunters[i].healthPoints, hm.hunters[i].maxHealthPoints);
            hm.ui.hunterHuds[i].hunterSpText.text = string.Format("{0}/{1}", hm.hunters[i].skillPoints, hm.hunters[i].maxSkillPoints);
        }*/

        SceneManager.UnloadSceneAsync("Battle");
        gm.gameViewController.SetActive(true);
        dun.UpdateCharacterRoom(s.attacker, attackerLastRoom);
        dun.UpdateCharacterRoom(s.defender, defenderLastRoom);

        //check for injured hunters.
        if (s.attacker is Hunter attackHunter && attackHunter.ForceTeleport == true)
            gm.ChangeGameState(gm.gameState = GameManager.GameState.HunterInjured, attackHunter);
        else if (s.defender is Hunter defendHunter && defendHunter.ForceTeleport == true)
            gm.ChangeGameState(gm.gameState = GameManager.GameState.HunterInjured, defendHunter);
        else
            gm.ChangeGameState(gm.gameState = GameManager.GameState.Dungeon);
        
    }
    
   

    public void UpdateRunChance(Character attacker, Character defender, float runPreventionMod, float runMod)
    {
        runChance = 1 - (attacker.spd * 0.02f + runPreventionMod) + (defender.spd * 0.01f + runMod);
        runChance = runChance < 0 ? 0 : runChance;
        runChance = runChance > 1 ? 1 : runChance;
        
    }

    //sets character's position to a room on the battlefield.
    private void SetCharacterPosition(Character character, Room room)
    {
        room.character = character;
        character.transform.position = new Vector3(room.transform.position.x, character.transform.position.y, room.transform.position.z);
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

    private void EnableTooltipUI(bool toggle)
    {
        tooltipUI.SetActive(toggle);
    }

    private void UpdateSuperMeter(Hunter hunter)
    {
        HunterManager hm = Singleton.instance.HunterManager;
        bool defenderFound = false;
        int i = 0;
        while (!defenderFound && i < hm.hunters.Count)
        {
            if (hm.hunters[i] == hunter)
            {
                defenderFound = true;
                hm.ui.hunterHuds[i].superMeterUI.value += hm.SuperMeterGain_combatDamage;
            }
            else
            {
                i++;
            }
        }
    }

    public void DisplayStatusEffect(Character character, string statusText)
    {
        StartCoroutine(ShowStatusEffect(character, statusText));
    }

    

    #region Button Methods
    public void OnSelectCardButtonPressed()
    {
        //cannot proceed if no card was selected.
       // CardManager cm = Singleton.instance.CardManager;
        //if (cm.selectedCard == null)
        //return;

        Hunter attacker = attackerRoom.character as Hunter;
        Hunter defender = defenderRoom.character as Hunter;

        if (combatState == CombatState.AttackerTurn)
        {
            if (attackersCard == null)
                return;

            activeCard_attackerText.text = string.Format("Active Card: {0}", attackersCard.cardName);
            attacker.cards.Remove(attackersCard);
            attackersCard.ActivateCard_Combat(attacker);
            ChangeCombatState(combatState = CombatState.DefenderTurn);
        }
        else if (combatState == CombatState.DefenderChooseCard)
        {
            if (defendersCard == null)
                return;

            activeCard_defenderText.text = string.Format("Active Card: {0}", defendersCard.cardName);
            defender.cards.Remove(defendersCard);
            defendersCard.ActivateCard_Combat(defender);
            ChangeCombatState(combatState = CombatState.BeginCombat);
        }
    }

    public void OnSkipCardButtonPressed()
    {
        CardManager cm = Singleton.instance.CardManager;

        if (combatState == CombatState.AttackerTurn)
        {
            attackersCard = null;
            ChangeCombatState(combatState = CombatState.DefenderTurn);
        }
        else
        {
            defendersCard = null;
            ChangeCombatState(combatState = CombatState.BeginCombat);
        }
    }

    public void OnCounterAttackButtonPressed()
    {
        //perform a basic attack with a 50% damage reduction. Passive skill effects apply.
        Character defender = defenderRoom.character;
        defender.ChangeCharacterState(defender.characterState = Character.CharacterState.Attacking);
        ChangeCombatState(combatState = CombatState.DefenderChooseCard);
    }

    public void OnGuardButtonPressed()
    {
        //roll 2 dice to reduce damage, with a chance of perfect guard
        Character defender = defenderRoom.character;
        defender.ChangeCharacterState(defender.characterState = Character.CharacterState.Guarding);
        ChangeCombatState(combatState = CombatState.DefenderChooseCard);
    }

    public void OnEscapeButtonPressed()
    {
        Character attacker = attackerRoom.character;
        Character defender = defenderRoom.character;
        UpdateRunChance(attacker, defender, runPreventionMod, runMod);
        defender.ChangeCharacterState(defender.characterState = Character.CharacterState.Moving); //must increase animation speed temporarily
        ChangeCombatState(combatState = CombatState.DefenderChooseCard);
    }

    

    public void OnSurrenderButtonPressed()
    {
        Hunter hunter = defenderRoom.character as Hunter;
        if (hunter.inventory.Count <= 0)
            return;

        //open defender's inventory so they can choose an item to give to attacker.
        inventory.ShowInventory(true, hunter);       
    }
    #endregion

    #region Button Hover Methods
    public void OnCounterAttackButtonHover()
    {
        EnableTooltipUI(true);
        tooltipText.text = "Perform basic attack that inflicts 50% damage";
    }

    public void OnGuardButtonHover()
    {
        EnableTooltipUI(true);
        tooltipText.text = "Reduce damage. Perfect Defense if a 12 is rolled";
    }

    public void OnSurrenderButtonHover()
    {
        EnableTooltipUI(true);
        tooltipText.text = "Give an item to attacker, then run away";
    }

    //displays text when mouse cursor hovers over escape button
    public void OnEscapeButtonHover()
    {
        Character attacker = attackerRoom.character;
        Character defender = defenderRoom.character;
        UpdateRunChance(attacker, defender, runPreventionMod, runMod);
        EnableTooltipUI(true);
        tooltipText.text = string.Format("{0}% chance to escape", runChance * 100);
    }

    public void OnButtonExitHover()
    {
        EnableTooltipUI(false);
    }
    #endregion

    #region Coroutines

    /* State is used to determine how the defender's rolls are simulated. */
    private IEnumerator SimulateDiceRoll(Dice attackerDice, Dice defenderDice, Character attacker, Character defender)
    {
        float rollDuration = 0.4f;
        attackerTotalAttackDamage.text = "";
        defenderTotalDefense.text = "";

        //setup dice
        attackerDice.InitializeDice(Dice.DiceType.Attack);
        defenderDice.InitializeDice(Dice.DiceType.Defend);

        //attacker attacks first
        //if (attackersTurn)
        //{
        //roll dice for both attacker and defender. For the attacker, must check for 'weakened' debuff.
        //NOTE: When ShowDiceUI is enabled, it immediately begins rolling
        StatusEffect_Weakened weak = attacker.GetStatusEffect(StatusEffect.Effect.Weakened, attacker.debuffs) as StatusEffect_Weakened;
        if (weak != null)
            attackerDice.ShowSingleDieUI(true);
        else
            attackerDice.ShowDiceUI(true);

        //if defender is not guarding, then only 1 die is rolled. Otherwise, DFP/RST is boosted and defender rolls 2 dice.
        if (defender.characterState == Character.CharacterState.Guarding)
        {
            defenderCounterattacking = false;
            defender.dfpMod += defenseBoost;
            defender.rstMod += defenseBoost;
            defenderDice.ShowDiceUI(true);
        }
        else
        {
            defenderDice.ShowSingleDieUI(true);
        }

        float currentTime = Time.time;
        yield return new WaitForSeconds(1);

        //get roll results. For the attacker, must check for 'weakened' debuff.
        attackRollResult = (weak != null) ? attackerDice.RollSingleDie(Dice.DiceType.Attack) : attackerDice.RollDice(Dice.DiceType.Attack);
        defendRollResult = (defender.characterState == Character.CharacterState.Guarding) ? defenderDice.RollDice(Dice.DiceType.Defend) : 
            defenderDice.RollSingleDie(Dice.DiceType.Defend);

        //apply mods
        perfectDefenseMod = defenderDice.RolledTwelve() ? 0 : 1;
        //criticalDamageMod = attackerDice.RolledTwelve() ? 2 : 1;
        counterAttackMod = attacker.isDefender ? 0.5f : 1;

        //critical check
        if (attackerDice.RolledTwelve())
        {
            //is the weapon an augmenter?
            if (attacker is Hunter hunter && hunter.equippedWeapon.weaponType == Weapon.WeaponType.Augmenter)
            {
                criticalDamageMod = 1;
                hasAugmenter = true;
            }
            else
            {
                criticalDamageMod = 2;
            }
        }

        //attack is performed.
        yield return new WaitForSeconds(1);
        attackerDice.ShowDiceUI(false);
        attackerDice.ShowSingleDieUI(false);
        defenderDice.ShowDiceUI(false);
        defenderDice.ShowSingleDieUI(false);

        yield return Attack(attacker, defender);

    }

    IEnumerator Attack(Character attacker, Character defender)
    {
        // attacker.chosenSkill.ActivateSkill(attacker, defender);
        StartCoroutine(ShowSkillName(attacker.chosenSkill.skillName, attacker));
        
        attacker.Attack(attacker.chosenSkill, defender);
        yield return null;

        //yield return ResolveDamage(attacker, defender);
    }

    public void DoDamage(Character attacker, Character defender)
    {   
       //Character defender = defenderRoom.character;
        StartCoroutine(ResolveDamage(attacker, defender));
    }

    public void DoFixedDamage(Character attacker, Character defender)
    {
        StartCoroutine(ResolveFixedDamage(attacker, defender));
    }

    public void ApplyEffect(Character target)
    {
        StartCoroutine(ResolveEffect(target));
    }

    public void CloseInventory()
    {
        StartCoroutine(CloseInventory_Coroutine());
    }

    private IEnumerator ResolveDamage(Character attacker, Character defender)
    {
        Debug.Log("Dealing damage");
        float damage;

        //blind check.
        StatusEffect_Blind blind = attacker.GetStatusEffect(StatusEffect.Effect.Blind, attacker.debuffs) as StatusEffect_Blind;
        if (blind != null && !blind.AttackSuccessful())
        {
            //no damage
            Debug.LogFormat("missed");
            damage = 0;
            damageValues[0].color = damageColor;
            damageValues[0].text = "MISS";
            damageValues[0].gameObject.SetActive(true);
        }
        else
        {
            damage = attacker.chosenSkill.CalculateDamage(attacker, defender, attackRollResult, defendRollResult);
            Debug.LogFormat("{0}'s counter attack mod: {1}", defender.characterName, counterAttackMod);
            damage = (damage < 1) ? 0 : Mathf.Round(damage * perfectDefenseMod * counterAttackMod);

            defender.healthPoints -= damage * criticalDamageMod;
            defender.healthPoints = (defender.healthPoints < 0) ? 0 : defender.healthPoints;
            Debug.LogFormat("Total damage to {0}: {1}", defender.characterName, damage);


            //text changes if critical was performed, or if character is defending.
            //damageText.color = (defender.characterState == Character.CharacterState.Guarding) ? reducedColor : damageColor;
            // damageText.text = criticalDamageMod > 1 ? string.Format("{0}!!", damage) : damage.ToString();
            //damageText.gameObject.SetActive(true);

            damageValues[0].color = (defender.characterState == Character.CharacterState.Guarding) ? reducedColor : damageColor;
            damageValues[0].text = criticalDamageMod > 1 ? string.Format("{0}!!", damage) : damage.ToString();
            damageValues[0].gameObject.SetActive(true);
        }

        //set position based on who's the current defender
        float xPos = defender.isDefender ? 6 : -1;
        Vector3 newPos = new(defender.transform.position.x + xPos, defender.transform.position.y, defender.transform.position.z);
        damageValues[0].transform.position = Camera.main.WorldToScreenPoint(newPos);

        //update HP
        //GameManager gm = Singleton.instance.GameManager;
        HunterManager hm = Singleton.instance.HunterManager;
        if (!attackerTurnOver)
        {
            defenderHealthPoints.text = string.Format("{0}/{1}", defender.healthPoints, defender.maxHealthPoints);
            //update super meter
            if (defender is Hunter hunterDefender)
            {
                //UpdateSuperMeter(hunter);
                hunterDefender.super.AddMeter(hm.SuperMeterGain_combatDamage);
            }

        }
        else
        {
            //updating attacker's HP since defender is counterattacking. defender is still referenced because the attacker is currently defending
            attackerHealthPoints.text = string.Format("{0}/{1}", defender.healthPoints, defender.maxHealthPoints);

            //update super meter
            if (defender is Hunter hun)
            {
                //UpdateSuperMeter(hunter);
                hun.super.AddMeter(hm.SuperMeterGain_combatDamage);
            }
        }

        //animate damage
        float duration = 1;
        float currentTime = Time.time;
        Vector3 textPos = damageValues[0].transform.position;
        Vector3 origPos = textPos;

        while (Time.time < currentTime + duration)
        {
            textPos = new Vector3(textPos.x, textPos.y + 50 * Time.deltaTime, textPos.z);
            damageValues[0].transform.position = textPos;
            yield return null;
        }

        damageValues[0].transform.position = origPos;
        damageValues[0].gameObject.SetActive(false);

        //if augmenter was used, apply debuff
        if (hasAugmenter && attacker is Hunter hunter)
        {
            hunter.equippedWeapon.ApplyAugmenterDebuff(defender);
        }

        //check if defender counterattacks, otherwise combat is over.
        if (defender.characterState == Character.CharacterState.Guarding)
        {
            defender.dfpMod -= defenseBoost;
            defender.rstMod -= defenseBoost;
        }
        
        attackerTurnOver = true;
        yield return new WaitForSeconds(1);
        if (defender.healthPoints > 0 && attackerTurnOver && defenderCounterattacking)
        {
            defenderCounterattacking = false;
            //defender always uses basic attack
            defender.chosenSkill = defender.skills[0] as ActiveSkill;

            //reset the criticalmod
            criticalDamageMod = 1;

            //yield return new WaitForSeconds(1);
            yield return SimulateDiceRoll(defenderDice, attackerDice, defender, attacker);
        }
        else
        {
            yield return CheckCombatResults(attacker, defender);
        }

    }

    //special coroutine for skills that deal fixed damage that don't require dice roll.
    IEnumerator ResolveFixedDamage(Character attacker, Character defender)
    {
        Debug.Log("Dealing fixed damage");

        float damage = attacker.chosenSkill.CalculateFixedDamage(attacker, defender);
        damage = (damage < 1) ? 0 : Mathf.Round(damage);

        defender.healthPoints -= damage;
        defender.healthPoints = (defender.healthPoints < 1) ? 0 : defender.healthPoints;
        Debug.LogFormat("Total damage to {0}: {1}", defender.characterName, damage);

        //text changes if critical was performed, or if character is defending.
        damageText.color = damageColor;
        damageText.text = damage.ToString();
        damageText.gameObject.SetActive(true);

        //set position based on who's the current defender
        float xPos = defender.isDefender ? 6 : -1;
        Vector3 newPos = new(defender.transform.position.x + xPos, defender.transform.position.y, defender.transform.position.z);
        damageText.transform.position = Camera.main.WorldToScreenPoint(newPos);

        //update HP
        //GameManager gm = Singleton.instance.GameManager;
        //HunterManager hm = Singleton.instance.HunterManager;
        defenderHealthPoints.text = string.Format("{0}/{1}", defender.healthPoints, defender.maxHealthPoints);
        /*if (!attackerTurnOver)
        {
            defenderHealthPoints.text = string.Format("{0}/{1}", defender.healthPoints, defender.maxHealthPoints);
            //update super meter
            if (defender is Hunter hunter)
            {
                UpdateSuperMeter(hunter);
            }

        }
        else
        {
            //updating attacker's HP since defender is counterattacking. defender is still referenced because the attacker is currently defending
            attackerHealthPoints.text = string.Format("{0}/{1}", defender.healthPoints, defender.maxHealthPoints);

            //update super meter
            if (defender is Hunter hunter)
            {
                UpdateSuperMeter(hunter);
            }
        }*/

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
            defenderCounterattacking = false;
            //defender always uses basic attack
            defender.chosenSkill = defender.skills[0] as ActiveSkill;

            //reset the criticalmod
            criticalDamageMod = 1;
            hasAugmenter = false;

            yield return new WaitForSeconds(1);
            yield return SimulateDiceRoll(defenderDice, attackerDice, defender, attacker);
        }

        //if we get here, combat has ended.
        yield return new WaitForSeconds(3);
        EndCombat();
        yield return null;
    }

    //for skills that don't deal damage
    IEnumerator ResolveEffect(Character target)
    {
        yield return null;
    }

    IEnumerator ShowStatusEffect(Character character, string statusText)
    {
        float duration = 1;
        float currentTime = Time.time;
        Vector3 charPos = new Vector3(character.transform.position.x + 2, character.transform.position.y + 2, 0);
        this.statusText.transform.position = Camera.main.WorldToScreenPoint(charPos);
        Vector3 textPos = this.statusText.transform.position;

        //show status effect text
        this.statusText.gameObject.SetActive(true);
        this.statusText.text = statusText;
        while (Time.time < currentTime + duration)
        {
            textPos = new Vector3(textPos.x, textPos.y + 50 * Time.deltaTime, textPos.z);
            this.statusText.transform.position = textPos;
            yield return null;
        }

        this.statusText.gameObject.SetActive(false);
    }

    private IEnumerator ShowSkillName(string skillName, Character skillUser)
    {
        skillNameUI.SetActive(true);

        Color attackerColor = new Color(0, 0, 0.5f, 0.5f);
        Color defenderColor = new Color(0.5f, 0, 0, 0.5f);
        //get skill name
        skillNameText.text = skillUser.chosenSkill.skillName;

        //change window colour based on who is attacking and defending.
        skillNameBackground.color = skillUser.isAttacker ? attackerColor : defenderColor;

        //turn off after a few seconds
        yield return new WaitForSeconds(2);
        skillNameUI.SetActive(false);

    }

    private IEnumerator CheckCombatResults(Character attacker, Character defender)
    {
        //is one the opponents defeated?
        EffectManager em = Singleton.instance.EffectManager;
        if (attacker.healthPoints <= 0)
        {
            CharacterDefeated = true;
            //was the opponent another hunter?
            if (attacker is Hunter && defender is Hunter)
            {
                attacker.ChangeCharacterState(attacker.characterState = Character.CharacterState.Injured);
                em.AddEffect(StatusEffect.Effect.Injured, attacker);

                //TODO: attacker takes item from defender.
                yield return new WaitForSeconds(1);
                yield return TakeItemFromLoser(defender as Hunter, attacker as Hunter);
                //TakeItemFromLoser(defender as Hunter, attacker as Hunter);
            }
            else if (attacker is Hunter && defender is Monster)
            {
                //the opponent was a monster; remove hunter from the game.
                yield return KillHunter(attacker as Hunter);
            }
            else
            {
                //attacker is a monster; remove from game.
                //grant money to opponent
                GameManager gm = Singleton.instance.GameManager;
                if (attacker == gm.ActiveCharacter())
                    gm.ActiveCharacterDefeated = true; //this will handle removing monster once game manager resumes.

                //roll for item
                yield return RollForLoot(defender as Hunter, attacker as Monster);
            }
        }
        else
        {
            attacker.ChangeCharacterState(attacker.characterState = Character.CharacterState.Idle);
        }

        if (defender.healthPoints <= 0)
        {
            CharacterDefeated = true;
            if (attacker is Hunter && defender is Hunter)
            {
                defender.ChangeCharacterState(defender.characterState = Character.CharacterState.Injured);
                em.AddEffect(StatusEffect.Effect.Injured, defender);

                //TODO: attacker takes item from defender.
                yield return new WaitForSeconds(1);
                yield return TakeItemFromLoser(attacker as Hunter, defender as Hunter);
                //TakeItemFromLoser(attacker as Hunter, defender as Hunter);
            }
            else if (defender is Hunter && attacker is Monster)
            {
                //the opponent was a monster; remove hunter from the game.
                yield return KillHunter(defender as Hunter);
            }
            else
            {
                //defender is a monster; remove from game.
                GameManager gm = Singleton.instance.GameManager;
                //gm.ForceStop = true;    
                if (defender == gm.ActiveCharacter())
                    gm.ActiveCharacterDefeated = true; //this will handle removing monster once game manager resumes.

                defender.gameObject.SetActive(false);
                
                //roll for item
                yield return RollForLoot(attacker as Hunter, defender as Monster);
            }
        }
        else
        {
            defender.ChangeCharacterState(defender.characterState = Character.CharacterState.Idle);
        }

        //if nobody was defeated, end combat
        if (!CharacterDefeated)
        {
            yield return new WaitForSeconds(2);
            EndCombat();
        }

    }

    private IEnumerator TakeItemFromLoser(Hunter winner, Hunter loser)
    {
        //open up the loser's inventory
        Singleton s = Singleton.instance;
        s.winner = winner;
        s.loser = loser;
        ChangeCombatState(combatState = CombatState.WinnerTakesItemFromLoser);
        //inventory.ShowInventory(true, loser, hideBackButton: true);

        //winner chooses an item to take. Grabbing reference for use by ItemObject's OnItemSelected method.
        //Singleton s = Singleton.instance;
        //s.winner = winner;
        //s.loser = loser;
        //if winner has too many items, winner drops an item. Target item cannot be dropped.
        //end combat
        yield return null;
        //yield return new WaitForSeconds(2);
        //EndCombat();
    }

    private IEnumerator RollForLoot(Hunter hunter, Monster monster)
    {
        //grant money to winner TODO: show UI
        hunter.credits += monster.credits;
        yield return ShowRewardUI(string.Format("Obtained {0} CR", monster.credits), 1.3f);

        //check if monster drops an item
        float roll = Random.value;
        Debug.LogFormat("Rolling for item chance. Rolled {0}", roll);
        if (roll <= monster.monsterData.dropChance)
        {
            //access drop table, starting from the highest level table.
            int i = monster.monsterData.dropTables.Count - 1;
            bool tableFound = false;
            while (!tableFound && i >= 0)
            {
                if (hunter.hunterLevel >= monster.monsterData.dropTables[i].minLevel)
                {
                    tableFound = true;
                    //roll for item
                    int totalWeight = 0;
                    List<MonsterData.DropTable> dropTable = monster.monsterData.dropTables[i].dropTable;
                    for (int j = 0; j < dropTable.Count; j++)
                    {
                        totalWeight += dropTable[j].itemWeight;
                    }

                    int randValue = Random.Range(0, totalWeight);

                    //which item did we get?
                    int k = 0;
                    bool itemFound = false;
                    while(!itemFound && k < dropTable.Count)
                    {
                        if (randValue <= dropTable[k].itemWeight)
                        {
                            //add item
                            itemFound = true;
                            yield return ShowRewardUI(string.Format("Found {0}!", dropTable[k].item.itemName), 1.3f);
                            hunter.inventory.Add(dropTable[k].item);
                        }
                        else
                        {
                            randValue -= dropTable[k].itemWeight;
                            k++;
                        }
                    }

                }
                else
                {
                    i--;
                }
            }

        }

        //if item drops, add item to winner's inventory
        //if winner has too many items, they drop one.
        //display UI showing what the hunter recieved.
        //yield return null;
        yield return new WaitForSeconds(2);
        EndCombat();
    }

    private IEnumerator KillHunter(Hunter hunter)
    {
        //hunter is removed from dungeon
        //all collected items are dropped for anyone to pick up
        yield return new WaitForSeconds(2);
        EndCombat();
    }

    //shows item name for a duration.
    private IEnumerator ShowRewardUI(string itemName, float duration)
    {
        droppedItemText.text = itemName; 
        droppedItemUI.SetActive(true);
        yield return new WaitForSeconds(duration);
        droppedItemUI.SetActive(false);
    }

    //closes inventory after winner of combat takes item from loser, then ends combat.
    private IEnumerator CloseInventory_Coroutine()
    {
        inventory.ShowInventory(false);
        //show message of item that winner obtained
        if (combatState == CombatState.WinnerTakesItemFromLoser)
            yield return ShowRewardUI(string.Format("Took {0}!", ItemTaken.itemName), 1.3f);

        //if the winner has too many items, they must make space or drop the item they took. Cannot drop target item.
        Hunter winner = Singleton.instance.winner as Hunter;
        HunterManager hm = Singleton.instance.HunterManager;

        if (winner.inventory.Count > hm.MaxInventorySize)
        {
            ChangeCombatState(combatState = CombatState.WinnerTooManyItems);
        }
        else
        {
            yield return new WaitForSeconds(2);
            EndCombat();
        }
    }
    #endregion
}
