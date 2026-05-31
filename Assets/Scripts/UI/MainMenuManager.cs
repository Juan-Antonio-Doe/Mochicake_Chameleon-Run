using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

    [field: Header("AutoAttach on Editor properties")]
    [field: SerializeField] private bool revalidateProperties { get; set; }
    [field: SerializeField, ReadOnlyField] private GameObject mainMenu { get; set; }
    [field: SerializeField, ReadOnlyField] private GameObject levelSelectMenuGO { get; set; }
    [field: SerializeField, ReadOnlyField] private LevelSelectMenu levelSelectMenu {  get; set; }

    [field: Header("Settings UI")]
    [field: SerializeField] private Text inputLayoutText { get; set; }
    [field: SerializeField] private InputSettings inputSettings { get; set; }

#if UNITY_EDITOR
    void OnValidate() {

        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

        if (!isValidPrefabStage && prefabConnected) {
            //Variables que solo se verificaran cuando est�n en una escena cuando se active el trigger `revalidateProperties` para evitar bucles en el editor.
            if (revalidateProperties)
                AssignComponents();
        }
    }

    void AssignComponents() {
        if (mainMenu == null)
            mainMenu = transform.GetChild(0).GetChild(0).gameObject;

        if (levelSelectMenuGO == null)
            levelSelectMenuGO = transform.GetChild(0).GetChild(2).gameObject;

        if (levelSelectMenu == null)
            levelSelectMenu = GetComponent<LevelSelectMenu>();

        revalidateProperties = false;
    }
#endif

    void Awake() {
        mainMenu.SetActive(true);

        if (LoadingData.previousScene != null)
            if (LoadingData.previousScene.Contains("Level_") || LoadingData.previousScene.Contains("Test")) {
                levelSelectMenuGO.SetActive(true);
                mainMenu.SetActive(false);
            }

        if (inputLayoutText && inputSettings)
            inputLayoutText.text = inputSettings.JumpMode == InputMode.HalfLeft ? "J | C" : "C | J";
    }

    void Start() {
        GameEvents.StartCleanup(this);
    }

    // Called by 'ClearData_Btn' on Settings Menu UI.
    public void ClearPlayerPrefsData() {
        GeneralUtilities.DeletePlayerPrefs();
        levelSelectMenu.UpdateLevelData();
    }
}