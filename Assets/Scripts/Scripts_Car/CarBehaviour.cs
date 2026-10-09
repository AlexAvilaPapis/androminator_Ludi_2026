using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarBehaviour : MonoBehaviour
{
    [SerializeField] LayerMask m_CollisionMask;
    [SerializeField] GameObject m_Body = null;

    [SerializeField] InputActionAsset m_InputActionAsset;
    InputAction m_MoveAction;

    // GENERAL DATA -----------------
    [SerializeField] float m_MaxWheelAngle = 15.0f;
    [SerializeField] float m_MaxCarSpeed = 30.0f;

    [SerializeField] AnimationCurve m_SpeedCurve;

    float m_HorizontalInput = 0.0f;
    float m_VerticalInput = 0.0f;

    float m_CurrentWheelAngle = 0.0f;
    float m_CurrentCarSpeed = 0.0f;

    float m_TotalWeight;
    // ------------------------------



    // BODY DATA -----------------
    BodyBehaviour m_BodyBehaviour;
    MeshFilter m_BodyMeshFilter;
    Renderer m_BodyRenderer;

    float m_BodyWeight = 1.0f;
    float m_WheelTurnPower = 5.0f;
    Mesh m_BodyMesh;
    Material m_BodyMaterial;
    int m_FrontWheelsAmount = 2;
    int m_BackWheelsAmount = 2;
    Vector3 m_FrontWheelPosition = new Vector3(0.8f, -0.5f, 1.0f);
    Vector3 m_BackWheelPosition = new Vector3(0.8f, -0.5f, -1.0f);
    // ---------------------------
    public List<GameObject> m_Wheels;
    List<GameObject> m_ActiveWheels = new List<GameObject>();



    // WHEELS DATA ---------------------
    WheelBehaviour[] m_WheelBehaviour;
    Transform[] m_WheelTransform;
    Transform[] m_WheelMeshTransform;
    MeshFilter[] m_WheelMeshFilter;
    Renderer[] m_WheelRenderer;

    float[] m_SpringFullDistance;
    float[] m_SpringRestDistance;
    float[] m_SpringForce;
    float[] m_SpringDamper;
    Mesh[] m_WheelMesh;
    Material[] m_WheelMaterial;

    float[] m_WheelGripFactor;
    float[] m_WheelWeight;
    // ---------------------------------
    RaycastHit[] m_WheelRayHit;

    Rigidbody m_CarRigidbody;



    private void Awake()
    {
        m_CarRigidbody = GetComponentInChildren<Rigidbody>();

        m_MoveAction = InputSystem.actions.FindAction("Move");
    }

    private void Start()
    {
        GetBodyParams();
        //GetWheelParams();
    }

    private void Update()
    {
        for (int i = 0; i < m_ActiveWheels.Count; i++)
        {
            ApplyForces(i);
        }

        ApplyMovementInput();
    }


    
    void ApplyMovementInput()
    {
        // STEERING -------------------------
        m_HorizontalInput = m_MoveAction.ReadValue<Vector2>().x;
        m_VerticalInput = m_MoveAction.ReadValue<Vector2>().y;

        if (m_MoveAction.IsPressed() && m_HorizontalInput != 0)
        {
            m_CurrentWheelAngle += m_MaxWheelAngle * m_HorizontalInput * m_WheelTurnPower * Time.deltaTime;
        }
        else if (m_CurrentWheelAngle > 1f || m_CurrentWheelAngle < -1f)
        {
            m_CurrentWheelAngle += m_MaxWheelAngle * -Mathf.Sign(m_CurrentWheelAngle) * m_WheelTurnPower * Time.deltaTime;
        }
        else
        {
            m_CurrentWheelAngle = 0;
        }

        m_CurrentWheelAngle = Mathf.Clamp(m_CurrentWheelAngle, -m_MaxWheelAngle, m_MaxWheelAngle);



        foreach (GameObject i_Wheel in m_ActiveWheels)
        {
            if (i_Wheel.name.Contains("WheelF"))
            {
                Vector3 l_CurrentRotation = i_Wheel.transform.localRotation.eulerAngles;
                l_CurrentRotation.y = m_CurrentWheelAngle;

                i_Wheel.transform.localRotation = Quaternion.Euler(l_CurrentRotation);
            }
        }
        // ----------------------------------



        // VELOCITY -------------------------
        m_CurrentCarSpeed = m_VerticalInput * m_MaxCarSpeed;
        // ----------------------------------
    }


    

    void ApplyForces(int i)
    {
        if (Physics.Raycast(m_WheelTransform[i].position, -m_WheelTransform[i].up, out m_WheelRayHit[i], m_SpringFullDistance[i], m_CollisionMask))
        {
            // SUSPENSION ------------------------------------------------
            //Vector3 l_SpringDirection = m_WheelRayHit[i].normal;
            Vector3 l_SpringDirection = m_WheelTransform[i].up;

            Vector3 l_WheelWorldVelocity = m_CarRigidbody.GetPointVelocity(m_WheelTransform[i].position);

            float l_Offset = m_SpringRestDistance[i] - m_WheelRayHit[i].distance;

            float l_VerticalVelocity = Vector3.Dot(l_SpringDirection, l_WheelWorldVelocity);

            float l_ElasticityForce = (l_Offset * m_SpringForce[i]);
            float l_DampingForce = (l_VerticalVelocity * m_SpringDamper[i]);
            float l_MaxElasticityForce = (m_SpringFullDistance[i] - m_SpringRestDistance[i]) * m_SpringForce[i];    // PRESCINDIBLE
            l_DampingForce = Mathf.Clamp(l_DampingForce, -l_MaxElasticityForce, l_MaxElasticityForce);              // PRESCINDIBLE

            float l_SuspensionForce = l_ElasticityForce - l_DampingForce;

            m_CarRigidbody.AddForceAtPosition(l_SpringDirection * l_SuspensionForce, m_WheelRayHit[i].point);

            m_WheelMeshTransform[i].position = m_WheelRayHit[i].point + (m_WheelTransform[i].up * (m_SpringRestDistance[i]));
            // -----------------------------------------------------------



            // STEERING --------------------------------------------------
            Vector3 l_SteeringDirection = Vector3.ProjectOnPlane(m_WheelTransform[i].right, m_WheelRayHit[i].normal);

            float l_SteeringVelocity = Vector3.Dot(l_SteeringDirection, l_WheelWorldVelocity);

            float l_DesiredVelocityChange = -l_SteeringVelocity * m_WheelGripFactor[i];

            float l_DesiredAcceleration = l_DesiredVelocityChange / 1;

            m_CarRigidbody.AddForceAtPosition(
                l_SteeringDirection * m_WheelWeight[i] * l_DesiredAcceleration,
                m_WheelTransform[i].position);
            // -----------------------------------------------------------



            // ACCELERATION ----------------------------------------------
            if (m_ActiveWheels[i].name.Contains("WheelF"))
            {
                //Vector3 l_AccelerationDirection = Vector3.ProjectOnPlane(m_WheelTransform[i].forward, m_WheelRayHit[i].normal);
                Vector3 l_AccelerationDirection = m_WheelTransform[i].forward;

                float l_CarSpeed = Vector3.Dot(m_WheelTransform[i].forward, m_CarRigidbody.linearVelocity);

                float l_CarSpeedNormalized = Mathf.Clamp01(Mathf.Abs(l_CarSpeed) / m_MaxCarSpeed);

                float l_Acceleration = 0.0f;
                if (m_CurrentCarSpeed != 0)
                {
                    l_Acceleration = m_SpeedCurve.Evaluate(l_CarSpeedNormalized) * m_CurrentCarSpeed;
                }
                else
                {
                    l_Acceleration = -Mathf.Sign(l_CarSpeed) * 1 / 10 * m_MaxCarSpeed;
                }

                m_CarRigidbody.AddForceAtPosition(l_AccelerationDirection * l_Acceleration / m_FrontWheelsAmount, m_WheelTransform[i].position);



                //l_CarSpeed = Vector3.Dot(m_WheelTransform[i].forward, m_CarRigidbody.linearVelocity);
                //Debug.Log(l_CarSpeed);
            }
            // -----------------------------------------------------------
        }
        else
        {
            // WHEELS GRAVITY --------------------------------------------
            Vector3 l_WheelWorldVelocity = m_CarRigidbody.GetPointVelocity(m_WheelTransform[i].position);

            Vector3 l_VerticalSpeed = Vector3.up * -Physics.gravity.y * Time.deltaTime;

            float l_SpringSpeed = Vector3.Dot(m_WheelTransform[i].up, l_VerticalSpeed);

            m_WheelMeshTransform[i].position -= m_WheelTransform[i].up * l_SpringSpeed / m_SpringDamper[i];
            m_WheelMeshTransform[i].localPosition = Vector3.ClampMagnitude(m_WheelMeshTransform[i].localPosition, m_SpringFullDistance[i] - m_SpringRestDistance[i]);
            // -----------------------------------------------------------
        }
    }



    void GetBodyParams()
    {
        m_BodyBehaviour = m_Body.GetComponent<BodyBehaviour>();
        m_BodyMeshFilter = m_Body.GetComponent<MeshFilter>();
        m_BodyRenderer = m_Body.GetComponent<Renderer>();

        m_BodyWeight = m_BodyBehaviour.m_BodyParams.m_BodyWeight;
        m_WheelTurnPower = m_BodyBehaviour.m_BodyParams.m_WheelTurnPower;
        m_BodyMesh = m_BodyBehaviour.m_BodyParams.m_BodyMesh;
        m_BodyMaterial = m_BodyBehaviour.m_BodyParams.m_BodyMaterial;
        m_FrontWheelsAmount = m_BodyBehaviour.m_BodyParams.m_FrontWheelsAmount;
        m_BackWheelsAmount = m_BodyBehaviour.m_BodyParams.m_BackWheelsAmount;
        m_FrontWheelPosition = m_BodyBehaviour.m_BodyParams.m_FrontWheelPosition;
        m_BackWheelPosition = m_BodyBehaviour.m_BodyParams.m_BackWheelPosition;

        m_BodyMeshFilter.mesh = m_BodyMesh;
        m_BodyRenderer.material = m_BodyMaterial;



        int l_FrontWheelsAmount = m_FrontWheelsAmount;
        int l_BackWheelsAmount = m_BackWheelsAmount;
        Vector3 l_FrontWheelPos = m_FrontWheelPosition;
        Vector3 l_BackWheelPos = m_BackWheelPosition;
        m_ActiveWheels.Clear();

        foreach (GameObject i_Wheel in m_Wheels)
        {
            if (i_Wheel.name.Contains("WheelF"))
            {
                if (l_FrontWheelsAmount > 0)
                {
                    if (!i_Wheel.activeInHierarchy) i_Wheel.SetActive(true);
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

            if (i_Wheel.activeInHierarchy)
            {
                m_ActiveWheels.Add(i_Wheel);
            }
        }

        SetNewWheelsParams(m_ActiveWheels.Count);

        GetWheelParams();



        CalculateCarWeight();
    }



    void GetWheelParams()
    {
        for (int i = 0; i < m_ActiveWheels.Count; i++)
        {
            GameObject l_Wheel = m_ActiveWheels[i];
            m_WheelBehaviour[i] = l_Wheel.GetComponent<WheelBehaviour>();
            m_WheelTransform[i] = l_Wheel.transform;
            m_WheelMeshFilter[i] = l_Wheel.GetComponentInChildren<MeshFilter>();
            m_WheelRenderer[i] = l_Wheel.GetComponentInChildren<Renderer>();
            m_WheelMeshTransform[i] = m_WheelRenderer[i].transform;

            WheelParams l_WheelParams = m_WheelBehaviour[i].m_WheelParams;
            m_SpringFullDistance[i] = l_WheelParams.m_SpringFullDistance;
            m_SpringRestDistance[i] = l_WheelParams.m_SpringRestDistance;
            m_SpringForce[i] = l_WheelParams.m_SpringForce;
            m_SpringDamper[i] = l_WheelParams.m_SpringDamper;
            m_WheelMesh[i] = l_WheelParams.m_WheelMesh;
            m_WheelMaterial[i] = l_WheelParams.m_WheelMaterial;
            m_WheelGripFactor[i] = l_WheelParams.m_WheelGripFactor;
            m_WheelWeight[i] = l_WheelParams.m_WheelWeight;

            m_WheelMeshFilter[i].mesh = m_WheelMesh[i];
            m_WheelRenderer[i].material = m_WheelMaterial[i];
        }



        CalculateCarWeight();
    }

    void SetNewWheelsParams(int i)
    {
        m_WheelBehaviour = new WheelBehaviour[i];
        m_WheelTransform = new Transform[i];
        m_WheelMeshTransform = new Transform[i];
        m_WheelMeshFilter = new MeshFilter[i];
        m_WheelRenderer = new Renderer[i];

        m_SpringFullDistance = new float[i];
        m_SpringRestDistance = new float[i];
        m_SpringForce = new float[i];
        m_SpringDamper = new float[i];
        m_WheelMesh = new Mesh[i];
        m_WheelMaterial = new Material[i];

        m_WheelGripFactor = new float[i];
        m_WheelWeight = new float[i];

        m_WheelRayHit = new RaycastHit[i];
    }



    void CalculateCarWeight()
    {
        float l_TotalWheelsWeight = 0.0f;
        for (int i = 0; i < m_ActiveWheels.Count; ++i)
        {
            l_TotalWheelsWeight += m_WheelWeight[i];
        }
        m_TotalWeight = m_BodyWeight + l_TotalWheelsWeight;
        m_CarRigidbody.mass = m_TotalWeight;
    }



    private void OnEnable()
    {
        m_InputActionAsset.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
        m_InputActionAsset.FindActionMap("Player").Disable();
    }
}
