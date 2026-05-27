using System;
using UnityEngine;
using UnityEngine.Events;

public enum TimerMode { Stopwatch, Countdown }

[Serializable]
public class Timer {

    [field: Header("Timer Settings")]
    [field: SerializeField] private TimerMode mode { get; set; } = TimerMode.Stopwatch;
    [field: SerializeField, Tooltip("Only used when TimeMode is Countdown.")] private float timeLimit { get; set; } = 60f;
    [field: SerializeField] private bool useUnscaledTime { get; set; } = false;

    // Countdown progress [0, 1]. Useful for UI fill bars.
    public float NormalizedTime => timeLimit > 0f ? Mathf.Clamp01(CurrentTime / timeLimit) : 0f;

    /*public event Action OnStart;
    public event Action OnStop;
    public event Action OnComplete;*/
    [field: Header("Timer Events")]
    [field: SerializeField] public UnityEvent OnStart;
    [field: SerializeField] public UnityEvent OnStop;
    [field: SerializeField] public UnityEvent OnComplete;

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] public float CurrentTime { get; private set; }
    [field: SerializeField, ReadOnlyField] public bool IsRunning { get; private set; }
    [field: SerializeField, ReadOnlyField] public bool IsPaused { get; private set; }
    [field: SerializeField, ReadOnlyField] public bool IsComplete { get; private set; }

    public void Start() {
        CurrentTime = mode == TimerMode.Countdown ? timeLimit : 0f;
        IsRunning = true;
        IsPaused = false;
        IsComplete = false;
        OnStart?.Invoke();
    }

    public void Stop() {
        IsRunning = false;
        IsPaused = false;
        OnStop?.Invoke();
    }

    public void Pause() {
        if (!IsRunning || IsPaused) return;
        IsRunning = false;
        IsPaused = true;
    }

    public void Resume() {
        if (!IsPaused) return;
        IsPaused = false;
        IsRunning = true;
    }

    public void Reset() {
        CurrentTime = mode == TimerMode.Countdown ? timeLimit : 0f;
        IsComplete = false;
    }

    // Call from MonoBehaviour Update()
    public void Tick() {
        if (!IsRunning || IsComplete) return;

        float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (mode == TimerMode.Stopwatch) {
            CurrentTime += delta;
        }
        else {
            CurrentTime = Mathf.Max(0f, CurrentTime - delta);

            if (CurrentTime <= 0f) {
                IsComplete = true;
                IsRunning = false;
                OnComplete?.Invoke();
            }
        }
    }

    public float GetRawTime() {
        return CurrentTime;
    }
}