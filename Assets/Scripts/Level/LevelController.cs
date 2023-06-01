using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    private bool isGameActive;
    private bool isPlayerDead;
    private bool isLevelOver;

    private Transform monsters;
    private Transform monsterFactories;

    private bool isTeleportsDisabled;
    private int teleportsDisabledBy;

    private bool isInstructionsScreenActive;
    private bool isPauseScreenActive;
    
    private SoundManager soundManager;
    private SoundManager.Bool isPlayingBackgroundMusic;
    private SoundManager.Bool isPlayingDefeatMusic;
    private SoundManager.Bool isPlayingVictoryMusic;
    [SerializeField]
    private AudioClip backgroundMusic;
    [SerializeField]
    private AudioClip defeatMusic;
    [SerializeField]
    private AudioClip victoryMusic;

    private Canvas levelInstructionScreen;
    private Canvas pauseMenu;
    private Canvas victoryScreen;
    private Canvas defeatScreen;

    private int currentSceneID;

    private void Awake()
    {
        isGameActive = false;
        isPlayerDead = false;
        isLevelOver = false;
        Cursor.visible = false;
        isTeleportsDisabled = false;
        isInstructionsScreenActive = true;
        isPauseScreenActive = false;

        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        isPlayingBackgroundMusic = new SoundManager.Bool(false);

        monsterFactories = GameObject.Find("MonsterFactories").transform;
        monsters = GameObject.Find("Monsters").transform;

        levelInstructionScreen = GameObject.Find("InstructionsScreen").GetComponent<Canvas>();
        pauseMenu = GameObject.Find("PauseMenu").GetComponent<Canvas>();
        victoryScreen = GameObject.Find("VictoryScreen").GetComponent<Canvas>();
        defeatScreen = GameObject.Find("DefeatScreen").GetComponent<Canvas>();

        levelInstructionScreen.sortingOrder = 5;
        pauseMenu.sortingOrder = 5;
        victoryScreen.sortingOrder = 5;
        defeatScreen.sortingOrder = 5;

        currentSceneID = SceneManager.GetActiveScene().buildIndex;
    }

    private void Start()
    {
        levelInstructionScreen.enabled = true;
        Cursor.visible = true;
    }

    void Update()
    {
        if (isGameActive)
        {
            if (IsLevelComplete())
            {
                ShowVictoryScreen();
            }
            else if (isPlayerDead)
            {
                ShowDefeatScreen();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                ShowPauseMenu();
            }

            if (!isPlayingBackgroundMusic.Value)
            {
                soundManager.PlaySoundWithoutBlocking(backgroundMusic, isPlayingBackgroundMusic);
            }
        }
        else if (isPauseScreenActive)
        {
            isPlayingBackgroundMusic.Value = false;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HidePauseMenu();
            }
        }
        else if (isInstructionsScreenActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideInstructionsScreen();
            }
        }
        
        if (isLevelOver)
        {
            if (isPauseScreenActive) HidePauseMenu();
        }
    }

    private bool IsLevelComplete()
    {
        bool levelCleared = true;

        if (monsterFactories.childCount == 0)
        {
            foreach (Transform monsterType in monsters)
            {
                if (monsterType.childCount != 0)
                {
                    levelCleared = false;
                    break;
                }
            }
        }
        else levelCleared = false;

        return levelCleared;
    }

    private void ShowPauseMenu()
    {
        soundManager.StopAllAudio();
        isGameActive = false;
        pauseMenu.enabled = true;
        isPauseScreenActive = true;
        Cursor.visible = true;
    }

    public void HidePauseMenu()
    {
        Cursor.visible = false;
        isPauseScreenActive = false;
        isGameActive = true;
        pauseMenu.enabled = false;
    }

    public void HideInstructionsScreen()
    {
        levelInstructionScreen.enabled = false;
        isGameActive = true;
        Cursor.visible = false;
    }

    private void ShowVictoryScreen()
    {
        soundManager.StopAllAudio();
        soundManager.PlaySoundWithoutBlocking(victoryMusic);
        Cursor.visible = true;
        isGameActive = false;
        victoryScreen.enabled = true;
        isLevelOver = true;
    }

    private void ShowDefeatScreen()
    {
        soundManager.StopAllAudio();
        soundManager.PlaySoundWithoutBlocking(defeatMusic);
        Cursor.visible = true;
        isGameActive = false;
        defeatScreen.enabled = true;
        isLevelOver = true;
    }

    public void ReloadCurrentLevelScene()
    {
        SceneManager.LoadSceneAsync(currentSceneID);
    }

    public void LoadNextLevelScene()
    {
        SceneManager.LoadSceneAsync(currentSceneID + 1);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync("Main Menu");
    }

    public void EnableTeleports(int instanceID)
    {
        if (instanceID == teleportsDisabledBy)
        {
            isTeleportsDisabled = false;
            Debug.Log("Teleports Enabled");
        }
    }

    public void DisableTeleports(int instanceID)
    {
        Debug.Log("Teleports Disabled");
        teleportsDisabledBy = instanceID;
        isTeleportsDisabled = true;
    }

    public bool IsGameActive
    {
        get { return isGameActive; }
    }

    public bool IsTeleportsDisabled
    {
        get { return isTeleportsDisabled; }
    }

    public bool IsPlayerDead
    {
        get { return isPlayerDead; }
        set { isPlayerDead = value; }
    }
}
