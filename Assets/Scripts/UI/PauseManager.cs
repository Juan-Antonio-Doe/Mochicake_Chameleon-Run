using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour {

    public static bool onPause { get; private set; }

    [field: Header("General pause properties")]
    [field: SerializeField] private bool revalidateProperties { get; set; }

    [field: Header("UI properties")]
    [field: SerializeField] private GameObject pauseMenu { get; set; }
    [field: SerializeField] private float cooldownBeforeResume { get; set; } = 1f;
    //[field: SerializeField] private GameObject backgroundCanvas { get; set; }
    [field: SerializeField] private Button firstSelectedButton { get; set; }
    //[field: SerializeField] private SettingsManager _settings { get; set; }
    [SerializeField, Tooltip("Objects to disable when closing the pause menu.")]
    private List<GameObject> _objectsToDisable = new();

    [field: Header("Settings UI")]
    [field: SerializeField] private Text inputLayoutText { get; set; }
    [field: SerializeField] private InputSettings inputSettings { get; set; }

    [field: Header("Animation properties")]
    [field: SerializeField, ReadOnlyField] private Animator menuAnim { get; set; }
    [field: SerializeField, ReadOnlyField] private GameObject unpauseCircleGO { get; set; }

    /*[field: Header("Audio properties")]
    [field: SerializeField] private AudioSource menuAudioSource { get; set; }*/

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private int sceneIndexToLoad { get; set; }
    [field: SerializeField, ReadOnlyField] private GameObject lastBtnSelected { get; set; }


#if UNITY_EDITOR

    void OnValidate() {
        if (!Application.isPlaying) {
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
            bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;
            if (!isValidPrefabStage && prefabConnected) {
                if (revalidateProperties)
                    ValidateAssings();
            }
        }
    }

    void ValidateAssings() {

        if (menuAnim == null) {
            menuAnim = GetComponentInChildren<Animator>();
            unpauseCircleGO = menuAnim.transform.GetChild(0).gameObject;
        }

        revalidateProperties = false;
    }
#endif

    void Start() {
        if (inputLayoutText && inputSettings)
            inputLayoutText.text = inputSettings.JumpMode == InputMode.HalfLeft ? "J | C" : "C | J";
    }

    void Update() {
        if (onPause) {
            // Select the last selected button if no button is selected (for keyboard/gamepad navigation).
            if (EventSystem.current.currentSelectedGameObject is null && lastBtnSelected is not null) {
                if (EventSystem.current.currentSelectedGameObject != lastBtnSelected) {
                    if (lastBtnSelected.activeInHierarchy)
                        EventSystem.current.SetSelectedGameObject(lastBtnSelected);
                }
            }
            else {
                lastBtnSelected = EventSystem.current.currentSelectedGameObject;
            }
        }
    }

    // Called by the pause button in the scene.
    public void Pause() {
        StartCoroutine(onPauseCo());
    }

    IEnumerator onPauseCo() {
        if (!onPause) {
            onPause = true;
            Time.timeScale = 0;
            EnableDisableThings(true);
            yield return null;
        }
        else {
            EnableDisableThings(false);

            // Play circle animation
            unpauseCircleGO.SetActive(true);
            menuAnim.Play("UnpauseCooldownAnim");
            yield return new WaitForSecondsRealtime(cooldownBeforeResume);
            // Stop circle animation
            unpauseCircleGO.SetActive(false);
            GeneralUtilities.ResetAnimators(new List<Animator>() { menuAnim }, this);

            onPause = false;
            Time.timeScale = 1;
        }
    }

    #region Buttons Methods
    // Called by Restart Button.
    public void RestartLevel() {
        //LoadScene.Load(LoadScene.CurrentIndexScene());
        sceneIndexToLoad = LoadScene.CurrentIndexScene();
        ChangeScene();
    }

    // Called by Exit_btn Button.
    public void BackToLevelMenu() {
        //LoadScene.Load(0);
        sceneIndexToLoad = 0;
        ChangeScene();
    }
    #endregion

    #region Utility methods
    public void EnableDisableThings(bool enablePause) {
        pauseMenu.SetActive(enablePause);
        //backgroundCanvas?.SetActive(enablePause);

        if (!enablePause) {
            GeneralUtilities.EnableDisableObjects(false, _objectsToDisable, this);
        }

        if (enablePause) {
            EventSystem.current.SetSelectedGameObject(null);
            firstSelectedButton?.Select();
        }

        LevelManager.Instance.hud.SetActive(!enablePause);
    }

    public void ChangeScene() {
        onPause = false;
        LoadScene.Load(sceneIndexToLoad);
    }

    public void SetLastBtnSelected(GameObject btn) {
        if (!onPause)
            return;

        lastBtnSelected = btn;
        EventSystem.current.SetSelectedGameObject(btn);
    }
    #endregion
}