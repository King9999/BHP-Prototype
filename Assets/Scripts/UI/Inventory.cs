using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

//UI for displaying a hunter's items that can be taken by other hunters.
public class Inventory : MonoBehaviour
{
    public List<ItemObject> items;
    public ItemObject extraItem;        //used when there's no room in inventory.
    [SerializeField]private GameObject detailsWindow;
    [SerializeField] private GameObject extraItemInventory;
    [SerializeField] private Button backButton;         

    public TextMeshProUGUI creditsText, itemTypeText, itemDetailsText;         //creditsText is money on hand
    //private byte emptyIndex;         //tracks which item object has available space.

    // Start is called before the first frame update
    void Start()
    {
        Singleton.instance.Inventory = this;
        //emptyIndex = 0;
    }

    //ensure that we never lose reference to inventory.
    private void OnEnable()
    {
        Singleton.instance.Inventory = this;
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
            ShowExtraItemInventory(true, item);
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

    //back button is hidden when a hunter loses to another hunter in combat.
    public void ShowInventory(bool toggle, Hunter hunter = null, bool hideBackButton = false)
    {
        gameObject.SetActive(toggle);
        ShowItemDetails(false);
        ShowExtraItemInventory(false);

        
        if (!backButton.gameObject.activeSelf)
            ShowBackButton(true);
        if (hideBackButton)
            ShowBackButton(false);           //not sure why I can't do 1 condition check on one line here.


        if (toggle == true)
        {
            for (int i = 0; i < hunter.inventory.Count; i++)
            {
                //if inventory is full, show extra inventory
                HunterManager hm = Singleton.instance.HunterManager;
                if (i >= hm.MaxInventorySize)
                {
                    ShowExtraItemInventory(true, hunter.inventory[i]);
                    continue;
                }

                items[i].gameObject.SetActive(true);
                items[i].item = hunter.inventory[i];
                items[i].GetItemData(items[i].item);
               
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

    /// <summary>
    /// Swaps two items in item objects.
    /// </summary>
    /// <param name="itemA">The first item to switch to the second item object.</param>
    /// <param name="itemB">The second item to switch to the first item object.</param>
    public void SwapItems(ItemObject itemA, ItemObject itemB)
    {
        Item objCopy = itemA.item;
        itemA.item = itemB.item;
        itemB.item = objCopy;
        itemA.GetItemData(itemA.item);
        itemB.GetItemData(itemB.item);
        //extraItem.GetItemData(extraItem.item);
    }

    public void ShowExtraItemInventory(bool toggle, Item item = null)
    {
        extraItemInventory.SetActive(toggle);
        if (toggle == true)
        {
            extraItem.item = item;
            extraItem.GetItemData(item);

            //hide back button
            ShowBackButton(false);
        }
    }

    public void ShowBackButton(bool toggle)
    {
        backButton.gameObject.SetActive(toggle);
    }

    public bool ExtraInventoryOpen()
    {
        return extraItemInventory.activeSelf;
    }

    //method for Back button
    public void OnBackButtonPressed()
    {
        //if extra inventory is open, cannot proceed.
        if (ExtraInventoryOpen())
            return;

        //close inventory
        ShowInventory(false);
    }

    //button method for Drop Item button. Also checks character state, and will open inventory again
    //if there's somehow still too many items.
    public void OnDropItemButtonPressed()
    {
        //if item is the target item or key item, cannot proceed.
        if (extraItem.item.isTargetItem || extraItem.item.isKeyItem)
        {
            Debug.Log("Item is important! Must take it!");
            return;
        }

        GameManager gm = Singleton.instance.GameManager;
        //check if we're in combat. If so, then winner had too many items and is cleaning up.
        if (gm.gameState == GameManager.GameState.Combat)
        {
            Combat combat = Singleton.instance.Combat;
            Hunter winner = Singleton.instance.winner as Hunter;

            //remove item from hunter inventory
            winner.inventory.Remove(extraItem.item);
            extraItem.item = null;
            combat.CloseInventory();
        }
        else
        {
            Hunter hunter = gm.ActiveCharacter() as Hunter;

            //remove item from hunter inventory
            hunter.inventory.Remove(extraItem.item);
            extraItem.item = null;
            ShowInventory(false);
        
            gm.CharacterState(gm.ActiveCharacter());
           
        }
    }

    //used by Items button in the field
    public void OnItemButtonPressed()
    {
       GameManager gm = Singleton.instance.GameManager;
       Hunter hunter = gm.ActiveCharacter() as Hunter;
       ShowInventory(true, hunter);
    }
}
