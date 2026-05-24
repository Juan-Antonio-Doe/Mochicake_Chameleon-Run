using System;

public static class GameEvents {
    public static event Action OnPlayerFailed;
    public static void TriggerPlayerFailed() => OnPlayerFailed?.Invoke();
}