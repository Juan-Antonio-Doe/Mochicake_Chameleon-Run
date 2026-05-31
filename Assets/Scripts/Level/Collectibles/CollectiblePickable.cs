using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CollectiblePickable : Collectible {

    [field: Header("Collectible pickable properties")]
    [field: SerializeField] protected bool isPickable { get; set; } = true;
    [field: SerializeField] public bool isPicked { get; private set; }
    //[field: SerializeField] protected UnityEvent onPickCollectible { get; set; }

#if UNITY_EDITOR
    protected override void AssignComponents() {

        base.AssignComponents();
    }
#endif

    protected virtual void OnEnable() {
        if (IsAlreadyCollectedAndSaved()) {
            isPicked = true;
            gameObject.SetActive(false);
            return;
        }
        isPicked = false;
        GameEvents.OnLevelCompleted += SaveData;
        GameEvents.OnPlayerFailed += ResetStatus;
    }

    private void OnTriggerEnter(Collider other) {
        if (!isPickable || isPicked)
            return;

        if (other.CompareTag("Player")) {
            OnPick();
        }
    }

    void OnPick() {
        isPicked = true;
        if (LevelManager.Instance)
            LevelManager.Instance.playerController.pAudioSource.PlayOneShot(colSFX);
        //onPickCollectible.Invoke();

        gameObject.SetActive(false);
    }

    void ResetStatus() {
        if (!IsAlreadyCollectedAndSaved()) {
            if (gameObject) // To avoid a "trying to access a destroyed object" exception.
                if (!gameObject.activeInHierarchy)
                    gameObject.SetActive(true);
            isPicked = false;
        }
    }

    protected bool IsAlreadyCollectedAndSaved() {
        return PlayerPrefs.HasKey(GetPlayerPrefsKey()) && PlayerPrefs.GetInt(GetPlayerPrefsKey(), 0) == 1;
    }

    protected override void SaveData() {
        if (isPicked) {
            PlayerPrefs.SetInt(GetPlayerPrefsKey(), 1);
            base.SaveData();
        }
    }

    protected virtual void OnDestroy() {
        // These need to be put here so the methods can be called when the object is disabled.
        GameEvents.OnLevelCompleted -= SaveData;
        GameEvents.OnPlayerFailed -= ResetStatus;
    }
}