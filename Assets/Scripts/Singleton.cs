using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Profiling;

public class Singleton : MonoBehaviour
{
    public static Singleton instance { get; private set; }
    public GameManager GameManager { get; set; }
    //public UI UI { get; set; }
    //public AudioManager AudioManager { get; private set; }
    //public TitleManager TitleManager {get; private set;}

    //save state data
    //public GameData gameData;
    //public SaveState saveState;
    //public bool saveStateFound;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Application.runInBackground = false;

        //PlayerPrefs.DeleteAll();

        DontDestroyOnLoad(instance);
    }
}
