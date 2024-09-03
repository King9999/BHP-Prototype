using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//simple script for rolling two six-sided dice. There will be an option to roll one die if necessary.
public class Dice : MonoBehaviour
{
    public int die1, die2;
    public Sprite[] diceSprites;
    public Image[] dieImages;     //0 is die 1, 1 is die 2
    public TextMeshProUGUI die1_text, die2_text;
    bool showDice, showSingleDie;
    public GameObject diceContainer1, diceContainer2;   //dice container 2 shows single die roll

    // Start is called before the first frame update
    void Start()
    {
        ShowDiceUI(false);
        ShowSingleDieUI(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (showDice == true)
        {
            //roll dice
            die1 = Random.Range(1, 7);
            die2 = Random.Range(1, 7);
            dieImages[0].sprite = diceSprites[die1 - 1];
            dieImages[1].sprite = diceSprites[die2 - 1];
        }

        if (showSingleDie == true)
        {
            //roll single die
            die1 = Random.Range(1, 7);
            die2 = 0;
            dieImages[0].sprite = diceSprites[die1 - 1];
        }
    }

    public int RollDice()
    {
        showDice = false;
        die1 = Random.Range(1, 7);
        die2 = Random.Range(1, 7);
        dieImages[0].sprite = diceSprites[die1 - 1];
        dieImages[1].sprite = diceSprites[die2 - 1];
        return die1 + die2;
    }

    public int RollSingleDie()
    {
        showSingleDie = false;
        die1 = Random.Range(1, 7);
        die2 = 0;
        dieImages[0].sprite = diceSprites[die1 - 1];
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
            die1_text = diceContainer1.GetComponentInChildren<TextMeshProUGUI>();
            die1_text.text = "";
            die2_text.text = "";
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
            die1_text = diceContainer2.GetComponentInChildren<TextMeshProUGUI>();
            die1_text.text = "";
            die2_text.text = "";
        }
        else
        {
            showSingleDie = false;
        }
    }    
}
