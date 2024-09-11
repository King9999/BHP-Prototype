using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* This class is a container for all Item scriptable objects. These objects will be interactable. 
 Must have a Sprite Renderer and a Button component.
 */
public class ItemObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Item item;

    [Header("---UI----")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI detailsText;
    public TextMeshProUGUI itemLevelText;
    public TextMeshProUGUI priceText;           //cost of an item when purchasing. Sell price is 75% of this value.
    public TextMeshProUGUI isKeyItemText;      //key items cannot be sold or dropped.
    public TextMeshProUGUI isTargetItemText;   //the target item required to complete a dungeon.
    public Image itemImage, itemBackground;     //item background is used for higlighting selected item.
    Color highlightColor, normalColor;
    bool showItemDetails;

    // Start is called before the first frame update
    void Start()
    {
        //GetItemData(item);
        //SpriteRenderer sr = GetComponent<SpriteRenderer>();
        //sr.sprite = item.sprite;
        highlightColor = new Color(128, 0, 0, 0.5f);
        normalColor = itemBackground.color;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        itemBackground.color = highlightColor;
        showItemDetails = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        itemBackground.color = normalColor;
        showItemDetails = false;
    }

    /* gather all relevant information based on the item type */
    public void GetDetails(Item item)
    {
        this.item = item;
        itemNameText.text = item.itemName;
    }

    public void OnItemSelected()
    {
        //mouse button code. Different options based on the item.
    }
}
