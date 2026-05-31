using UnityEngine;
using UnityEngine.Events;

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
            // To avoid a "MissingReferenceException: The object of type 'CollectiblePickable' has been destroyed but you are still trying to access it."
            // exception. That seems to keep happening...
            if (this != null)
                if (gameObject != null) 
                    if (!gameObject.activeInHierarchy)
                        gameObject.SetActive(true);
            isPicked = false;
        }

        /*
         * Debugueando:
         * El bug de la exception se puede reproducir asi:
         * - Completar un nivel con alguna estrella recogida (no todas).
         * - Volver a cargar el nivel y recolectar alguna otra estrella.
         * - Fallar el nivel despues de recolectar la estrella.
         * - Exception.
         * 
         * Imagino que puede ser devido a una fuga de la suscripcion a OnPlayerFailed += ResetStatus;
         * pero no deberia ocurrir porque el objeto:
         *   - O no deberia estar destruido.
         *   - O si lo esta, deberia haberse desuscrito.
         *   
         * Ya deberia estar arreglado con la actualizacion de GameEvents.cs
         *   - Haciendo una limpieza cada vez que se pasa por la escena del menu principal.
         */
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