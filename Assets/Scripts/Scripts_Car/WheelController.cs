using UnityEngine;

public class WheelController : MonoBehaviour
{
    public float m_MaxTurnAngle = 30.0f;

    WheelCollider m_WheelCollider;
    Transform m_WheelMeshTransform;

    private void Start()
    {
        m_WheelCollider = GetComponent<WheelCollider>();
        m_WheelMeshTransform = transform.Find("WheelMesh");
    }

    public void WheelSteer(float i_SteerValue)
    {
        m_WheelCollider.steerAngle = i_SteerValue * m_MaxTurnAngle;
    }

    public void WheelAccelerate(float i_AccelerateValue)
    {
        m_WheelCollider.motorTorque = i_AccelerateValue;
    }

    public void UpdateMeshTransform()
    {
        Vector3 l_Position = transform.position;
        Quaternion l_Rotation = transform.rotation;

        m_WheelCollider.GetWorldPose(out l_Position, out l_Rotation);

        m_WheelMeshTransform.transform.position = l_Position;
        m_WheelMeshTransform.transform.rotation = l_Rotation;
    }
}
