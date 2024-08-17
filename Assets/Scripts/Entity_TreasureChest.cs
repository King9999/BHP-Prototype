using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Treasure chests contain randomly generated items. Characters can pass through them, and also land on them,
 * but only Hunters can open chests. */
public class Entity_TreasureChest : Entity
{
    public Item item;       //the item the chest holds
    public Sprite openChest, closedChest;

    // Start is called before the first frame update
    void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = closedChest;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
