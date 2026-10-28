using UnityEngine;

public class DeviceDetector : MonoBehaviour
{
    [SerializeField] DeviceType m_DeviceType;

    private void Awake()
    {
        gameObject.SetActive(SystemInfo.deviceType == m_DeviceType);
    }
}
