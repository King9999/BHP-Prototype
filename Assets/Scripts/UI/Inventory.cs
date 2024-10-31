using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

//UI for displaying a hunter's items that can be taken by other hunters.
public class Inventory : MonoBehaviour
{
    public List<ItemObject> items;
    [SerializeField] private ItemObject extraItem;        //used when there's no room in inventory.
    [SerializeField]private GameObject detailsWindow;
    [SerializeField] private GameObject extraItemInventory;

    public TextMeshProUGUI creditsText, itemTypeText, itemDetailsText;         //creditsText is money on hand
    //private byte emptyIndex;         //tracks which item object has available space.

    // Start is called before the first frame update
    void Start()
    {
        Singleton.instance.Inventory = this;
        //emptyIndex = 0;
    }

    public void AddItem(Item item)
    {
        //before adding item, set the index to the last space
        //look for available space to add item.
        //items[emptyIndex].item = item;
        //emptyIndex++;
        bool spaceFound = false;
        int i = 0;
        while (!spaceFound && i < items.Count)
        {
            if (items[i].item == null)
            {
                spaceFound = true;
                items[i].item = item;
            }
            else
            {
                i++;
            }
        }

        if (!spaceFound)
        {
            //no space, player must make room.
            ShowExtraItemInventory(true, extraItem: item);
        }
    }

    public void RemoveItem(Item item)
    {
        //look for available space to add item.
        bool itemFound = false;
        int i = 0;
        while (!itemFound && i < items.Count)
        {
            if (items[i].item == item)
            {
                itemFound = true;
                items[i].item = null;
            }
            else
            {
                i++;
            }
        }

        //sort items so empty spaces are always at the bottom
        for (int j = 0; j < items.Count; j++)
        {
            if (items[j].item != null)
                continue;

            //emptyIndex = j;             //found an empty space
            if (j + 1 < items.Count)
            {
                ItemObject copy = items[j + 1];
                items[j + 1] = items[j];
                items[j] = copy;
            }
        }
        //items = items.OrderBy(x => x.item != null).ToList();
    }

    /*public bool InventoryFull()
    {
        return emptyIndex >= items.Count;
    }*/

    public void ShowItemDetails(bool toggle)
    {
        detailsWindow.SetActive(toggle);
    }

    public void ShowInventory(bool toggle, Hunter hunter = null)
    {
        gameObject.SetActive(toggle);
        ShowItemDetails(false);
        ShowExtraItemInventory(false);
        if (toggle == true)
        {
            for (int i = 0; i < hunter.inventory.Count; i++)
            {
                //if inventory is full, show extra inventory
                if (i >= hunter.maxInventorySize)
                {
                    ShowExtraItemInventory(true, items[i].item);
                    continue;
                }

                items[i].gameObject.SetActive(true);
                items[i].item = hunter.inventory[i];

                //consumables are marked for easy reading.
                Item item = items[i].item;
                if (item is Consumable)
                    items[i].itemNameText.text = string.Format("{0}(Usable)", hunter.inventory[i].itemName);
                else
                    items[i].itemNameText.text = hunter.inventory[i].itemName;
            }

            //update money
            creditsText.text = string.Format("Money: {0} CR", hunter.credits);
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].gameObject.SetActive(false);
            }
        }
    }

    public void ShowExtraItemInventory(bool toggle, Item extraItem = null)
    {
        extraItemInventory.SetActive(toggle);
        if (toggle == true)
        {
            this.extraItem.item = extraItem;
        }
    }

    public bool ExtraInventoryOpen()
    {
        return extraItemInventory.activeSelf;
    }

    //method for Back button
    public void OnBackButtonPressed()
    {
        //close inventory
        ShowInventory(false);
    }

    public void OnDropItemButtonPressed()
    {
        extraItem.item = null;
        ShowExtraItemInventory(false);
    }
}
