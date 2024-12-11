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
    public static InventoryManager im;
    public static StageManager stm;
    
    // Other Managers
    public static PlayManager pm;

    public Transform controller;
    
    // Singleton pattern & Find SoundManager
    private void Awake()
    {
        if (gm == null)
        {
            gm = this;
            DontDestroyOnLoad(gameObject);
            
            sm = GetComponentInChildren<SoundManager>();
            em = GetComponentInChildren<EffectManager>();
            im = GetComponentInChildren<InventoryManager>();
            stm = GetComponentInChildren<StageManager>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (gm != this)
        {
            Destroy(gameObject);
        }
        
        // Safe check for "Controller" object
        GameObject controllerObject = GameObject.Find("Controller");
        if (controllerObject != null)
        {
            controller = controllerObject.transform;
        }
        else
        {
            Debug.LogWarning("Controller object not found in the scene. Setting controller to null.");
            controller = null; // Prevent NullReferenceException
        }
    }
    
    // Called every time when some scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pm = FindObjectOfType<PlayManager>();
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null) {
            pm.playerTransform = playerMovement.transform;
        }
        
        GameObject controllerObject = GameObject.Find("Controller");
        if (controllerObject != null)
        {
            controller = controllerObject.transform;
        }

        if (GameObject.FindWithTag("Player"))
        {
            em.FadeOutCircleTransition();
        }
        else
        {
            em.NoEffectOnCt();
        }
        // locks cursor and makes it invisible
        Cursor.lockState = Cursor.lockState;
        Cursor.visible = false;
        
        im.OnSceneLoaded();
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
