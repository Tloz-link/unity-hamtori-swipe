using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    public static Managers Instance { get { Init(); return s_instance; } }

    DataManager _data = new DataManager();
    PoolManager _pool = new PoolManager();
    GameManagerEX _game = new GameManagerEX();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEX _scene = new SceneManagerEX();
    SoundManager _sound;
    UIManager _ui = new UIManager();

    public static DataManager Data { get { return Instance._data; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static GameManagerEX Game { get { return Instance._game; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static SoundManager Sound
    { 
        get 
        {
            if (Instance._sound == null)
                Instance._sound = s_instance.AddComponent<SoundManager>();
            return Instance._sound;
        }
    }
    public static SceneManagerEX Scene { get { return Instance._scene; } }

    public static string _savePath;

    void Start()
    {
        _savePath = Application.persistentDataPath + "/SaveData.json";
    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject{ name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            if (s_instance._sound == null)
                s_instance._sound = s_instance.AddComponent<SoundManager>();

            s_instance._data.Init();
            s_instance._sound.Init();
            s_instance._pool.Init();

            DOTween.Init(false, false, LogBehaviour.Default).SetCapacity(750, 250);
            Application.targetFrameRate = 60;
        }
    }

    public static void Clear()
    {
        Sound.Clear();
        Scene.Clear();
        UI.Clear();

        Pool.Clear();
    }
}
