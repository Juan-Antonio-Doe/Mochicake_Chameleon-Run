using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour, IDragHandler, IEndDragHandler {

    private const string prefKeyUnlocked = "UnlockedLevels";

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; }

    [field: Header("References")]
    [field: SerializeField] private ScrollRect scrollRect { get; set; }
    [field: SerializeField] private GameObject levelButtonPrefab { get; set; }

    [field: Header("Settings")]
    [field: SerializeField] private int firstLevelSceneIndex { get; set; } = 1;
    [field: SerializeField] private float snapSpeed { get; set; } = 10f;

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private int levelCount { get; set; }
    [field: SerializeField, ReadOnlyField] private int unlockedLevels { get; set; }

    private float buttonWidth { get; set; }
    private Coroutine snapCoroutine { get; set; }

#if UNITY_EDITOR
    void OnValidate() {
        if (!Application.isPlaying) {
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
            //bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

            if (!isValidPrefabStage/* && prefabConnected*/) {
                if (revalidateProperties)
                    ValidateAssings();
            }
        }
    }

    void ValidateAssings() {
        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>();

        revalidateProperties = false;
    }
#endif

    void Start() {
        unlockedLevels = PlayerPrefs.GetInt(prefKeyUnlocked, 1);
        levelCount = SceneManager.sceneCountInBuildSettings - firstLevelSceneIndex;

        // Force layout rebuild so viewport rect is calculated before reading its width
        Canvas.ForceUpdateCanvases();
        buttonWidth = scrollRect.viewport.rect.width / 3f;

        StartCoroutine(SpawnLevelButtonsCo());
    }

    private IEnumerator SpawnLevelButtonsCo() {
        for (int i = 0; i < levelCount; i++) {
            GameObject instance = Instantiate(levelButtonPrefab, scrollRect.content);

            // Set fixed width so ContentSizeFitter can calculate total content width
            RectTransform rt = instance.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(buttonWidth, rt.sizeDelta.y);

            int level = i + 1;
            instance.GetComponent<LevelButton>().Setup(level, level <= unlockedLevels, firstLevelSceneIndex, this);

            yield return null;
        }
    }

    // -- Scroll Snap -------------------------------------------

    public void OnDrag(PointerEventData eventData) {
        Debug.Log("Dragging");
    }

    public void OnEndDrag(PointerEventData eventData) {
        float currentX = scrollRect.content.anchoredPosition.x;
        float targetX = Mathf.Round(currentX / buttonWidth) * buttonWidth;

        // Clamp to valid scroll range
        float minX = -(scrollRect.content.rect.width - scrollRect.viewport.rect.width);
        targetX = Mathf.Clamp(targetX, minX, 0f);

        if (snapCoroutine != null) StopCoroutine(snapCoroutine);
        snapCoroutine = StartCoroutine(SmoothSnap(targetX));
    }

    private IEnumerator SmoothSnap(float targetX) {
        // Disable scroll inertia during snap to avoid fighting with it
        scrollRect.inertia = false;

        Vector2 contentPos = scrollRect.content.anchoredPosition;

        while (Mathf.Abs(contentPos.x - targetX) > 0.5f) {
            contentPos.x = Mathf.Lerp(contentPos.x, targetX, Time.deltaTime * snapSpeed);
            scrollRect.content.anchoredPosition = contentPos;
            yield return null;
        }

        contentPos.x = targetX;
        scrollRect.content.anchoredPosition = contentPos;

        scrollRect.inertia = true;
    }

    // -- Public ------------------------------------------------

    public void LoadLevel(int level) {
        if (level > unlockedLevels) return;

        int sceneIndex = firstLevelSceneIndex + level - 1;
        if (sceneIndex < SceneManager.sceneCountInBuildSettings)
            LoadScene.Load(sceneIndex);
        else
            Debug.LogError($"Scene index {sceneIndex} is out of range.");
    }

    public void UnlockNextLevel(int completedLevel) {
        if (completedLevel < unlockedLevels) return;
        unlockedLevels = completedLevel + 1;
        PlayerPrefs.SetInt(prefKeyUnlocked, unlockedLevels);
        PlayerPrefs.Save();
    }
}