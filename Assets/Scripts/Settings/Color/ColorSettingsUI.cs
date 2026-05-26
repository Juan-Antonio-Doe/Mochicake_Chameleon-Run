using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ColorSettingsUI : MonoBehaviour {

    private const string prefKeyA = "ColorA_Index";
    private const string prefKeyB = "ColorB_Index";

    private static readonly Vector3 selectedScale = Vector3.one * 1.15f;
    private static readonly Vector3 normalScale = Vector3.one;

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;

    [field: Header("References")]
    [field: SerializeField, ReadOnlyField] private ColorManager colorManager { get; set; }
    [field: SerializeField] private ColorPresets presets { get; set; }

    [field: Header("Color A Buttons")]
    [field: SerializeField] private Text colorAButtonParent { get; set; }
    [field: SerializeField, ReadOnlyField] private Button[] colorAButtons { get; set; } = new Button[3];

    [field: Header("Color B Buttons")]
    [field: SerializeField] private Text colorBButtonParent { get; set; }
    [field: SerializeField, ReadOnlyField] private Button[] colorBButtons { get; set; } = new Button[3];

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private int selectedAIndex { get; set; }
    [field: SerializeField, ReadOnlyField] private int selectedBIndex { get; set; }

#if UNITY_EDITOR
    void OnValidate() {
        if (!Application.isPlaying) {
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
            bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

            if (!isValidPrefabStage && revalidateProperties)
                AssingOnValidate();
        }
    }

    void AssingOnValidate() {
        if (colorManager == null)
            colorManager = FindObjectOfType<ColorManager>();

        if (colorAButtonParent != null)
            colorAButtons = colorAButtonParent.GetComponentsInChildren<Button>();

        if (colorBButtonParent != null)
            colorBButtons = colorBButtonParent.GetComponentsInChildren<Button>();

        revalidateProperties = false;
    }
#endif

    void Awake() {
        SetupButtons();
    }

    void Start() {
        // Load saved indices, defaulting to 0 for both
        SelectColorA(PlayerPrefs.GetInt(prefKeyA, 0), save: false);
        SelectColorB(PlayerPrefs.GetInt(prefKeyB, 0), save: false);
    }

    private void SetupButtons() {
        for (int i = 0; i < colorAButtons.Length; i++) {
            colorAButtons[i].image.color = presets.GetColorA(i);
            int index = i; // Capture for lambda
            colorAButtons[i].onClick.AddListener(() => SelectColorA(index));
        }

        for (int i = 0; i < colorBButtons.Length; i++) {
            colorBButtons[i].image.color = presets.GetColorB(i);
            int index = i;
            colorBButtons[i].onClick.AddListener(() => SelectColorB(index));
        }
    }

    private void SelectColorA(int index, bool save = true) {
        colorAButtonParent.color = presets.GetColorA(index);
        UpdateSelectionVisual(colorAButtons, selectedAIndex, index);
        selectedAIndex = index;

        colorManager.colorSettings.SetColorA(presets.GetColorA(index));
        colorManager.ApplyColors();

        if (!save) return;
        PlayerPrefs.SetInt(prefKeyA, index);
        PlayerPrefs.Save();
    }

    private void SelectColorB(int index, bool save = true) {
        colorBButtonParent.color = presets.GetColorB(index);
        UpdateSelectionVisual(colorBButtons, selectedBIndex, index);
        selectedBIndex = index;

        colorManager.colorSettings.SetColorB(presets.GetColorB(index));
        colorManager.ApplyColors();

        if (!save) return;
        PlayerPrefs.SetInt(prefKeyB, index);
        PlayerPrefs.Save();
    }

    // Updates only the two affected buttons instead of looping through all
    private void UpdateSelectionVisual(Button[] buttons, int oldIndex, int newIndex) {
        buttons[oldIndex].transform.localScale = normalScale;
        buttons[newIndex].transform.localScale = selectedScale;
    }
}