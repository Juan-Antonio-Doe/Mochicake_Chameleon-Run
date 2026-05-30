using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

    // Static Properties
    public static LevelManager Instance { get; private set; }
    //public static bool IsLevelOnGoing { get; private set; }


    [field: Header("Autoattach On Editor Properties")]
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;
    [field: SerializeField, ReadOnlyField] public PlayerController playerController { get; private set; }
    [field: SerializeField, ReadOnlyField] public ColorManager colorManager { get; private set; }
    //[field: SerializeField, ReadOnlyField] public ObjectPool objectPool { get; private set; }
    //[field: SerializeField, ReadOnlyField] public PauseManager pauseManager { get; private set; }

    [field: Header("Level Properties")]
    [field: SerializeField] private Timer levelTimer { get; set; } = new Timer();


    [field: Header("Level UI Properties")]
    [field: SerializeField] public GameObject hud { get; set; }
    [field: SerializeField] private Text timerText { get; set; }

    /*[field: Header("End Level")]
    [field: SerializeField] private UnityEvent onLevelEnd { get; set; }

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private bool isLevelOnGoingDebug { get; set; }*/

#if UNITY_EDITOR
    /*
     * Suelo usar este metodo para automatizar la asignacion de propiedades en el inspector en tiempo de edicion.
     * Este codigo se ejecuta cuando se modifica un componente en el inspector. La propiedad `revalidateProperties`
     * sirve para evitar que el codigo se ejecute constantemente. Se podria considerar dicho bool como un trigger.
     */

    void OnValidate() {
        if (!Application.isPlaying) {

            // Codigo que evita que el OnValidate se ejecute en Prefab Stages provocando bucles en el editor.
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
            bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

            if (!isValidPrefabStage && prefabConnected) {
                if (revalidateProperties)
                    AssingOnValidate();
            }
        }
    }

    void AssingOnValidate() {
        // Code to execute when revalidating properties

        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (colorManager == null)
            colorManager = FindObjectOfType<ColorManager>();

        revalidateProperties = false;
    }
#endif

    void Awake() {
        if (!Instance)
            Instance = this;
    }

    void OnEnable() => GameEvents.OnPlayerFailed += ResetLevel;
    void OnDisable() => GameEvents.OnPlayerFailed -= ResetLevel;

    void Start() {
        levelTimer.Start();
    }

    void Update() {
        levelTimer.Tick();
        timerText.text = GeneralUtilities.FormatTime(levelTimer.CurrentTime, TimeFormat.SecondsCentiseconds);
    }

    // Called by EndTrigger at the end of the level (on editor).
    public void EndLevel() {
        levelTimer.Stop();
        
        SaveLevelStats();
        LoadScene.Load(0);
    }

    // Called by FailTrigger collider on level scenes (on editor).
    public void ResetLevel() {
        levelTimer.Reset();
        playerController.resetPlayer();
    }

    void SaveLevelStats() {
        string sceneName = LoadScene.CurrentNameScene();
        int number = -1;
        string[] parts = sceneName.Split('_');

        // If the index of the current scene is greater than the index where the numbered levels begin...
        if (LoadScene.CurrentIndexScene() >= 1) {
            if (parts[0].Equals("Test")) {
                number = -1;
            }
            else
                number = int.Parse(parts[parts.Length - 1]);
        }
        else
            return;

        // If the current scene number is greater than the stored one, it's updated.
        //Debug.Log($"Number: {number} >= UnlokedLevels: {PlayerPrefs.GetInt("UnlokedLevels", 0)}");
        if (number >= PlayerPrefs.GetInt("UnlokedLevels", 0))
            PlayerPrefs.SetInt("UnlokedLevels", number + 1);


        // ToDo: Save level stats like collectibles collected to PlayerPrefs.

        if (PlayerPrefs.GetFloat($"Level_{number}_Time", 99f) > levelTimer.CurrentTime)
            PlayerPrefs.SetFloat($"Level_{number}_Time", levelTimer.CurrentTime);

        PlayerPrefs.Save();
    }
}