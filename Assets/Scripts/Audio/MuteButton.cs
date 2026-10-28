using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MuteButton : MonoBehaviour
{
    [SerializeField] private string exposedParameter = "MasterVolume";

    [Header("Sprites")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    private Image iconImage;
    private Button button;
    private AudioMixer audioMixer;
    private bool isMuted;

    private void Awake()
    {
        button = GetComponent<Button>();
        iconImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (AudioManager.Instance != null) audioMixer = AudioManager.Instance.Mixer;
        if (audioMixer == null)
            Debug.LogWarning("MuteButton: no Audio Mixer found. Add an AudioManager to the scene and assign the Audio Mixer on it.");

        isMuted = PlayerPrefs.GetInt(exposedParameter + "_Muted", 0) == 1;
        ApplyMute();
        button.onClick.AddListener(ToggleMute);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(ToggleMute);
    }

    private void ToggleMute()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt(exposedParameter + "_Muted", isMuted ? 1 : 0);
        ApplyMute();
    }

    private void ApplyMute()
    {
        if (audioMixer != null)
        {
            float volume = PlayerPrefs.GetFloat(exposedParameter + "_Volume", 1f);
            float decibels = (isMuted || volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;
            audioMixer.SetFloat(exposedParameter, decibels);
        }

        if (iconImage != null)
            iconImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
    }
}