using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* Similar to ItemObject, except for skill usage.*/
public class SkillObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ActiveSkill skill;

    [Header("---UI----")]
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI detailsText;         
    public TextMeshProUGUI costText;           //cost of using a skill. Could be SP or charges
    public TextMeshProUGUI cooldownText, rangeText;
    public TextMeshProUGUI dmgModText;          //dmg effectiveness.
    public Image skillImage, skillBackground;
    Color highlightColor, normalColor;
    //bool showSkillDetails;

    // Start is called before the first frame update
    void Start()
    {
        highlightColor = new Color(128, 0, 0, 0.5f);
        normalColor = skillBackground.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //mouse button function
    public void OnSkillSelected()
    {
        GameManager gm = Singleton.instance.GameManager;
        HunterManager hm = Singleton.instance.HunterManager;
        gm.selectedSkill = skill;
        gm.GetSkillRange();
        hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.SelectSkillTile);
        //gm.ShowSkillRange(gm.ActiveCharacter(), skill.minRange, skill.maxRange);
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        skillBackground.color = normalColor;
        if (skill != null)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.SkillMenu);
        }
    }

    public void OnPointerEnter(PointerEventData pointer)
    {
        //display skill details
        skillBackground.color = highlightColor;
        if (skill != null)
        {
            HunterManager hm = Singleton.instance.HunterManager;
            hm.ChangeHunterMenuState(hm.hunterMenuState = HunterManager.HunterMenuState.SkillDetails);
            GetDetails(skill);
        }
        
        Debug.Log("mouse is hovering on " + skillNameText.text);
    }

    private void GetDetails(Skill skill)
    {
        HunterManager hm = Singleton.instance.HunterManager;
        hm.ui.skillDetailsText.text = skill.skillDetails;

        //more skill info.
        hm.ui.skillDetailsText.text += "\n-------\n";

    }
}
