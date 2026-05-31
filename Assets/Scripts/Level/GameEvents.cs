using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents {
    public static event Action OnPlayerFailed;
    public static event Action OnLevelCompleted;

    // Queue of handlers to remove
    static readonly Queue<Delegate> cleanupQueue = new Queue<Delegate>();
    static bool cleanupRunning = false;
    const int BatchSize = 5; // How many handlers to clean per frame

    #region Public methods for event calls
    public static void TriggerPlayerFailed() {
        SafeInvoke(ref OnPlayerFailed);
    }

    public static void TriggerLevelCompleted() {
        SafeInvoke(ref OnLevelCompleted);
    }
    #endregion

    #region Safety and Clean Up methods
    // --- Safe invocation: filters out destroyed targets and queues them for cleanup. ---
    static void SafeInvoke(ref Action evt) {
        var handlers = evt;
        if (handlers == null) return;

        foreach (Action d in handlers.GetInvocationList()) {
            // If the target is a UnityEngine.Object and Unity considers it destroyed -> mark it
            var targetUnityObj = d.Target as UnityEngine.Object;
            if (d.Target != null && targetUnityObj == null) {
                cleanupQueue.Enqueue(d);
                continue;
            }

            try {
                d();
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                cleanupQueue.Enqueue(d);
            }
        }
    }

    // --- Starts the cleanup coroutine using the provided MonoBehaviour. ---
    public static void StartCleanup(MonoBehaviour runner) {
        if (cleanupQueue.Count == 0) return;
        if (runner == null) return;
        if (!cleanupRunning) {
            cleanupRunning = true;
            runner.StartCoroutine(CleanupCoroutine());
        }
    }

    // --- Coroutine that processes the queue in batches ---
    static IEnumerator CleanupCoroutine() {
        while (cleanupQueue.Count > 0) {
            int processed = 0;
            while (processed < BatchSize && cleanupQueue.Count > 0) {
                var d = cleanupQueue.Dequeue();
                // We attempt to unsubscribe the delegate from the known events.
                try { OnPlayerFailed -= (Action)d; } catch { /* ignore */ }
                try { OnLevelCompleted -= (Action)d; } catch { /* ignore */ }
                processed++;
            }
            yield return null; // Distribute work across frames.
        }
        cleanupRunning = false;
    }
    #endregion
}