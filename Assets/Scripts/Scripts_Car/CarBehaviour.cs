using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    [SerializeField] LayerMask m_CollisionMask;
    public GameObject[] m_Wheels = null;
    public GameObject m_Body = null;

    Rigidbody m_CarRigidbody;



    // BODY DATA -----------------
    BodyBehaviour m_BodyBehaviour;
    MeshFilter m_BodyMeshFilter;
    Renderer m_BodyRenderer;
    
    float m_BodyWeight = 1.0f;
    Mesh m_BodyMesh;
    Material m_BodyMaterial;
    // ---------------------------



    // WHEELS DATA ---------------------
    WheelBehaviour m_WheelBehaviour;
    Transform m_WheelTransform;
    Transform m_WheelMeshTransform;
    MeshFilter m_WheelMeshFilter;
    Renderer m_WheelRenderer;

    float m_SpringFullDistance = 1.0f;
    float m_SpringRestDistance = 0.7f;
    float m_SpringForce = 10.0f;
    float m_SpringDamper = 0.5f;
    Mesh m_WheelMesh;
    Material m_WheelMaterial;
    // ---------------------------------
    RaycastHit m_WheelRayHit;



    private void Awake()
    {
        m_CarRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        GetBodyParams();
    }

    private void Update()
    {
        ApplyForce_Suspension();
    }



    void ApplyForce_Suspension()
    {
        foreach (GameObject l_Wheel in m_Wheels)
        {
            GetWheelParams(l_Wheel);



            if (Physics.Raycast(m_WheelTransform.position, -m_WheelTransform.up, out m_WheelRayHit, m_SpringFullDistance, m_CollisionMask))
            {
                Vector3 l_SpringDirection = m_WheelRayHit.normal /*m_WheelTransform.up*/;

                Vector3 l_WheelWorldVelocity = m_CarRigidbody.GetPointVelocity(m_WheelTransform.position);

                float l_Offset = m_SpringRestDistance - m_WheelRayHit.distance;

                float l_Velocity = Vector3.Dot(l_SpringDirection, l_WheelWorldVelocity);

                float l_SuspensionForce = (l_Offset * m_SpringForce) - (l_Velocity * m_SpringDamper);



                m_CarRigidbody.AddForceAtPosition(l_SpringDirection * l_SuspensionForce, m_WheelTransform.position);

                m_WheelMeshTransform.position = m_WheelRayHit.point + (m_WheelTransform.up * (m_SpringRestDistance));
            }
        }
    }

    void GetBodyParams()
    {
        m_BodyBehaviour     = m_Body.GetComponent<BodyBehaviour>();
        m_BodyMeshFilter    = m_Body.GetComponent<MeshFilter>();
        m_BodyRenderer      = m_Body.GetComponent<Renderer>();

        m_BodyWeight        = m_BodyBehaviour.m_BodyParams.m_BodyWeight;
        m_BodyMesh          = m_BodyBehaviour.m_BodyParams.m_BodyMesh;
        m_BodyMaterial      = m_BodyBehaviour.m_BodyParams.m_BodyMaterial;

        m_CarRigidbody.mass         = m_BodyWeight;
        m_BodyMeshFilter.mesh       = m_BodyMesh;
        m_BodyRenderer.material     = m_BodyMaterial;
    }
    void GetWheelParams(GameObject i_Wheel)
    {
        m_WheelBehaviour        = i_Wheel.GetComponent<WheelBehaviour>();
        m_WheelTransform        = i_Wheel.transform;
        m_WheelMeshFilter       = i_Wheel.GetComponentInChildren<MeshFilter>();
        m_WheelRenderer         = i_Wheel.GetComponentInChildren<Renderer>();
        m_WheelMeshTransform    = m_WheelRenderer.transform;

        m_SpringFullDistance    = m_WheelBehaviour.m_WheelParams.m_SpringFullDistance;
        m_SpringRestDistance    = m_WheelBehaviour.m_WheelParams.m_SpringRestDistance;
        m_SpringForce           = m_WheelBehaviour.m_WheelParams.m_SpringForce;
        m_SpringDamper          = m_WheelBehaviour.m_WheelParams.m_SpringDamper;
        m_WheelMesh             = m_WheelBehaviour.m_WheelParams.m_WheelMesh;
        m_WheelMaterial         = m_WheelBehaviour.m_WheelParams.m_WheelMaterial;

        m_WheelMeshFilter.mesh      = m_WheelMesh;
        m_WheelRenderer.material    = m_WheelMaterial;
    }
}
