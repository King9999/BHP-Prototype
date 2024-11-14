using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

public class Singleton : MonoBehaviour
{
    public static Singleton instance { get; private set; }
    public GameManager GameManager { get; set; }
    public ItemModManager ItemModManager { get; private set; }
    public ItemManager ItemManager { get; private set; }
    public MonsterManager MonsterManager { get; private set; }
    public CardManager CardManager { get; private set; }
    public HunterManager HunterManager { get; set; }
    public EffectManager EffectManager { get; private set; }
    public SkillManager SkillManager { get; private set; }
    public HunterUI HunterUI { get; set; }
    public Dungeon Dungeon { get; private set; }
    public CardMenu CardMenu { get; set; }
    public Inventory Inventory { get; set; }
    //public UI UI { get; set; }
    //public AudioManager AudioManager { get; private set; }
    //public TitleManager TitleManager {get; private set;}

    //save state data
    //public GameData gameData;
    //public SaveState saveState;
    //public bool saveStateFound;

    public Combat Combat { get; set; }
    public Character attacker, defender;        //used to transfer character data to and from combat.

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Application.runInBackground = false;
        Debug.unityLogger.logEnabled = Debug.isDebugBuild;      //disables all calls to Debug.Log in the finished build.

        //PlayerPrefs.DeleteAll();
        ItemModManager = GetComponentInChildren<ItemModManager>();
        ItemManager = GetComponentInChildren<ItemManager>();
        MonsterManager = GetComponentInChildren<MonsterManager>();
        Dungeon = GetComponentInChildren<Dungeon>();
        CardManager = GetComponentInChildren<CardManager>();
        EffectManager = GetComponentInChildren<EffectManager>();
        SkillManager = GetComponentInChildren<SkillManager>();
        //HunterManager = GetComponentInChildren<HunterManager>();

        DontDestroyOnLoad(instance);

        //move to game scene
        SceneManager.LoadScene("Hunter Setup");
    }
}
