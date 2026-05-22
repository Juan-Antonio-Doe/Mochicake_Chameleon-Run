using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility to use SceneManagement more easily (Wihtout importing the namespace).
/// </summary>

public static class LoadScene {

    /// <summary> Load a scene by name. </summary>
    public static void Load(string nextScene) {
        CommonLoad();
        SceneManager.LoadScene(LoadingData.sceneToLoad = nextScene);
    }

    public static string CurrentNameScene() {
        return SceneManager.GetActiveScene().name;
    }

    public static int CurrentIndexScene() {
        return SceneManager.GetActiveScene().buildIndex;
    }

    private static void CommonLoad() {
        LoadingData.previousScene = CurrentNameScene();
        Time.timeScale = 1;
        //PauseManager.SetOnPause(false);
    }

    /// <summary> Load a scene by index. </summary>
    public static void Load(int sceneIndex) {
        CommonLoad();
        LoadingData.sceneToLoad = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary> Load the next level in the build settings. </summary>
    public static void LoadNextLevel() {
        int index = SceneManager.GetActiveScene().buildIndex + 1;
        if (IsValidScene(index)) {
            CommonLoad();
            SceneManager.LoadScene(index);
        }
        else {
            SceneManager.LoadScene(LoadingData.sceneToLoad = SceneManager.GetSceneByBuildIndex(0).name);
        }
    }

    /// <summary> Check if the scene index is valid. </summary>
    public static bool IsValidScene(int index) {
        //Debug.Log($"Scene {index} must be in count: {SceneManager.sceneCountInBuildSettings}");
        //Debug.Log($"Scene is Valid: {SceneManager.GetSceneByBuildIndex(index).IsValid()}");

        // Comment IsValid because is a shit that only return true if the scene is loaded.
        if ((index < SceneManager.sceneCountInBuildSettings)/* && SceneManager.GetSceneByBuildIndex(index).IsValid()*/)
            return true;
        else
            return false;
    }

    private static AsyncOperation asyncOperation;

    /// <summary> Preload a scene by name. </summary>
    public static void PreloadScene(string sceneName) {
        asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
    }

    /// <summary> Preload a scene by index. </summary>
    public static void PreloadScene(int sceneIndex) {
        asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        asyncOperation.allowSceneActivation = false;
    }

    /// <summary> Load the scene preloaded. </summary>
    public static void LoadPreloadedScene() {
        if (asyncOperation != null)
            asyncOperation.allowSceneActivation = true;
        else {
            Debug.LogWarning("There is no scene preloaded.");
        }
    }
}
