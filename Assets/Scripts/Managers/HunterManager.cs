using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.XR;

/* This script handles hunter creation. The UI for hunter setup is here. */
public class HunterManager : MonoBehaviour
{
    public HunterUI ui;
    public List<Hunter> hunters;

    public enum MenuState { PointAlloc, ChooseWeapon }
    public MenuState state;

    public static HunterManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Singleton.instance.HunterManager = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //Singleton.instance.HunterManager = this;
        state = MenuState.PointAlloc;
        ChangeState(state);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeState(MenuState state)
    {
        switch(state)
        {
            case MenuState.PointAlloc:
                
                break;

            case MenuState.ChooseWeapon:
                ui.ShowPointAllocationMenu(false);
                
                break;
        }
    }
}