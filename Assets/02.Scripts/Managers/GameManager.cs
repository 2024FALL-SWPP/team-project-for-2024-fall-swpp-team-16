using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // GameManager itself (Singleton)
    public static GameManager gm { get; private set; }
    
    // DontDestroyOnLoad Managers
    public static SoundManager sm;
    public static EffectManager em;
    
    // Other Managers
    public static PlayManager pm;

    public Transform Player;
    
    // Singleton pattern & Find SoundManager
    private void Awake()
    {
        if (gm == null)
        {
            gm = this;
            DontDestroyOnLoad(gameObject);
            
            sm = GetComponentInChildren<SoundManager>();
            em = GetComponentInChildren<EffectManager>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (gm != this)
        {
            Destroy(gameObject);
        }
    }
    
    // Called every time when some scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pm = FindObjectOfType<PlayManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    # region Test
    // Update is called once per frame
    void Update()
    {
        
    }
    # endregion
}
