using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CountdownTimer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text timerText;

    [Header("Settings")]
    [SerializeField, Min(0f)] private float duration = 60f;
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Events")]
    public UnityEvent onFinished;

    public float TimeRemaining { get; private set; }
    public bool IsRunning { get; private set; }

    private void Awake()
    {
        if (timerText == null)
            timerText = GetComponent<TMP_Text>();

        ResetTimer();

        if (startOnAwake)
            StartTimer();
    }

    private void Update()
    {
        if (!IsRunning) return;

        TimeRemaining -= useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            IsRunning = false;
            UpdateDisplay();
            onFinished?.Invoke();
            return;
        }

        UpdateDisplay();
    }

    public void StartTimer()
    {
        if (TimeRemaining <= 0f)
            TimeRemaining = duration;

        IsRunning = true;
    }

    public void PauseTimer() => IsRunning = false;

    public void ResetTimer()
    {
        IsRunning = false;
        TimeRemaining = duration;
        UpdateDisplay();
    }

    public void SetDuration(float seconds)
    {
        duration = Mathf.Max(0f, seconds);
        ResetTimer();
    }

    private void UpdateDisplay()
    {
        if (timerText == null) return;

        int total = Mathf.CeilToInt(TimeRemaining);
        timerText.text = $"{total / 60:00}:{total % 60:00}";
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            TimeRemaining = duration;
            UpdateDisplay();
        }
    }
#endif
}