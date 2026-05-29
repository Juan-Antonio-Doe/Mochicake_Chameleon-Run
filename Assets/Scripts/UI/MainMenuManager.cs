using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainMenuManager : MonoBehaviour {

    [field: Header("AutoAttach on Editor properties")]
    [field: SerializeField] private bool revalidateProperties { get; set; }
    [field: SerializeField, ReadOnlyField] private GameObject mainMenu { get; set; }
    [field: SerializeField, ReadOnlyField] private GameObject levelSelectMenu { get; set; }

#if UNITY_EDITOR
    void OnValidate() {

        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

        if (!isValidPrefabStage && prefabConnected) {
            //Variables que solo se verificaran cuando están en una escena cuando se active el trigger `revalidateProperties` para evitar bucles en el editor.
            if (revalidateProperties)
                AssignComponents();
        }
    }

    void AssignComponents() {
        if (mainMenu == null)
            mainMenu = transform.GetChild(0).GetChild(0).gameObject;

        if (levelSelectMenu == null)
            levelSelectMenu = transform.GetChild(0).GetChild(2).gameObject;

        revalidateProperties = false;
    }
#endif

    void Awake() {
        mainMenu.SetActive(true);

        if (LoadingData.previousScene != null)
            if (LoadingData.previousScene.Contains("Level_") || LoadingData.previousScene.Contains("Test")) {
                levelSelectMenu.SetActive(true);
                mainMenu.SetActive(false);
            }
    }
}