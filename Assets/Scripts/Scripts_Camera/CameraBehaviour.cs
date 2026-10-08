using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] Transform m_TargetTransform;
    [SerializeField] float m_MaxDistanceToTarget = 10.0f;
    [SerializeField] float m_CameraHeight = 5.0f;

    Vector3 m_CameraToTargetVector;

    void Update()
    {
        transform.position = new Vector3(transform.position.x, m_TargetTransform.position.y + m_CameraHeight, transform.position.z);

        m_CameraToTargetVector = m_TargetTransform.position - transform.position;

        LookAtTarget();
        MoveToTarget();
    }

    void LookAtTarget()
    {
        Vector3 l_Forward = Vector3.Normalize(m_CameraToTargetVector);
        transform.rotation = Quaternion.LookRotation(l_Forward, Vector3.up);
    }

    void MoveToTarget()
    {
        float l_DistanceToTarget = Vector3.Magnitude(m_CameraToTargetVector);

        if (l_DistanceToTarget > m_MaxDistanceToTarget)
        {
            Vector3 l_DesiredPosition = m_TargetTransform.position - Vector3.Normalize(m_CameraToTargetVector) * m_MaxDistanceToTarget;
            transform.position = Vector3.Lerp(transform.position, l_DesiredPosition, Time.deltaTime);
        }
    }
}
