using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class SteeringWheel : MonoBehaviour
{
    [SerializeField] private float maxAngle = 90f;
    [SerializeField] private float returnSpeed = 300f;
    [SerializeField] private Canvas canvas;

    private RectTransform rect;
    private float currentAngle;
    private float lastPointerAngle;
    private bool isDragging;

    Vector2 pos;
    TouchControl touch;

    private void Awake() => rect = GetComponent<RectTransform>();

    private void Update()
    {
        touch = Touchscreen.current?.primaryTouch;
        if (touch == null) return;

        pos = touch.position.ReadValue();

        if (touch.press.wasPressedThisFrame)
        {
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, pos, cam))
            {
                isDragging = true;
                lastPointerAngle = GetAngle(pos);
            }
        }
        else if (touch.press.isPressed && isDragging)
        {
            float a = GetAngle(pos);
            currentAngle = Mathf.Clamp(currentAngle + Mathf.DeltaAngle(lastPointerAngle, a), -maxAngle, maxAngle);
            lastPointerAngle = a;
        }
        else if (touch.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (!isDragging)
            currentAngle = Mathf.MoveTowards(currentAngle, 0f, returnSpeed * Time.deltaTime);

        rect.localEulerAngles = new Vector3(0f, 0f, currentAngle);
    }

    private float GetAngle(Vector2 screenPos)
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 center = RectTransformUtility.WorldToScreenPoint(cam, rect.position);
        Vector2 dir = screenPos - center;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
}