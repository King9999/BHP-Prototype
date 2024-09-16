using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* Similar to ItemObject, except for skill usage.*/
public class SkillObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Skill skill;

    [Header("---UI----")]
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI detailsText;         
    public TextMeshProUGUI costText;           //cost of using a skill. Could be SP or charges
    public TextMeshProUGUI cooldownText;
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

    }

    public void OnPointerExit(PointerEventData pointer)
    {
        //remove details window
    }

    public void OnPointerEnter(PointerEventData pointer)
    {
        //display skill details
        Debug.Log("mouse is hovering on " + skillNameText.text);
    }
}
