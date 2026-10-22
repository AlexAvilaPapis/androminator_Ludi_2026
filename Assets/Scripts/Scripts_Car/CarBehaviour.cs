using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarBehaviour : MonoBehaviour
{
    [SerializeField] LayerMask m_CollisionMask;
    [SerializeField] GameObject m_Body = null;

    [SerializeField] InputActionAsset m_InputActionAsset;
    InputAction m_MoveAction;
    InputAction m_JumpAction;

    // GENERAL DATA -----------------
    [Header("CAR DATA")]
    [SerializeField] float m_MaxWheelAngle = 15.0f;
    [SerializeField] float m_MaxCarSpeed = 30.0f;

    [SerializeField] AnimationCurve m_SpeedCurve;

    [SerializeField] float m_JumpAcceleration = 10.0f;
    [SerializeField] float m_JumpFlipForce = 10.0f;

    float m_HorizontalInput = 0.0f;
    float m_VerticalInput = 0.0f;

    float m_CurrentWheelAngle = 0.0f;
    float m_CurrentCarSpeed = 0.0f;

    float m_TotalWeight;

    Rigidbody m_CarRigidbody;

    [SerializeField] List<GameObject> m_Wheels;
    // ------------------------------



    // BODY DATA -----------------
    BodyBehaviour m_BodyBehaviour;
    MeshFilter m_BodyMeshFilter;
    Renderer m_BodyRenderer;
    BoxCollider m_BodyCollider;

    float m_BodyWeight = 1.0f;
    float m_WheelTurnPower = 5.0f;
    Vector3 m_ColliderDimensions = new Vector3(2, 1, 3);
    Mesh m_BodyMesh;
    Material m_BodyMaterial;
    int m_FrontWheelsAmount = 2;
    int m_BackWheelsAmount = 2;
    Vector3 m_FrontWheelPosition = new Vector3(0.8f, -0.5f, 1.0f);
    Vector3 m_BackWheelPosition = new Vector3(0.8f, -0.5f, -1.0f);
    // ---------------------------
    List<GameObject> m_ActiveWheels = new List<GameObject>();



    // WHEELS DATA ---------------------
    WheelBehaviour[] m_WheelBehaviour;
    Transform[] m_WheelTransform;
    Transform[] m_WheelMeshTransform;
    MeshFilter[] m_WheelMeshFilter;
    Renderer[] m_WheelMeshRenderer;

    float[] m_SpringPosition;
    float[] m_SpringFullDistance;
    float[] m_SpringRestDistance;
    float[] m_SpringForce;
    float[] m_SpringDamper;
    Mesh[] m_WheelMesh;
    Material[] m_WheelMaterial;

    float[] m_WheelGripFactor;
    float[] m_WheelWeight;

    float[] m_WheelCurrentRotation;
    // ---------------------------------
    RaycastHit[] m_WheelRayHit;



    // TERRAIN DATA --------------------
    [Header("TERRAIN DATA")]
    [SerializeField] GameObject m_Terrain;
    Texture2D m_TerrainMask;

    float[] m_CurrentSpeedMultiplier;
    float[] m_CurrentAdditionalGrip;
    // Road
    [SerializeField] float m_RoadSpeedMultiplier = 1.0f;
    [SerializeField] float m_RoadAdditionalGrip = 0.0f;

    // Grass
    [SerializeField] float m_GrassSpeedMultiplier = 0.5f;
    [SerializeField] float m_GrassAdditionalGrip = -0.3f;

    // Mud
    [SerializeField] float m_MudSpeedMultiplier = 0.25f;
    [SerializeField] float m_MudAdditionalGrip = -0.2f;
    // ---------------------------------



    // DEADZONE ------------------------
    [Header("DEADZONE DATA")]
    [SerializeField] float m_DeadzoneWidth = 1000.0f;
    [SerializeField] float m_DeadzoneHeight = 500.0f;
    [SerializeField] Color m_DeadzoneColor = Color.red;
    [SerializeField] Transform m_RespawnTransform;
    // ---------------------------------




    private void Awake()
    {
        m_CarRigidbody = GetComponentInChildren<Rigidbody>();

        m_MoveAction = InputSystem.actions.FindAction("Move");
        m_JumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void Start()
    {
        m_TerrainMask = (Texture2D)m_Terrain.GetComponent<MeshRenderer>().material.GetTexture("_MaskTex");

        UpdateBodyParams();
        //UpdateWheelParams();
    }

    private void Update()
    {
        Update_Jump();

        for (int i = 0; i < m_ActiveWheels.Count; i++)
        {
            ApplyForces(i);
        }

        ApplyMovementInput();

        if (!m_IsGrounded) Update_OnAirRoll();

        Update_Deadzone();
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



    float m_CarVelocitySign = 0;
    void ApplyForces(int i)
    {
        bool l_IsWheelGrounded = Physics.Raycast(m_WheelTransform[i].position, -m_WheelTransform[i].up, out m_WheelRayHit[i], m_SpringPosition[i] + m_SpringFullDistance[i], m_CollisionMask);

        if (l_IsWheelGrounded)
        {
            // Detect terrain mask color ---------------------------------
            if (m_WheelRayHit[i].transform.name == "Terrain")
            {
                if (m_TerrainMask != null)
                {
                    Color l_TerrainMaskColor = m_TerrainMask.GetPixelBilinear(m_WheelRayHit[i].textureCoord.x, m_WheelRayHit[i].textureCoord.y);
                    //Debug.Log(l_TerrainMaskColor);

                    // BLACK
                    m_CurrentSpeedMultiplier[i] = m_GrassSpeedMultiplier;
                    m_CurrentAdditionalGrip[i] = m_GrassAdditionalGrip;
                    // RED
                    if (l_TerrainMaskColor.r > 0)
                    {
                        m_CurrentSpeedMultiplier[i] = Mathf.Lerp(m_CurrentSpeedMultiplier[i], m_RoadSpeedMultiplier, l_TerrainMaskColor.r);
                        m_CurrentAdditionalGrip[i] = Mathf.Lerp(m_CurrentAdditionalGrip[i], m_RoadAdditionalGrip, l_TerrainMaskColor.r);
                    }
                    // BLUE
                    if (l_TerrainMaskColor.b > 0)
                    {
                        m_CurrentSpeedMultiplier[i] = Mathf.Lerp(m_CurrentSpeedMultiplier[i], m_MudSpeedMultiplier, l_TerrainMaskColor.b);
                        m_CurrentAdditionalGrip[i] = Mathf.Lerp(m_CurrentAdditionalGrip[i], m_MudAdditionalGrip, l_TerrainMaskColor.b);
                    }
                }
                else
                {
                    m_CurrentSpeedMultiplier[i] = 1.0f;
                    m_CurrentAdditionalGrip[i] = 0.0f;
                }
            }
            else
            {
                m_CurrentSpeedMultiplier[i] = 1.0f;
                m_CurrentAdditionalGrip[i] = 0.0f;
            }

            //Debug.Log("Speed mult: " + m_CurrentSpeedMultiplier[i] + " | Add Grip: " + m_CurrentAdditionalGrip[i]);
            // -----------------------------------------------------------



            // SUSPENSION ------------------------------------------------
            //Vector3 l_SpringDirection = m_WheelRayHit[i].normal;
            Vector3 l_SpringDirection = m_WheelTransform[i].up;

            Vector3 l_WheelWorldVelocity = m_CarRigidbody.GetPointVelocity(m_WheelTransform[i].position);

            float l_Offset = m_SpringPosition[i] + m_SpringRestDistance[i] - m_WheelRayHit[i].distance;

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

            float l_TotalGripFactor = m_WheelGripFactor[i] + m_CurrentAdditionalGrip[i];
            l_TotalGripFactor = Mathf.Clamp01(l_TotalGripFactor);

            float l_DesiredVelocityChange = -l_SteeringVelocity * l_TotalGripFactor;

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

                float l_CarSpeedNormalized = Mathf.Clamp01(Mathf.Abs(l_CarSpeed) / (m_MaxCarSpeed * m_CurrentSpeedMultiplier[i]));

                float l_Verticality = Vector3.Dot(Vector3.up, transform.forward * Mathf.Sign(m_VerticalInput));
                float l_Acceleration = 0.0f;
                if (m_CurrentCarSpeed != 0)
                {
                    l_Acceleration = m_SpeedCurve.Evaluate(l_CarSpeedNormalized) * m_CurrentCarSpeed * (1 - l_Verticality);
                }
                else
                {
                    l_Acceleration = -Mathf.Sign(l_CarSpeed) * 1 / 10 * m_MaxCarSpeed * (1 - m_CurrentAdditionalGrip[i]);
                }

                m_CarRigidbody.AddForceAtPosition(l_AccelerationDirection * l_Acceleration / m_FrontWheelsAmount, m_WheelTransform[i].position);
            }
            // -----------------------------------------------------------



            // JUMP ------------------------------------------------------
            m_CarRigidbody.angularDamping = 0.05f;
            if (m_JumpAction.IsPressed())
            //if (m_JumpTrigger)
            {
                Vector3 l_JumpDirection = m_WheelTransform[i].up;

                m_CarRigidbody.AddForceAtPosition(l_JumpDirection * m_TotalWeight * m_JumpAcceleration, m_WheelTransform[i].position);
                //m_CarRigidbody.angularVelocity = Vector3.zero;

                m_CarVelocitySign = Mathf.Sign(Vector3.Dot(transform.forward, m_CarRigidbody.linearVelocity));
            }
            // -----------------------------------------------------------



            // Rotate wheels ---------------------------------------------
            float l_CarVelocity = Vector3.Dot(m_WheelTransform[i].forward, m_CarRigidbody.linearVelocity);

            m_WheelCurrentRotation[i] += l_CarVelocity / m_SpringRestDistance[i] * Mathf.Rad2Deg * Time.deltaTime;

            m_WheelTransform[i].GetChild(0).localRotation = Quaternion.Euler(m_WheelCurrentRotation[i], 0f, 0f);
            // -----------------------------------------------------------



            // Apply weight to swing obstacle ----------------------------
            if (m_WheelRayHit[i].transform.name == "Columpio")
            {
                Rigidbody l_SwingRigidbody = m_WheelRayHit[i].transform.GetComponent<Rigidbody>();

                l_SwingRigidbody.AddForceAtPosition(-m_WheelTransform[i].up * m_CarRigidbody.mass * -Physics.gravity.y * Time.deltaTime, m_WheelRayHit[i].point);
            }
            // -----------------------------------------------------------
        }
        else
        {
            m_CarRigidbody.angularDamping = 1.0f;

            // WHEELS GRAVITY --------------------------------------------
            Vector3 l_WheelWorldVelocity = m_CarRigidbody.GetPointVelocity(m_WheelTransform[i].position);

            Vector3 l_VerticalSpeed = Vector3.up * -Physics.gravity.y * Time.deltaTime;

            float l_SpringSpeed = Vector3.Dot(m_WheelTransform[i].up, l_VerticalSpeed);

            m_WheelMeshTransform[i].position -= m_WheelTransform[i].up * l_SpringSpeed / m_SpringDamper[i] * Time.deltaTime;
            m_WheelMeshTransform[i].localPosition = new Vector3(0, -m_SpringPosition[i], 0) + Vector3.ClampMagnitude(m_WheelMeshTransform[i].localPosition, m_SpringFullDistance[i] - m_SpringRestDistance[i]);
            // -----------------------------------------------------------



            // Rotate wheels ---------------------------------------------
            float l_CarHorizontalSpeed = new Vector2(m_CarRigidbody.linearVelocity.x, m_CarRigidbody.linearVelocity.z).magnitude;

            m_WheelCurrentRotation[i] += l_CarHorizontalSpeed * m_CarVelocitySign / m_SpringRestDistance[i] * Mathf.Rad2Deg * Time.deltaTime;

            m_WheelTransform[i].GetChild(0).localRotation = Quaternion.Euler(m_WheelCurrentRotation[i], 0f, 0f);
            // -----------------------------------------------------------
        }
    }



    public void UpdateBodyParams()
    {
        m_BodyBehaviour = m_Body.GetComponent<BodyBehaviour>();
        m_BodyMeshFilter = m_Body.GetComponent<MeshFilter>();
        m_BodyRenderer = m_Body.GetComponent<Renderer>();
        m_BodyCollider = m_Body.GetComponent<BoxCollider>();

        m_BodyWeight = m_BodyBehaviour.m_BodyParams.m_BodyWeight;
        m_WheelTurnPower = m_BodyBehaviour.m_BodyParams.m_WheelTurnPower;
        m_ColliderDimensions = m_BodyBehaviour.m_BodyParams.m_ColliderDimensions;
        m_BodyMesh = m_BodyBehaviour.m_BodyParams.m_BodyMesh;
        m_BodyMaterial = m_BodyBehaviour.m_BodyParams.m_BodyMaterial;
        m_FrontWheelsAmount = m_BodyBehaviour.m_BodyParams.m_FrontWheelsAmount;
        m_BackWheelsAmount = m_BodyBehaviour.m_BodyParams.m_BackWheelsAmount;
        m_FrontWheelPosition = m_BodyBehaviour.m_BodyParams.m_FrontWheelPosition;
        m_BackWheelPosition = m_BodyBehaviour.m_BodyParams.m_BackWheelPosition;

        m_BodyMeshFilter.mesh = m_BodyMesh;
        m_BodyRenderer.material = m_BodyMaterial;
        m_BodyCollider.size = m_ColliderDimensions;



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

        UpdateWheelParams();



        CalculateCarWeight();
    }



    public void UpdateWheelParams()
    {
        for (int i = 0; i < m_ActiveWheels.Count; i++)
        {
            GameObject l_Wheel = m_ActiveWheels[i];
            m_WheelBehaviour[i] = l_Wheel.GetComponent<WheelBehaviour>();
            m_WheelTransform[i] = l_Wheel.transform;
            m_WheelMeshFilter[i] = l_Wheel.GetComponentInChildren<MeshFilter>();
            m_WheelMeshRenderer[i] = l_Wheel.GetComponentInChildren<Renderer>();
            m_WheelMeshTransform[i] = m_WheelMeshRenderer[i].transform;

            WheelParams l_WheelParams = m_WheelBehaviour[i].m_WheelParams;
            m_SpringPosition[i] = l_WheelParams.m_SpringPosition;
            m_SpringFullDistance[i] = l_WheelParams.m_SpringFullDistance;
            m_SpringRestDistance[i] = l_WheelParams.m_SpringRestDistance;
            m_SpringForce[i] = l_WheelParams.m_SpringForce;
            m_SpringDamper[i] = l_WheelParams.m_SpringDamper;
            m_WheelMesh[i] = l_WheelParams.m_WheelMesh;
            m_WheelMaterial[i] = l_WheelParams.m_WheelMaterial;
            m_WheelGripFactor[i] = l_WheelParams.m_WheelGripFactor;
            m_WheelWeight[i] = l_WheelParams.m_WheelWeight;

            m_WheelMeshFilter[i].mesh = m_WheelMesh[i];
            m_WheelMeshRenderer[i].material = m_WheelMaterial[i];
        }



        CalculateCarWeight();
    }

    void SetNewWheelsParams(int i)
    {
        m_WheelBehaviour = new WheelBehaviour[i];
        m_WheelTransform = new Transform[i];
        m_WheelMeshTransform = new Transform[i];
        m_WheelMeshFilter = new MeshFilter[i];
        m_WheelMeshRenderer = new Renderer[i];

        m_SpringPosition = new float[i];
        m_SpringFullDistance = new float[i];
        m_SpringRestDistance = new float[i];
        m_SpringForce = new float[i];
        m_SpringDamper = new float[i];
        m_WheelMesh = new Mesh[i];
        m_WheelMaterial = new Material[i];

        m_WheelGripFactor = new float[i];
        m_WheelWeight = new float[i];

        m_WheelRayHit = new RaycastHit[i];



        m_CurrentSpeedMultiplier = new float[i];
        m_CurrentAdditionalGrip = new float[i];



        m_WheelCurrentRotation = new float[i];
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


    bool m_IsJumping = false;
    bool m_WasJumping = false;
    bool m_JumpTrigger = false;
    bool m_IsGrounded = false;
    void Update_Jump()
    {
        bool l_isAllWheelsGrounded = true;

        for (int i = 0; i < m_ActiveWheels.Count; i++)
        {
            if (!Physics.Raycast(m_WheelTransform[i].position, -m_WheelTransform[i].up, m_SpringFullDistance[i], m_CollisionMask))
            {
                l_isAllWheelsGrounded = false;
                break;
            }
        }

        float l_CarVerticalVelocity = m_CarRigidbody.linearVelocity.y;

        //m_IsGrounded = (l_isAllWheelsGrounded || l_CarVerticalVelocity == 0);
        m_IsGrounded = (l_isAllWheelsGrounded || (l_CarVerticalVelocity < 0.1f && l_CarVerticalVelocity > -0.1f));

        m_IsJumping = m_JumpAction.IsPressed() && m_IsGrounded;

        m_JumpTrigger = m_IsJumping && !m_WasJumping;



        // EXECUTE JUMP ------------------------
        //if (m_JumpTrigger)
        {
            //if (l_isAllWheelsGrounded)
            //{
            //    //Debug.Log("All wheels grounded");
            //    //Vector3 l_JumpDirection = transform.up;
            //    //m_CarRigidbody.linearVelocity += l_JumpDirection * m_JumpAcceleration / m_TotalWeight;


            //    //Vector3 l_JumpDirection = transform.up;
            //    //m_CarRigidbody.AddForce(l_JumpDirection * m_JumpAcceleration);


            //    //for (int i = 0; i < m_ActiveWheels.Count; i++)
            //    //{
            //    //    Vector3 l_JumpDirection = m_WheelTransform[i].up;
            //    //    //m_CarRigidbody.AddForceAtPosition(l_JumpDirection * m_JumpAcceleration, m_WheelTransform[i].position);
            //    //    m_CarRigidbody.AddForce(l_JumpDirection * m_JumpAcceleration);
            //    //}
            //    //    m_CarRigidbody.angularVelocity = Vector3.zero;
            //    //m_CarRigidbody.angularDamping = 5.0f;
            //}
            if (l_CarVerticalVelocity == 0)
            {
                //Debug.Log("Car is flipped");
                Vector3 l_JumpDirection = Vector3.up;

                for (int i = 0; i < m_ActiveWheels.Count; i++)
                {
                    m_CarRigidbody.AddForceAtPosition(l_JumpDirection * m_TotalWeight * m_JumpFlipForce,
                                                      m_WheelTransform[i].position + m_WheelTransform[i].forward * 1);
                }
            }
        }
        // -------------------------------------



        m_WasJumping = m_IsJumping;
    }

    void Update_OnAirRoll()
    {
        Vector3 l_PitchRotation = transform.right * m_VerticalInput * Time.deltaTime * 10.0f;
        Vector3 l_RollRotation = -transform.forward * m_HorizontalInput * Time.deltaTime * 10.0f;
        m_CarRigidbody.angularVelocity += l_PitchRotation + l_RollRotation;
    }



    void Update_Deadzone()
    {
        if (Mathf.Abs(transform.position.x) > m_DeadzoneWidth / 2
            || Mathf.Abs(transform.position.z) > m_DeadzoneWidth / 2
            || Mathf.Abs(transform.position.y) > m_DeadzoneHeight / 2)
        {
            transform.position = m_RespawnTransform.position;
            transform.rotation = m_RespawnTransform.rotation;
            StartCoroutine(ResetCar());
        }
    }
    IEnumerator ResetCar()
    {
        m_CarRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        m_CarRigidbody.isKinematic = true;

        yield return new WaitForEndOfFrame();

        m_CarRigidbody.isKinematic = false;
        m_CarRigidbody.constraints = RigidbodyConstraints.None;

        yield return null;
    }



    private void OnEnable()
    {
        m_InputActionAsset.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
        m_InputActionAsset.FindActionMap("Player").Disable();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = m_DeadzoneColor;

        Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_DeadzoneWidth, m_DeadzoneHeight, m_DeadzoneWidth));
    }
}
