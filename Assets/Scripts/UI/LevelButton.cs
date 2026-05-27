using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour {

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
    [field: SerializeField, Tooltip("Level to load.")] private int level { get; set; }
    [field: SerializeField, Tooltip("Word 'level' for translations")] private string levelLabel { get; set; }
    private int sceneIndex { get; set; }

    [field: Header("Stats properties")]
    [field: SerializeField, ReadOnlyField] private Text levelCollectiblesValueText { get; set; }
    //private int _levelScore { get; set; }
    [field: SerializeField, ReadOnlyField] private Text levelTimeValueText { get; set; }
    //private string _levelTime { get; set; }

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

    public void Setup(int level, bool isUnlocked, int firtLevelScenesIndex, LevelSelectMenu levelSelectMenu) {
        this.level = level;
        sceneIndex = firtLevelScenesIndex + level - 1;
        levelText.text = $"{levelLabel}: {this.level}";
        button.interactable = isUnlocked;
        this.levelSelectMenu = levelSelectMenu;

        lockImage.enabled = !isUnlocked;
        statsPanel.SetActive(isUnlocked);
        LoadDataLevel();
    }

    public void OnClick() {
        levelSelectMenu.LoadLevel(level);
    }

    private void LoadDataLevel() {
        //levelCollectiblesValueText.text = string.Format("{0:D10}", PlayerPrefs.GetInt($"Level_{sceneIndex}", 0));
        levelCollectiblesValueText.text = PlayerPrefs.GetInt($"Level_{sceneIndex}", 0).ToString() + "/" + "10";
        //levelTimeValueText.text = PlayerPrefs.GetString($"Level_{sceneIndex}_TimeFormatted", "0:00.000");
        levelTimeValueText.text = GeneralUtilities.FormatTime(PlayerPrefs.GetFloat($"Level_{sceneIndex}_Time", 0f), TimeFormat.MinutesSecondsMilliseconds);
    }
}