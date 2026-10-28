using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer audioMixer;

    [Header("Button Sounds")]
    [SerializeField] private AudioResource hoverSound;
    [SerializeField] private AudioResource clickSound;
    [SerializeField] private AudioResource backSound;
    [SerializeField, Min(0f)] private float hoverCooldown = 0.1f;

    [SerializeField] private AudioMixerGroup outputGroup;

    public AudioMixer Mixer => audioMixer;

    private AudioSource audioSource;
    private float lastHoverTime = -999f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (outputGroup != null) audioSource.outputAudioMixerGroup = outputGroup;
    }

    public void PlayHover()
    {
        if (Time.unscaledTime - lastHoverTime < hoverCooldown) return;

        lastHoverTime = Time.unscaledTime;
        PlaySound(hoverSound);
    }

    public void PlayClick() { PlaySound(clickSound); }
    public void PlayBack() { PlaySound(backSound); }

    public void PlaySound(AudioResource sound)
    {
        if (sound == null) return;

        if (sound is AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            audioSource.resource = sound;
            audioSource.Play();
        }
    }
}