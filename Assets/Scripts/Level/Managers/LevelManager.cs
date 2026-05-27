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
     * Suelo usar este método para automatizar la asignación de propiedades en el inspector en tiempo de edición.
     * Este código se ejecuta cuando se modifica un componente en el inspector. La propiedad `revalidateProperties`
     * sirve para evitar que el código se ejecute constantemente. Se podría considerar dicho bool como un trigger.
     */

    void OnValidate() {
        if (!Application.isPlaying) {

            // Código que evita que el OnValidate se ejecute en Prefab Stages provocando bucles en el editor.
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

    public void OnTimerComplete() {
        Debug.Log("Time is up!");
    }

    void ResetLevel() {
        levelTimer.Reset();
        playerController.resetPlayer();
    }
}