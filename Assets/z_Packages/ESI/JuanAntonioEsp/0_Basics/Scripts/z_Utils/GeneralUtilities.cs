using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GeneralUtilities {

    #region Commonly used methods

    #region Cursor methods
    /// <summary>
    /// Sets the cursor state. True for locked, false for unlocked.
    /// </summary>
    /// <param name="locked"></param>
    /// <returns></returns>
    public static void EnableDisableCursor(bool locked) {
        /*if (PauseManager.onPause)
            locked = false;*/

        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    /// <summary>
    /// Returns the current state of the cursor. True for locked, false for unlocked.
    /// </summary>
    /// <returns></returns>
    public static bool CursorState() {
        return Cursor.lockState == CursorLockMode.Locked;
    }
    #endregion

    /// <summary>
    /// Completely stops a ParticleSystem clearing all particles.
    /// </summary>
    public static void CompletelyStopParticleSystem(ParticleSystem system, bool childrens = true) {
        system.Stop(childrens, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    #region Methods for Enabling/Disabling Objects
    /// <summary>
    /// Enable or disable a list of GameObjects.
    /// </summary>
    public static void EnableDisableObjects(bool enabled, List<GameObject> objectsToDisable, MonoBehaviour coroutineExecuter = null) {
        if (coroutineExecuter == null) {
            foreach (GameObject obj in objectsToDisable) {
                obj?.SetActive(enabled);
            }
        }
        else {
            coroutineExecuter.StartCoroutine(EnableDisableObjectsCo(enabled, objectsToDisable));
        }
    }

    /// <summary>
    /// Enable or disable a list of GameObjects. Ignore the objects in the list of objects to ignore.
    /// </summary>
    public static void EnableDisableObjects(bool enabled, List<GameObject> objectsToDisable, List<GameObject> objectsToIgnore, MonoBehaviour coroutineExecuter = null) {
        if (coroutineExecuter == null) {
            foreach (GameObject obj in objectsToDisable) {
                if (!objectsToIgnore.Contains(obj))
                    obj?.SetActive(enabled);
            }
        }
        else {
            coroutineExecuter.StartCoroutine(EnableDisableObjectsCo(enabled, objectsToDisable, objectsToIgnore));
        }
    }

    static IEnumerator EnableDisableObjectsCo(bool enabled, List<GameObject> objectsToDisable, List<GameObject> objectsToIgnore = null) {
        foreach (GameObject obj in objectsToDisable) {
            if (objectsToIgnore is null || !objectsToIgnore.Contains(obj))
                obj?.SetActive(enabled);
            yield return null;
        }
    }
    #endregion

    #region Methods for animations
    #region Reset Animator methods
    /// <summary>
    /// Reset a list of Animators to their default state.
    /// </summary>
    public static void ResetAnimators(List<Animator> animators, MonoBehaviour coroutineExecuter = null) {
        if (coroutineExecuter == null) {
            foreach (Animator anim in animators) {
                if (anim is not null)
                    if (anim.gameObject.activeInHierarchy)
                        anim?.Update(0);
                anim?.Rebind();
            }
        }
        else {
            coroutineExecuter.StartCoroutine(ResetAnimatorsCo(animators));
        }
    }

    /// <summary>
    /// Reset a list of Animators to their default state. Ignore the animators in the list of animators to ignore.
    /// </summary>
    public static void ResetAnimators(List<Animator> animators, List<Animator> animatorsToIgnore, MonoBehaviour coroutineExecuter = null) {
        if (coroutineExecuter == null) {
            foreach (Animator anim in animators) {
                if (!animatorsToIgnore.Contains(anim)) {
                    if (anim is not null)
                        if (anim.gameObject.activeInHierarchy)
                            anim?.Update(0);
                    anim?.Rebind();
                }
            }
        }
        else {
            coroutineExecuter.StartCoroutine(ResetAnimatorsCo(animators, animatorsToIgnore));
        }
    }

    static IEnumerator ResetAnimatorsCo(List<Animator> animators, List<Animator> animatorsToIgnore = null) {
        foreach (Animator anim in animators) {
            if (animatorsToIgnore is null || !animatorsToIgnore.Contains(anim)) {
                if (anim is not null)
                    if (anim.gameObject.activeInHierarchy)
                        anim?.Update(0);
                anim?.Rebind();
            }
            yield return null;
        }
    }
    #endregion
    #endregion

    #region Menu Methods
    /// <summary>
    /// Manages the game shutdown on Android or Unity Standalone.
    /// </summary>
    public static void CloseGame() {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
        try {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                // Try to send it to the background first.
                activity.Call("moveTaskToBack", true);
                // Try to finish the Activity (finish)
                activity.Call("finish");
                // finishAffinity to close the entire task (API 16+)
                try {
                    activity.Call("finishAffinity");
                } catch { /* ignore si no está disponible */ }
            }
        } catch {
            // Fallback: close the process.
            Application.Quit();
        }
#else
        // Standalone (Windows, Mac, Linux) and others
        Application.Quit();
#endif
    }
    #endregion
    #endregion

    #region Vector methods
    /// <summary>
    /// Returns the distance between two Vector3 points without the square root operation.
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    public static float Vector3SqrDistance(Vector3 startPos, Vector3 targetPos) {
        return (startPos - targetPos).sqrMagnitude;
    }

    /// <summary>
    /// Returns the closest point from an array of Vector3 points to a current point.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="currentPoint"></param>
    /// <returns></returns>
    public static Vector3 GetClosestPoint(Vector3[] points, Vector3 currentPoint) {
        Vector3 pMin = Vector3.zero;
        float minDist = Mathf.Infinity;

        foreach (Vector3 p in points) {
            float dist = Vector3SqrDistance(p, currentPoint);
            if (dist < minDist) {
                pMin = p;
                minDist = dist;
            }
        }
        return pMin;
    }
    #endregion

    #region Generation Methods
    /// <summary>
    /// Returns a random number between minInclusive and maxExclusive.
    /// </summary>
    /// <param name="minInclusive"></param>
    /// <param name="maxExclusive"></param>
    /// <returns></returns>
    public static int GenerateRandomNumber(int minInclusive, int maxExclusive) {
        int seed = System.DateTime.Now.Millisecond + Time.frameCount;
        UnityEngine.Random.InitState(seed);
        return UnityEngine.Random.Range(minInclusive, maxExclusive);
    }

    /// <summary> 
    /// Generate UniqueID using ClassType and Vector3.
    /// Set directly the uniqueID property of the class passed as out parameter.
    /// </summary>
    public static void GenerateUniqueID<T>(T typeObject, Transform transform, out string uniqueID) {
        //return $"{typeObject.GetType()}_{position.x}_{position.y}_{position.z}_{System.Guid.NewGuid()}";
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        string gameObjectName = transform.gameObject.name;

        uniqueID = $"[{typeObject.GetType()}]_[{gameObjectName}]_{position.x}_{position.y}_{position.z}_{rotation.x}_{rotation.y}_{rotation.z}_{rotation.w}";
    }

    #endregion

    #region Conterter/Parsing methods
    /// <summary>
    /// Converts a LayerMask to its corresponding layer index. 
    /// Note that this method assumes the LayerMask contains only one layer.
    /// </summary>
    public static int ToLayerIndex(LayerMask mask) {
        return Mathf.RoundToInt(Mathf.Log(mask.value, 2));
    }

    /// <summary>
    /// Get the number at the end of the scene name in projects with multiples levels named "whatever_n".
    /// </summary>
    public static int GetLevelNumber() {
        string[] parts = LoadScene.CurrentNameScene().Split('_');

        if (parts.Length > 0) {
            return int.Parse(parts[parts.Length - 1]);
        }

        return 0;
    }

    /// <summary>
    /// Formats a time value in seconds to a string based on the specified format.
    /// </summary>
    /// <param name="time">Time in seconds.</param>
    /// <param name="format">The desired display format.</param>
    public static string FormatTime(float time, TimeFormat format = TimeFormat.MinutesSeconds) {
        return format switch {
            TimeFormat.SecondsOnly =>
                $"{(int)time:00}",

            TimeFormat.SecondsMilliseconds =>
                $"{(int)time:00}.{(int)(time * 1000f) % 1000:000}",

            TimeFormat.SecondsCentiseconds =>
                $"{(int)time:00}.{(int)(time * 100f) % 100:00}",

            TimeFormat.MinutesSeconds =>
                $"{(int)(time / 60f):00}:{(int)time % 60:00}",

            TimeFormat.MinutesSecondsMilliseconds =>
                $"{(int)(time / 60f):00}:{(int)time % 60:00}.{(int)(time * 1000f) % 1000:000}",

            TimeFormat.MinutesSecondsCentiseconds =>
                $"{(int)(time / 60f):00}:{(int)time % 60:00}.{(int)(time * 100f) % 100:00}",

            TimeFormat.HoursMinutesSeconds =>
                $"{(int)(time / 3600f):00}:{(int)(time % 3600f / 60f):00}:{(int)time % 60:00}",

            _ => $"{(int)(time / 60f):00}:{(int)time % 60:00}"
        };
    }
    #endregion

    #region PlayerPrefs Utilities
    /// <summary>
    /// Delete PlayerPrefs.
    /// </summary>
#if UNITY_EDITOR
    [MenuItem("Tools/PlayerPrefs/Delete PlayerPrefs")]
#endif
    public static void DeletePlayerPrefs() {
        PlayerPrefs.DeleteAll();
    }

#if UNITY_EDITOR
    private static string Company => PlayerSettings.companyName ?? "UnknownCompany";
    private static string Product => PlayerSettings.productName ?? "UnknownProduct";

    /// <summary>
    /// Open PlayerPrefs location.
    /// </summary>
    [MenuItem("Tools/PlayerPrefs/Open PlayerPrefs location")]
    public static void OpenPlayerPrefsFolder() {
        // Determine platform and build the expected path or registry key.
        if (Application.platform == RuntimePlatform.OSXEditor) {
            // macOS: ~/Library/Preferences/unity.<Company>.<Product>.plist
            string plistFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library", "Preferences",
                $"unity.{Company}.{Product}.plist"
            );

            OpenPathOrShow(plistFile);
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor) {
            // Windows: PlayerPrefs are stored in the registry under HKCU\Software\<Company>\<Product>
            string regKey = $@"HKCU\Software\Unity\UnityEditor\{Company}\{Product}";

            // Copy the registry path to clipboard and open regedit.
            GUIUtility.systemCopyBuffer = regKey;

            try {
                Process.Start("regedit.exe");
            }
            catch (Exception ex) {
                EditorUtility.DisplayDialog("PlayerPrefs (Windows)",
                    $"Failed to start regedit: {ex.Message}\nRegistry key copied to clipboard:\n{regKey}", "OK");
                return;
            }

            EditorUtility.DisplayDialog("PlayerPrefs (Windows)",
                $"Registry key copied to clipboard:\n{regKey}\n\nRegedit was opened. Paste the key into the address bar of regedit to navigate.", "OK");
        }
        else if (Application.platform == RuntimePlatform.LinuxEditor) {
            // Linux: ~/.config/unity3d/<Company>/<Product>
            string configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".config", "unity3d", Company, Product
            );

            OpenPathOrShow(configPath);
        }
        else {
            // Other editor platforms (shouldn't normally happen)
            EditorUtility.DisplayDialog("PlayerPrefs",
                "This tool is intended for the Editor on macOS, Windows, or Linux. Mobile and WebGL PlayerPrefs are stored on the device/browser.", "OK");
        }
    }

    /// <summary>
    /// Open the folder containing the given path, or show a dialog and copy the path to clipboard if it doesn't exist.
    /// </summary>
    private static void OpenPathOrShow(string path) {
        // If the exact file exists, open its containing folder. If a directory exists, open it directly.
        bool isFile = File.Exists(path);
        bool isDir = Directory.Exists(path);

        if (isFile || isDir) {
            string folder = isFile ? Path.GetDirectoryName(path) : path;

            try {
                if (Application.platform == RuntimePlatform.OSXEditor) {
                    // Use 'open' on macOS
                    Process.Start("open", $"\"{folder}\"");
                }
                else if (Application.platform == RuntimePlatform.LinuxEditor) {
                    // Use xdg-open on Linux
                    Process.Start("xdg-open", $"\"{folder}\"");
                }
                else {
                    // Default to explorer on Windows (shouldn't reach here for Windows because registry is used)
                    Process.Start("explorer.exe", $"\"{folder}\"");
                }
            }
            catch (Exception ex) {
                // If opening fails, copy path to clipboard and inform the user.
                GUIUtility.systemCopyBuffer = folder;
                EditorUtility.DisplayDialog("PlayerPrefs",
                    $"Could not open folder automatically: {ex.Message}\nThe path was copied to the clipboard:\n{folder}", "OK");
            }
        }
        else {
            // Path not found: copy to clipboard and notify user so they can inspect manually.
            GUIUtility.systemCopyBuffer = path;
            EditorUtility.DisplayDialog("PlayerPrefs",
                $"Path not found:\n{path}\n\nThe path has been copied to the clipboard.", "OK");
        }
    }
#endif
    #endregion
}

public enum TimeFormat {
    SecondsOnly,
    SecondsMilliseconds,
    SecondsCentiseconds,
    MinutesSeconds,
    MinutesSecondsMilliseconds,
    MinutesSecondsCentiseconds,
    HoursMinutesSeconds
}