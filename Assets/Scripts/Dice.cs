using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//simple script for rolling two six-sided dice. There will be an option to roll one die if necessary.
public class Dice : MonoBehaviour
{
    public int die1, die2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int RollDice(/*int die1, int die2*/)
    {
        die1 = Random.Range(1, 7);
        die2 = Random.Range(1, 7);

        return die1 + die2;
    }

    public int RollDie()
    {
        die1 = Random.Range(1, 7);
        die2 = 0;
        return die1;
    }
}
