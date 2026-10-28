using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private bool isBackButton = false;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClicked);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance == null || !button.interactable) return;
        AudioManager.Instance.PlayHover();
    }

    private void OnClicked()
    {
        if (AudioManager.Instance == null) return;

        if (isBackButton) AudioManager.Instance.PlayBack();
        else AudioManager.Instance.PlayClick();
    }
}