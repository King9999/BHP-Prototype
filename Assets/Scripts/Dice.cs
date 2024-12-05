using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

//simple script for rolling two six-sided dice. There will be an option to roll one die if necessary.
public class Dice : MonoBehaviour
{
    public int die1, die2;
    public DiceObject[] diceObjects;
    //[SerializeField] public Sprite[,] diceSprites;
    public Image[] dieImages;     //0 is die 1, 1 is die 2
    public TextMeshProUGUI singleDieText, TwoDiceText;
    bool showDice, showSingleDie;
    public GameObject diceContainer1, diceContainer2;   //dice container 2 shows single die roll

    public enum DiceType { Move, Attack, Defend}   //move = blue, attack = red, yellow = defend
    public DiceType diceType;

    [Serializable]
    public struct DiceObject
    {
        public Sprite[] diceSprites;
    }


    // Start is called before the first frame update
    void Start()
    {
        ShowDiceUI(false);
        ShowSingleDieUI(false);
    }


    //sets the correct sprite set before the update loop.
    public void InitializeDice(DiceType diceType)
    {
        this.diceType = diceType;
    }

    // Update is called once per frame
    void Update()
    {
        if (showDice == true)
        {
            //roll dice
            die1 = UnityEngine.Random.Range(1, 7);
            die2 = UnityEngine.Random.Range(1, 7);
            dieImages[0].sprite = diceObjects[(int)diceType].diceSprites[die1 - 1];
            dieImages[1].sprite = diceObjects[(int)diceType].diceSprites[die2 - 1];
        }

        if (showSingleDie == true)
        {
            //roll single die
            die1 = UnityEngine.Random.Range(1, 7);
            die2 = 0;
            dieImages[0].sprite = diceObjects[(int)diceType].diceSprites[die1 - 1];
        }
    }

    public int RollDice(DiceType diceType)
    {
        showDice = false;
        this.diceType = diceType;
        die1 = UnityEngine.Random.Range(1, 7);
        die2 = UnityEngine.Random.Range(1, 7);
        dieImages[0].sprite = diceObjects[(int)diceType].diceSprites[die1 - 1];
        dieImages[1].sprite = diceObjects[(int)diceType].diceSprites[die2 - 1];
        TwoDiceText.text = string.Format("{0}", die1 + die2);
        //die2_text.text = die2.ToString();
        return die1 + die2;
    }

    public int RollSingleDie(DiceType diceType)
    {
        showSingleDie = false;
        this.diceType = diceType;
        die1 = UnityEngine.Random.Range(1, 7);
        die2 = 0;
        dieImages[0].sprite = diceObjects[(int)diceType].diceSprites[die1 - 1];
        singleDieText.text = die1.ToString();

        //check if there's a bonus 
        GameManager gm = Singleton.instance.GameManager;
        if (gm.gameState != GameManager.GameState.Combat && (gm.movementMod > 0 || gm.ActiveCharacter().mov > 0))
        {
            singleDieText.text = string.Format("{0}<color=green>+{1}</color>", die1, gm.movementMod + gm.ActiveCharacter().mov);
        }

        return die1;
    }

    public void ShowDiceUI(bool toggle)
    {
        diceContainer1.SetActive(toggle);
        if (toggle == true)
        {
            showDice = true;
            Image[] images = diceContainer1.GetComponentsInChildren<Image>();
            dieImages[0] = images[1];   //because there are 2 image objects, we want the die image located at index 1.
            TwoDiceText = diceContainer1.GetComponentInChildren<TextMeshProUGUI>();
            TwoDiceText.text = "";
            //die2_text.text = "";
        }
        else
        {
            showDice = false;
        }
    }

    public void ShowSingleDieUI(bool toggle)
    {
        diceContainer2.SetActive(toggle);
        if (toggle == true)
        {
            showSingleDie = true;
            Image[] images = diceContainer2.GetComponentsInChildren<Image>();
            dieImages[0] = images[1]; //because there are 2 image objects, we want the die image located at index 1.
            singleDieText = diceContainer2.GetComponentInChildren<TextMeshProUGUI>();
            singleDieText.text = "";
            //die2_text.text = "";
        }
        else
        {
            showSingleDie = false;
        }
    }
    
    public bool RolledTwelve()
    {
        return die1 + die2 >= 12;
    }
}
