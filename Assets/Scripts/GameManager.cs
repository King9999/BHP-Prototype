using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* handles the game state */
public class GameManager : MonoBehaviour
{
    //hunter stats
    public TextMeshProUGUI hunterName;
    public TextMeshProUGUI hunterStr, hunterVit, hunterMnt, hunterSpd, hunterAtp, hunterDfp, hunterMnp, hunterRst, hunterEvd, hunterHp, hunterSp;
    public Hunter hunterPrefab, hunter;

    // Start is called before the first frame update
    void Start()
    {
        CreateHunter();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateHunter()
    {
        hunter = Instantiate(hunterPrefab);
        hunter.name = "King";
        hunter.InitializeStats();
        hunterStr.text = hunter.str.ToString();
        hunterSpd.text = hunter.spd.ToString();
        hunterVit.text = hunter.vit.ToString();
        hunterMnt.text = hunter.mnt.ToString();
        hunterName.text = hunter.name.ToString();
        hunterAtp.text = hunter.atp.ToString();
        hunterDfp.text = hunter.dfp.ToString();
        hunterMnp.text = hunter.mnp.ToString();
        hunterRst.text = hunter.rst.ToString();
        hunterEvd.text = (hunter.evd * 100) + "%";
        hunterHp.text = hunter.healthPoints + "/" + hunter.maxHealthPoints;
        hunterSp.text = hunter.skillPoints + "/" + hunter.maxSkillPoints;
    }
}
