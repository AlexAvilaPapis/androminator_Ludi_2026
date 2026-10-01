using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    [SerializeField] LayerMask m_CollisionMask;
    [SerializeField] GameObject m_Body = null;
    //[SerializeField] GameObject m_WheelPrefab = null;

    Rigidbody m_CarRigidbody;
    float m_TotalWeight;



    // BODY DATA -----------------
    BodyBehaviour m_BodyBehaviour;
    MeshFilter m_BodyMeshFilter;
    Renderer m_BodyRenderer;
    
    float m_BodyWeight = 1.0f;
    Mesh m_BodyMesh;
    Material m_BodyMaterial;
    int m_FrontWheelsAmount = 2;
    int m_BackWheelsAmount = 2;
    Vector3 m_FrontWheelPosition = new Vector3(0.8f, -0.5f, 1.0f);
    Vector3 m_BackWheelPosition = new Vector3(0.8f, -0.5f, -1.0f);
    // ---------------------------
    public List<GameObject> m_Wheels;



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

    float m_WheelGripFactor;
    float m_WheelWeight;
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
            if (l_Wheel.activeInHierarchy)
            {

            
                GetWheelParams(l_Wheel);



                if (Physics.Raycast(m_WheelTransform.position, -m_WheelTransform.up, out m_WheelRayHit, m_SpringFullDistance, m_CollisionMask))
                {
                    // SUSPENSION ------------------------------------------------
                    Vector3 l_SpringDirection = /*m_WheelRayHit.normal*/ m_WheelTransform.up;

                    Vector3 l_WheelWorldVelocity = m_CarRigidbody.GetPointVelocity(m_WheelTransform.position);
                    //Debug.Log(l_Wheel.name + ": " + l_WheelWorldVelocity);
                    float l_Offset = m_SpringRestDistance - m_WheelRayHit.distance;

                    float l_Velocity = Vector3.Dot(l_SpringDirection, l_WheelWorldVelocity);

                    float l_SuspensionForce = (l_Offset * m_SpringForce) - (l_Velocity * m_SpringDamper);



                    m_CarRigidbody.AddForceAtPosition(l_SpringDirection * l_SuspensionForce, m_WheelTransform.position);
                    //m_CarRigidbody.AddForceAtPosition(l_SpringDirection * l_SuspensionForce, m_WheelRayHit.point);

                    m_WheelMeshTransform.position = m_WheelRayHit.point + (m_WheelTransform.up * (m_SpringRestDistance));
                    // -----------------------------------------------------------



                    //// STEERING --------------------------------------------------
                    //Vector3 l_SteeringDirection = m_WheelTransform.right;

                    ////Vector3 l_WheelWorldVelocity2 = m_CarRigidbody.GetPointVelocity(m_WheelTransform.position);

                    //float l_SteeringVelocity = Vector3.Dot(l_SteeringDirection, l_WheelWorldVelocity);

                    //float l_DesiredVelocityChange = -l_SteeringVelocity * m_WheelGripFactor;
                    ////Debug.Log(l_SteeringVelocity + " || " + l_DesiredVelocityChange);
                    //Debug.Log(l_Wheel.name + ": " + l_WheelWorldVelocity);

                    //float l_DesiredAcceleration = l_DesiredVelocityChange / Time.fixedDeltaTime;

                    //m_CarRigidbody.AddForceAtPosition(l_SteeringDirection * m_WheelWeight * l_DesiredAcceleration, m_WheelTransform.position);
                    //// -----------------------------------------------------------

                    float l_SteeringVelocity = Vector3.Dot(l_WheelWorldVelocity, m_WheelTransform.right);
                    Debug.Log(l_SteeringVelocity);

                    float l_DesiredVelocityChange = -l_SteeringVelocity * m_WheelGripFactor;

                    //float l_DesiredAcceleration = l_DesiredVelocityChange * (1 / Time.fixedDeltaTime);

                    m_CarRigidbody.AddForceAtPosition(
                        m_WheelTransform.right * (m_BodyWeight / (m_FrontWheelsAmount + m_BackWheelsAmount)) * l_DesiredVelocityChange, 
                        m_WheelTransform.position);
                }
                else
                {
                    m_WheelMeshTransform.position = Vector3.Lerp(m_WheelMeshTransform.position, m_WheelTransform.position + (-m_WheelTransform.up * (m_SpringRestDistance)), Time.deltaTime);
                }
            }
        }
    }



    void ApplyForce_Steering()
    {

    }



    void GetBodyParams()
    {
        m_BodyBehaviour     = m_Body.GetComponent<BodyBehaviour>();
        m_BodyMeshFilter    = m_Body.GetComponent<MeshFilter>();
        m_BodyRenderer      = m_Body.GetComponent<Renderer>();

        m_BodyWeight            = m_BodyBehaviour.m_BodyParams.m_BodyWeight;
        m_BodyMesh              = m_BodyBehaviour.m_BodyParams.m_BodyMesh;
        m_BodyMaterial          = m_BodyBehaviour.m_BodyParams.m_BodyMaterial;
        m_FrontWheelsAmount     = m_BodyBehaviour.m_BodyParams.m_FrontWheelsAmount;
        m_BackWheelsAmount      = m_BodyBehaviour.m_BodyParams.m_BackWheelsAmount;
        m_FrontWheelPosition    = m_BodyBehaviour.m_BodyParams.m_FrontWheelPosition;
        m_BackWheelPosition     = m_BodyBehaviour.m_BodyParams.m_BackWheelPosition;
        
        m_CarRigidbody.mass         = m_BodyWeight;
        m_BodyMeshFilter.mesh       = m_BodyMesh;
        m_BodyRenderer.material     = m_BodyMaterial;



        int l_FrontWheelsAmount = m_FrontWheelsAmount;
        int l_BackWheelsAmount = m_BackWheelsAmount;
        Vector3 l_FrontWheelPos = m_FrontWheelPosition;
        Vector3 l_BackWheelPos = m_BackWheelPosition;

        foreach (GameObject i_Wheel in m_Wheels)
        {
            if (i_Wheel.name.Contains("WheelF"))
            {
                if (l_FrontWheelsAmount > 0)
                {
                    if(!i_Wheel.activeInHierarchy) i_Wheel.SetActive(true);
                    i_Wheel.transform.localPosition = l_FrontWheelPos;
                    l_FrontWheelPos.x *= -1;
                    l_FrontWheelsAmount--;
                }
                else
                {
                    i_Wheel.SetActive(false);
                }
            }

            if (i_Wheel.name.Contains("WheelB"))
            {
                if (l_BackWheelsAmount > 0)
                {
                    if (!i_Wheel.activeInHierarchy) i_Wheel.SetActive(true);
                    i_Wheel.transform.localPosition = l_BackWheelPos;
                    l_BackWheelPos.x *= -1;
                    l_BackWheelsAmount--;
                }
                else
                {
                    i_Wheel.SetActive(false);
                }
            }
        }
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
        m_WheelGripFactor       = m_WheelBehaviour.m_WheelParams.m_WheelGripFactor;
        m_WheelWeight           = m_WheelBehaviour.m_WheelParams.m_WheelWeight;

        m_WheelMeshFilter.mesh      = m_WheelMesh;
        m_WheelRenderer.material    = m_WheelMaterial;
    }


    private void OnDrawGizmos()
    {
        foreach (GameObject l_Wheel in m_Wheels)
        {
            if (l_Wheel.activeInHierarchy)
            {
                Gizmos.DrawLine(l_Wheel.transform.position, l_Wheel.transform.position + l_Wheel.transform.right);
            }
        }
    }
}
