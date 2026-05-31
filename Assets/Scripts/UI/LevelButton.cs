using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour, IBeginDragHandler,
    IDragHandler, IEndDragHandler {

    [field: Header("AutoAttach on Editor properties")]
    [field: SerializeField] private bool revalidateProperties { get; set; }
    [field: SerializeField, ReadOnlyField] private Text levelText { get; set; }
    [field: SerializeField, ReadOnlyField] private Text levelNameText { get; set; }
    [field: SerializeField, ReadOnlyField] private Button button { get; set; }
    [field: SerializeField, ReadOnlyField] private Image image { get; set; }
    [field: SerializeField, ReadOnlyField] private Image lockImage { get; set; }
    [field: SerializeField, ReadOnlyField] private GameObject statsPanel { get; set; }

    [field: Header("Button properties")]
    [field: SerializeField, ReadOnlyField, Tooltip("Setted when instancing button")] private LevelSelectMenu levelSelectMenu { get; set; }
    [field: SerializeField, ReadOnlyField, Tooltip("Setted when instancing button")] private ScrollRect parentScrollRect { get; set; }
    [field: SerializeField, Tooltip("Level to load.")] private int level { get; set; }
    [field: SerializeField, Tooltip("Word 'level' for translations")] private string levelLabel { get; set; }
    //private int sceneIndex { get; set; }

    [field: Header("Stats properties")]
    [field: SerializeField, ReadOnlyField] private Text levelCollectiblesValueText { get; set; }
    [field: SerializeField, ReadOnlyField] private Text levelTimeValueText { get; set; }
    [field: SerializeField, ReadOnlyField] private string maxCollectibles {  get; set; }
    [field: SerializeField, ReadOnlyField] private bool isDragging { get; set; }

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
        if (button == null)
            button = GetComponent<Button>();
        if (image == null)
            image = GetComponent<Image>();

        if (levelText == null) {
            // Child 0 -> "__Btn_body" - Child 0.0 -> "LevelNumberText"
            levelText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
            levelNameText = levelText.transform.GetChild(0).GetComponent<Text>();
        }
        
        if (lockImage == null)
            lockImage = transform.GetChild(0).GetChild(3).GetComponent<Image>();

        if (statsPanel == null) {
            statsPanel = transform.GetChild(0).GetChild(2).gameObject;
            levelCollectiblesValueText = statsPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>();
            levelTimeValueText = statsPanel.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        }

        revalidateProperties = false;
    }
#endif

    public void Setup(int level, bool isUnlocked, int firtLevelScenesIndex, LevelSelectMenu levelSelectMenu, LevelData levelData) {
        this.level = level;
        //sceneIndex = firtLevelScenesIndex + level;
        levelText.text = $"{levelLabel}: {this.level}";
        levelNameText.text = levelData.levelName;
        maxCollectibles = levelData.maxCollectibles;
        button.interactable = isUnlocked;
        this.levelSelectMenu = levelSelectMenu;

        lockImage.enabled = !isUnlocked;
        statsPanel.SetActive(isUnlocked);
        LoadDataLevel();
    }

    public void OnClick() {
        if (isDragging) return;
        levelSelectMenu.LoadLevel(level);
    }

    private void LoadDataLevel() {
        levelCollectiblesValueText.text = PlayerPrefs.GetInt($"Level_{level}_CollectedTotal", 0).ToString() + "/" + maxCollectibles;
        levelTimeValueText.text = GeneralUtilities.FormatTime(PlayerPrefs.GetFloat($"Level_{level}_Time", 0f), TimeFormat.SecondsMilliseconds);
    }

    public void SetScrollRect(ScrollRect sr) {
        parentScrollRect = sr;
    }

    // --- Drag forwarding ---
    public void OnBeginDrag(PointerEventData eventData) {
        isDragging = true;
        if (parentScrollRect != null) parentScrollRect.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData) {
        if (parentScrollRect != null) parentScrollRect.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (parentScrollRect != null) parentScrollRect.OnEndDrag(eventData);
        
        StartCoroutine(ResetDragFlagNextFrame()); // Small delay to prevent an immediate click from being triggered
    }

    System.Collections.IEnumerator ResetDragFlagNextFrame() {
        yield return null;
        isDragging = false;
    }
}