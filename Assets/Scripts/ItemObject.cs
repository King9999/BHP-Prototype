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
    public TextMeshProUGUI detailsText, itemTypeText;
    public TextMeshProUGUI itemLevelText;
    public TextMeshProUGUI priceText;           //cost of an item when purchasing. Sell price is 75% of this value.
    public TextMeshProUGUI isKeyItemText;      //key items cannot be sold or dropped.
    public TextMeshProUGUI isTargetItemText;   //the target item required to complete a dungeon.
    public Image itemImage, itemBackground;     //item background is used for higlighting selected item.
    Color highlightColor, normalColor;
    //bool showItemDetails;

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
        //showItemDetails = true;
        if (item != null)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.InventoryItemDetails);
        }
        GetDetails(item);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        itemBackground.color = normalColor;
        //showItemDetails = false;
        if (item != null)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.Inventory);
        }
    }

    /* gather all relevant information based on the item type */
    private void GetDetails(Item item)
    {
        HunterManager hm = Singleton.instance.HunterManager;
        this.item = item;
        //itemNameText.text = item.itemName;
        //hm.ui.itemDetailsText.fontSize = 30;
        hm.ui.itemDetailsText.text = item.details + "\n\n";
        hm.ui.itemTypeText.text = item.itemType.ToString();

        //different info is displayed depending on item type
        if (item is Weapon weapon)
        {
            //hm.ui.itemDetailsText.fontSize = 26;
            if (weapon.isUniqueItem)
            {
                hm.ui.itemTypeText.text = "Unique " + item.itemType.ToString() + "\n\n";
            }

            hm.ui.itemDetailsText.text += "Equip Level: " + weapon.itemLevel + "\nATP +" + weapon.atp + 
                " MNP +" + weapon.mnp + "\n\nItem Mods:\n";

            if (weapon.itemMods.Count > 0)
            {
                foreach (ItemMod mod in weapon.itemMods)
                {
                    if (mod.isUnique)
                        hm.ui.itemDetailsText.text += "(U)" + mod.modName + "\n";
                    else
                        hm.ui.itemDetailsText.text += mod.modName + "\n";
                }
            }
            else
            {
                hm.ui.itemDetailsText.text += "<None>";
            }
        }
        else if (item is Armor armor)
        {
            if (armor.isUniqueItem)
            {
                hm.ui.itemTypeText.text = "Unique " + item.itemType.ToString() + "\n\n";
            }

            hm.ui.itemDetailsText.text += "Equip Level: " + armor.itemLevel + "\nDFP +" + armor.dfp +
                " RST +" + armor.rst + "\n\nItem Mods:\n";

            if (armor.itemMods.Count > 0)
            {
                foreach (ItemMod mod in armor.itemMods)
                {
                    if (mod.isUnique)
                        hm.ui.itemDetailsText.text += "(U) " + mod.modName + "\n";
                    else
                        hm.ui.itemDetailsText.text += mod.modName + "\n";
                }
            }
            else
            {
                hm.ui.itemDetailsText.text += "<None>";
            }
        }
        else if (item is Accessory acc)
        {
            if (acc.isUniqueItem)
            {
                hm.ui.itemTypeText.text = "Unique " + item.itemType.ToString() + "\n\n";
            }

            hm.ui.itemDetailsText.text += acc.statBonuses + "\n\nItem Mods:\n";

            if (acc.itemMods.Count > 0)
            {
                foreach (ItemMod mod in acc.itemMods)
                {
                    if (mod.isUnique)
                        hm.ui.itemDetailsText.text += "(U) " + mod.modName + "\n";
                    else
                        hm.ui.itemDetailsText.text += mod.modName + "\n";
                }
            }
            else
            {
                hm.ui.itemDetailsText.text += "<None>";
            }
        }
    }

    public void OnItemSelected()
    {
        //mouse button code. Different options based on the item.
        GameManager gm = Singleton.instance.GameManager;
        if (item is Consumable consumable)
        {
            if (gm.ActiveCharacter() is Hunter hunter)
            {
                consumable.ActivateEffect(hunter);
                hunter.inventory.Remove(item);
                item = null;
                HunterManager hm = Singleton.instance.HunterManager;
                hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.Inventory);
                gameObject.SetActive(false);
            }
        }
    }

   
}
