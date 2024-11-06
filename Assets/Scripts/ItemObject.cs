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
    //public TextMeshProUGUI detailsText, itemTypeText;
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

        //consumables are marked with (Usable)
        if (item is Consumable)
        {
            itemNameText.text = string.Format("{0}(Usable)", item.itemName);
        }
    }

    private void ClearItemData()
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
            Inventory inv = Singleton.instance.Inventory;
            inv.ShowItemDetails(true);
            //HunterManager hm = Singleton.instance.HunterManager;
            //hm.ui.inventory.ShowItemDetails(true);
            //hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.InventoryItemDetails);
            GetDetails(item);
        }
        
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        itemBackground.color = normalColor;
        //showItemDetails = false;
        if (item != null)
        {
            Inventory inv = Singleton.instance.Inventory;
            inv.ShowItemDetails(false);
            //HunterManager hm = Singleton.instance.HunterManager;
            //hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.Inventory);
        }
    }

    /* gather all relevant information based on the item type */
    private void GetDetails(Item item)
    {
        Inventory inv = Singleton.instance.Inventory;
        //inv.ShowItemDetails(true);
        //HunterManager hm = Singleton.instance.HunterManager;
        //this.item = item;
        //itemNameText.text = item.itemName;
        //hm.ui.itemDetailsText.fontSize = 30;
        inv.itemDetailsText.text = string.Format("{0}\n\n", item.details);
        inv.itemTypeText.text = item.itemType.ToString();
        //hm.ui.itemDetailsText.text = item.details + "\n\n";
        //hm.ui.itemTypeText.text = item.itemType.ToString();

        //different info is displayed depending on item type
        if (item is Weapon weapon)
        {
            //hm.ui.itemDetailsText.fontSize = 26;
            if (weapon.isUniqueItem)
            {
                inv.itemTypeText.text = string.Format("Unique {0}\n\n", item.itemType.ToString());
                //hm.ui.itemTypeText.text = "Unique " + item.itemType.ToString() + "\n\n";
            }

            inv.itemDetailsText.text += string.Format("Equip Level: {0}\nATP +{1} MNP +{2}", weapon.itemLevel, weapon.atp, weapon.mnp);
            //hm.ui.itemDetailsText.text += "Equip Level: " + weapon.itemLevel + "\nATP +" + weapon.atp +
              //  " MNP +" + weapon.mnp;
            
            //weapon range
            if (weapon.minRange > 0)
            {
                inv.itemDetailsText.text += string.Format("\nRange: {0} - {1}", weapon.minRange, weapon.maxRange);
                //hm.ui.itemDetailsText.text += "\nRange: " + weapon.minRange + " - " + weapon.maxRange;
            }
            else
            {
                inv.itemDetailsText.text += string.Format("\nRange: {0}", weapon.maxRange);
                //hm.ui.itemDetailsText.text += "\nRange: " + weapon.maxRange;
            }

            //add skill if applicable
            if (weapon.itemSkill != null)
            {
                inv.itemDetailsText.text += string.Format("\n\nSkill: {0}\n{1}", weapon.itemSkill.skillName, weapon.itemSkill.skillDetails);
                //hm.ui.itemDetailsText.text += "\n\nSkill: " + weapon.itemSkill.skillName + 
                    //"\n" + weapon.itemSkill.skillDetails;

                if (weapon.itemSkill.skillType == Skill.SkillType.Active)
                {
                    //more details
                    //hm.ui.itemDetailsText.text += "\n"
                }
            }

            //add item mods
            inv.itemDetailsText.text += "\n\nItem Mods:\n";
            //hm.ui.itemDetailsText.text += "\n\nItem Mods:\n";

            if (weapon.itemMods.Count > 0)
            {
                foreach (ItemMod mod in weapon.itemMods)
                {
                    if (mod.isUnique)
                        inv.itemDetailsText.text += string.Format("[U]{0}\n", mod.modName);
                        //hm.ui.itemDetailsText.text += "(U)" + mod.modName + "\n";
                    else
                        inv.itemDetailsText.text += string.Format("{0}\n", mod.modName);
                    //hm.ui.itemDetailsText.text += mod.modName + "\n";
                }
            }
            else
            {
                inv.itemDetailsText.text += "<None>";
                //hm.ui.itemDetailsText.text += "<None>";
            }
        }
        else if (item is Armor armor)
        {
            if (armor.isUniqueItem)
            {
                inv.itemTypeText.text = string.Format("Unique {0}\n\n", item.itemType.ToString());
                //hm.ui.itemTypeText.text = "Unique " + item.itemType.ToString() + "\n\n";
            }

            inv.itemDetailsText.text += string.Format("Equip Level: {0}\nDFP +{1} RST +{2}", armor.itemLevel, 
                armor.dfp, armor.rst);
            //hm.ui.itemDetailsText.text += "Equip Level: " + armor.itemLevel + "\nDFP +" + armor.dfp +
            //" RST +" + armor.rst + "\n\nItem Mods:\n";

            //add skill if applicable
            if (armor.itemSkill != null)
            {
                inv.itemDetailsText.text += string.Format("\n\nSkill: {0}\n{1}", armor.itemSkill.skillName, armor.itemSkill.skillDetails);

                if (armor.itemSkill.skillType == Skill.SkillType.Active)
                {
                    //more details
                    //hm.ui.itemDetailsText.text += "\n"
                }
            }

            //item mods
            inv.itemDetailsText.text += "\n\nItem Mods:\n";
            if (armor.itemMods.Count > 0)
            {
                foreach (ItemMod mod in armor.itemMods)
                {
                    if (mod.isUnique)
                        inv.itemDetailsText.text += string.Format("[U]{0}\n", mod.modName);
                    //hm.ui.itemDetailsText.text += "(U) " + mod.modName + "\n";
                    else
                        inv.itemDetailsText.text += string.Format("{0}\n", mod.modName);
                    //hm.ui.itemDetailsText.text += mod.modName + "\n";
                }
            }
            else
            {
                inv.itemDetailsText.text += "<None>";
                //hm.ui.itemDetailsText.text += "<None>";
            }
        }
        else if (item is Accessory acc)
        {
            if (acc.isUniqueItem)
            {
                inv.itemTypeText.text = string.Format("Unique {0}\n\n", item.itemType.ToString());
                //hm.ui.itemTypeText.text = "Unique " + item.itemType.ToString() + "\n\n";
            }

            //add skill if applicable
            if (acc.itemSkill != null)
            {
                inv.itemDetailsText.text += string.Format("\n\nSkill: {0}\n{1}", acc.itemSkill.skillName, acc.itemSkill.skillDetails);

                if (acc.itemSkill.skillType == Skill.SkillType.Active)
                {
                    //more details
                    //hm.ui.itemDetailsText.text += "\n"
                }
            }

            //display all applicable stat bonuses
            inv.itemDetailsText.text += string.Format("{0}\n\nItem Mods:\n", acc.statBonuses);
            //hm.ui.itemDetailsText.text += acc.statBonuses + "\n\nItem Mods:\n";

            if (acc.itemMods.Count > 0)
            {
                foreach (ItemMod mod in acc.itemMods)
                {
                    if (mod.isUnique)
                        inv.itemDetailsText.text += string.Format("[U]{0}\n", mod.modName);
                    //hm.ui.itemDetailsText.text += "(U) " + mod.modName + "\n";
                    else
                        inv.itemDetailsText.text += string.Format("{0}\n", mod.modName);
                    //hm.ui.itemDetailsText.text += mod.modName + "\n";
                }
            }
            else
            {
                inv.itemDetailsText.text += "<None>";
                //hm.ui.itemDetailsText.text += "<None>";
            }
        }
    }

    //button method used by ItemObject itself.
    public void OnItemSelected()
    {
        //mouse button code. Different options based on the item.
        GameManager gm = Singleton.instance.GameManager;
        Hunter hunter = gm.ActiveCharacter() as Hunter;
        Inventory inv = Singleton.instance.Inventory;

        //first, must check if there's too many items in inventory. The buttons' functionality changes if true.
        if (inv.ExtraInventoryOpen())
        {
            //swap item that was clicked with the extra item.
            inv.SwapItems(this, inv.extraItem);
        }
        else if (gm.gameState == GameManager.GameState.Combat && gm.combatManager.combatState == Combat.CombatState.Surrendering)
        {
            //clicking the item transfers the item over to the attacker.
        }
        else
        {
            //use item
            if (item is Consumable consumable)
            {
                consumable.ActivateEffect(hunter);
                hunter.inventory.Remove(item);
                ClearItemData();
                //HunterManager hm = Singleton.instance.HunterManager;
                //hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.Inventory);
                gameObject.SetActive(false);
            }
        }
    }

   
}
