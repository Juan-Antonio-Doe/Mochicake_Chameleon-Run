using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public abstract class Collectible : MonoBehaviour {

    [field: Header("Autoattach on Editor properties")]
    [field: SerializeField] protected bool revalidateProperties { get; set; }

    [field: Header("Collectible properties")]
    [field: SerializeField, ReadOnlyField] public string uniqueID { get; protected set; }   // Used for save/load data management.
    [field: SerializeField] private bool deleteUniqueID { get; set; }
    [field: SerializeField] public string collectibleName { get; protected set; }
    [field: SerializeField] protected AudioClip colSFX { get; set; }

#if UNITY_EDITOR
    virtual protected void OnValidate() {

        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

        if (!isValidPrefabStage && prefabConnected) {
            //Variables que solo se verificaran cuando estan en una escena cuando se active el trigger `revalidateProperties` para evitar bucles en el editor.
            if (revalidateProperties)
                AssignComponents();
        }
    }

    protected virtual void AssignComponents() {
        // Generate Unique ID for this gameObject in scene.
        AssingUniqueID();

        deleteUniqueID = false;
        revalidateProperties = false;
    }

    void AssingUniqueID() {
        if (deleteUniqueID) {
            uniqueID = string.Empty;
            Debug.Log("Deleted ID.");
            return;
        }

        if (string.IsNullOrEmpty(uniqueID)) {
            GeneralUtilities.GenerateUniqueID(this, transform, out string newID);
            uniqueID = newID;
            Debug.Log($"[{GetType()}] Generated new ID: {uniqueID}");
        }
        else {
            Debug.Log($"[{GetType()}] ID already exists: {uniqueID}");
        }
    }
#endif

    protected string GetPlayerPrefsKey() {
        string[] parts = LoadScene.CurrentNameScene().Split('_');
        int number = int.Parse(parts[parts.Length - 1]);

        if (string.IsNullOrEmpty(uniqueID)) {
            Debug.Log($"<color=red>The following object doesn't have a UniqueID:\n" +
                $"[{GetType()}]_[{gameObject.name}]_{transform.position.x}_{transform.position.y}_{transform.position.z}</color>");
        }

        return $"Level_{number}_Collectible_{uniqueID}";
    }

    protected virtual void SaveData() {
        PlayerPrefs.Save();
    }

}