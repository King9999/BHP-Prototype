using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

//UI for displaying a hunter's items that can be taken by other hunters.
public class Inventory : MonoBehaviour
{
    public List<ItemObject> items;
    public GameObject detailsWindow;
    public TextMeshProUGUI creditsText, itemTypeText, itemDetailsText;         //creditsText is money on hand
    private int emptyIndex;         //tracks which item object has available space.

    // Start is called before the first frame update
    void Start()
    {
        Singleton.instance.Inventory = this;
        emptyIndex = 0;
    }

    public void AddItem(Item item)
    {
        //look for available space to add item.
        items[emptyIndex].item = item;
        emptyIndex++;
        /*bool spaceFound = false;
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
        }*/
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

            emptyIndex = j;             //found an empty space
            if (j + 1 < items.Count)
            {
                ItemObject copy = items[j + 1];
                items[j + 1] = items[j];
                items[j] = copy;
            }
        }
        //items = items.OrderBy(x => x.item != null).ToList();
    }

    public bool InventoryFull()
    {
        return emptyIndex >= items.Count;
    }

    public void ShowItemDetails(bool toggle)
    {
        detailsWindow.SetActive(toggle);
    }
}
