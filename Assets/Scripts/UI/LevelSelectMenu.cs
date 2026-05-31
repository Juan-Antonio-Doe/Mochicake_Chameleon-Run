using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour, IEndDragHandler {

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; }

    [field: Header("References")]
    [field: SerializeField] private ScrollRect scrollRect { get; set; }
    [field: SerializeField] private GameObject levelButtonPrefab { get; set; }

    [field: Header("Settings")]
    [field: SerializeField] private int firstLevelSceneIndex { get; set; } = 1;
    [field: SerializeField] private float snapSpeed { get; set; } = 10f;

    [field: Header("Levels")]
    [field: SerializeField] private LevelsDatabaseSO levelDatabase { get; set; }

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private int levelCount { get; set; }
    [field: SerializeField, ReadOnlyField] private int unlockedLevels { get; set; }
    [field: SerializeField, ReadOnlyField] private List<LevelButton> levelButtonInstances { get; set; } = new List<LevelButton>();

    private float buttonWidth { get; set; }
    private Coroutine snapCoroutine { get; set; }

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
        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>();

        revalidateProperties = false;
    }
#endif

    void Start() {
        unlockedLevels = PlayerPrefs.GetInt("UnlokedLevels", 0);
        levelCount = SceneManager.sceneCountInBuildSettings - firstLevelSceneIndex;

        // Force layout rebuild so viewport rect is calculated before reading its width
        Canvas.ForceUpdateCanvases();
        buttonWidth = scrollRect.viewport.rect.width / 3f;

        StartCoroutine(SpawnLevelButtonsCo());
    }

    private IEnumerator SpawnLevelButtonsCo() {
        // Add placeholder first button so the first level stays centered when selected.
        CreatePlaceholder();

        for (int i = 0; i < levelCount; i++) {
            GameObject instance = Instantiate(levelButtonPrefab, scrollRect.content);

            // Set fixed width so ContentSizeFitter can calculate total content width
            RectTransform rt = instance.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(buttonWidth, rt.sizeDelta.y);

            LevelButton levelButton = instance.GetComponent<LevelButton>();
            levelButton.Setup(i, i <= unlockedLevels, firstLevelSceneIndex, this, levelDatabase.levelDatas[i]);
            levelButton.SetScrollRect(scrollRect);

            levelButtonInstances.Add(levelButton);

            yield return null;
        }

        // Add placeholder last button for centering last level when selected.
        CreatePlaceholder();
        Canvas.ForceUpdateCanvases();

        // Auto-select last level played (fallback to last unlocked) and scroll to it.
        int savedLastPlayed = PlayerPrefs.GetInt("LastPlayedLevel", -1);
        savedLastPlayed = Mathf.Min(savedLastPlayed, unlockedLevels);

        int indexToSelect;

        if (savedLastPlayed >= 0 && savedLastPlayed < levelCount) {
            indexToSelect = Mathf.Clamp(savedLastPlayed, 0, levelButtonInstances.Count - 1);
        }
        else {
            // Fallback: select last level unlocked
            indexToSelect = Mathf.Clamp(unlockedLevels - 1, 0, levelButtonInstances.Count - 1);
        }

        GameObject toSelect = levelButtonInstances[indexToSelect].gameObject;

        // Keep a button selected so mouse/gamepad navigation doesn't break.
        EventSystem.current.SetSelectedGameObject(toSelect);

        // Center the button in the viewport.
        yield return StartCoroutine(CenterOnItemCoroutine(toSelect.GetComponent<RectTransform>()));
    }

    private void CreatePlaceholder() {
        GameObject placeholder = new GameObject("Level placeholder", typeof(RectTransform));
        placeholder.transform.SetParent(scrollRect.content, false);

        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        le.preferredWidth = buttonWidth;
        le.minWidth = buttonWidth;
        le.flexibleWidth = 0;

        RectTransform rt = placeholder.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(buttonWidth, 0);
    }

    private IEnumerator CenterOnItemCoroutine(RectTransform target) {
        yield return null;
        Canvas.ForceUpdateCanvases();
        yield return null;

        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        // Center position of the target in content's local space
        Vector2 contentLocalPos = content.localPosition;
        Vector2 targetLocalPos = content.InverseTransformPoint(target.TransformPoint(target.rect.center));
        Vector2 viewportLocalCenter = viewport.TransformPoint(viewport.rect.center);
        Vector2 viewportLocalCenterInContent = content.InverseTransformPoint(viewportLocalCenter);

        float difference = targetLocalPos.x - viewportLocalCenterInContent.x;

        Vector2 newAnchored = content.anchoredPosition - new Vector2(difference, 0);

        // Limit to content bounds
        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;
        float maxX = (contentWidth - viewportWidth) / 2f;
        float minX = -maxX;

        // The value is normalized to ensure it doesn't exceed the content bounds.
        newAnchored.x = Mathf.Clamp(newAnchored.x, minX, maxX);

        // Smoothly animate the scroll transition.
        float t = 0f;
        float duration = 0.25f;
        Vector2 startAnchored = content.anchoredPosition;
        while (t < duration) {
            t += Time.unscaledDeltaTime;
            content.anchoredPosition = Vector2.Lerp(startAnchored, newAnchored, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        content.anchoredPosition = newAnchored;
    }

    // -- Scroll Snap -------------------------------------------

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

        int sceneIndex = firstLevelSceneIndex + level;
        if (sceneIndex < SceneManager.sceneCountInBuildSettings)
            LoadScene.Load(sceneIndex);
        else
            Debug.LogError($"Scene index {sceneIndex} is out of range.");
    }

    public void UpdateLevelData() {
        unlockedLevels = PlayerPrefs.GetInt("UnlokedLevels", 0);
        foreach (LevelButton lBtn in levelButtonInstances) {
            lBtn.RefreshData(lBtn.level <= unlockedLevels);
        }
    }
}