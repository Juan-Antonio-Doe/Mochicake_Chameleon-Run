using System;

public static class GameEvents {
    public static event Action OnPlayerFailed;
    public static event Action OnLevelCompleted;

    public static void TriggerPlayerFailed() => OnPlayerFailed?.Invoke();
    public static void TriggerLevelCompleted() => OnLevelCompleted?.Invoke();
}