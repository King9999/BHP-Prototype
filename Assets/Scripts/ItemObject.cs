using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* This class is a container for all Item scriptable objects. These objects will be interactable. 
 Must have a Sprite Renderer and a Button component.
 */
public class ItemObject : MonoBehaviour
{
    public Item item;

    [Header("---UI----")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI detailsText;
    public TextMeshProUGUI itemLevelText;
    public TextMeshProUGUI priceText;           //cost of an item when purchasing. Sell price is 75% of this value.
    public TextMeshProUGUI isKeyItemText;      //key items cannot be sold or dropped.
    public TextMeshProUGUI isTargetItemText;   //the target item required to complete a dungeon.
    public Image itemImage;

    // Start is called before the first frame update
    void Start()
    {
        //GetItemData(item);
        //SpriteRenderer sr = GetComponent<SpriteRenderer>();
        //sr.sprite = item.sprite;
    }

   
    public void GetItemData(Item item)
    {
        itemNameText.text = item.itemName;
        itemImage.sprite = item.sprite;
    }

    public void ClearItemData()
    {
        item = null;
        itemNameText.text = "";
        itemImage.sprite = null;
    }


    /* Used to display all relevant information based on the item type */
    public void ShowDetails(Item item)
    {

    }

    public void OnItemSelected()
    {
        //mouse button code. Different options based on the item.
    }
}
