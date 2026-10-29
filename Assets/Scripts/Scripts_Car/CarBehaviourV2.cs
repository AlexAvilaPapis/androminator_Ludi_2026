using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarBehaviourV2 : MonoBehaviour
{
    // CAR GENERAL
    [Header("General params")]
    [SerializeField] float m_CarPower = 1.0f;

    [SerializeField] BodyParams m_BodyParams;
    [SerializeField] WheelParams m_WheelParams;

    [SerializeField] GameObject m_Body;
    [SerializeField] GameObject[] m_Wheels;

    Rigidbody m_Rigidbody;

    // INPUT
    [Header("Input params")]
    [SerializeField] InputActionAsset m_InputActionAsset;
    InputAction m_MoveAction;
    InputAction m_JumpAction;
    float m_HorizontalInput = 0.0f;
    float m_VerticalInput = 0.0f;

    // WHEELS
    List<WheelController> m_WheelControllers = new List<WheelController>();
    List<WheelCollider> m_WheelColliders = new List<WheelCollider>();
    List<MeshFilter> m_WheelMeshFilters = new List<MeshFilter>();
    List<Renderer> m_WheelMeshRenderers = new List<Renderer>();

    float m_WheelWeight = 1.0f;
    float m_WheelRadius = 0.5f;
    float m_WheelSuspensionDistance = 0.0f;
    float m_WheelForceApplyHeight = 0.0f;
    float m_WheelSpringForce = 50.0f;
    float m_WheelSpringDamper = 0.0f;
    float m_WheelSlip = 1.0f;
    Mesh m_WheelMesh;
    Material m_WheelMaterial;

    // BODY
    MeshFilter m_BodyMeshFilter;
    Renderer m_BodyMeshRenderer;
    BoxCollider m_BodyCollider;

    Mesh m_BodyMesh;
    Material m_BodyMaterial;
    Vector3 m_ColliderDimensions;
    float m_BodyWeight = 1.0f;
    Vector3 m_FrontWheelPosition = Vector3.zero;
    Vector3 m_BackWheelPosition = Vector3.zero;




    void Awake()
    {
        m_MoveAction = InputSystem.actions.FindAction("Move");
        m_JumpAction = InputSystem.actions.FindAction("Jump");
    }
    void Start()
    {
        // GET BODY COMPS
        m_Rigidbody = GetComponent<Rigidbody>();

        // GET BODY COMPS
        m_BodyMeshFilter = m_Body.GetComponent<MeshFilter>();
        m_BodyMeshRenderer = m_Body.GetComponent<MeshRenderer>();
        m_BodyCollider = m_Body.GetComponent<BoxCollider>();

        // GET WHEELS COMPS
        foreach (GameObject i_Wheel in m_Wheels)
        {
            m_WheelColliders.Add(i_Wheel.GetComponent<WheelCollider>());
            m_WheelControllers.Add(i_Wheel.GetComponent<WheelController>());
            m_WheelMeshFilters.Add(i_Wheel.GetComponentInChildren<MeshFilter>());
            m_WheelMeshRenderers.Add(i_Wheel.GetComponentInChildren<Renderer>());
        }

        // GET PARAMS
        GetBodyParams(m_BodyParams);
        GetWheelsParams(m_WheelParams);
    }



    private void Update()
    {
        m_HorizontalInput = m_MoveAction.ReadValue<Vector2>().x;
        m_VerticalInput = m_MoveAction.ReadValue<Vector2>().y;
    }
    private void FixedUpdate()
    {
        Update_Wheels();
    }



    void Update_Wheels()
    {
        foreach (WheelController i_WheelController in m_WheelControllers)
        {
            if (i_WheelController.name.Contains("WheelColliderF"))
            {
                i_WheelController.WheelSteer(m_HorizontalInput);
                i_WheelController.WheelAccelerate(m_VerticalInput * m_CarPower);

                //Vector3 l_WheelRotation = i_WheelController.transform.localRotation.eulerAngles;
                //l_WheelRotation.y = m_HorizontalInput * 30;
                //i_WheelController.transform.localRotation = Quaternion.Euler(l_WheelRotation);
                //m_Rigidbody.AddForceAtPosition(i_WheelController.transform.forward * m_VerticalInput * m_CarPower, i_WheelController.transform.position);
            }
            i_WheelController.UpdateMeshTransform();
        }
    }

    public void GetBodyParams(BodyParams i_BodyParams)
    {
        m_BodyParams = i_BodyParams;

        // to local params
        m_BodyMesh = m_BodyParams.m_BodyMesh;
        m_BodyMaterial = m_BodyParams.m_BodyMaterial;
        m_ColliderDimensions = m_BodyParams.m_ColliderDimensions;
        m_BodyWeight = m_BodyParams.m_BodyWeight;
        m_FrontWheelPosition = m_BodyParams.m_FrontWheelPosition;
        m_BackWheelPosition = m_BodyParams.m_BackWheelPosition;

        // apply params
        m_BodyMeshFilter.mesh = m_BodyMesh;
        m_BodyMeshRenderer.material = m_BodyMaterial;
        m_BodyCollider.size = m_ColliderDimensions;
        m_Rigidbody.mass = m_BodyWeight;
        foreach (GameObject i_Wheel in m_Wheels)
        {
            if (i_Wheel.name.Contains("WheelColliderF"))
                i_Wheel.transform.localPosition = m_FrontWheelPosition;
            if (i_Wheel.name.Contains("WheelColliderB"))
                i_Wheel.transform.localPosition = m_BackWheelPosition;
            if (i_Wheel.name.EndsWith("L"))
                i_Wheel.transform.localPosition = new Vector3(-i_Wheel.transform.localPosition.x, i_Wheel.transform.localPosition.y, i_Wheel.transform.localPosition.z);
        }
    }
    public void GetWheelsParams(WheelParams i_WheelParams)
    {
        m_WheelParams = i_WheelParams;

        // to local params
        m_WheelWeight = m_WheelParams.m_WheelWeight;
        m_WheelRadius = m_WheelParams.m_WheelRadius;
        m_WheelSuspensionDistance = m_WheelParams.m_SpringPosition;
        m_WheelForceApplyHeight = m_WheelParams.m_WheelRadius;
        m_WheelSpringForce = m_WheelParams.m_SpringForce;
        m_WheelSpringDamper = m_WheelParams.m_SpringDamper;
        m_WheelSlip = m_WheelParams.m_WheelSlip;
        m_WheelMesh = m_WheelParams.m_WheelMesh;
        m_WheelMaterial = m_WheelParams.m_WheelMaterial;

        // apply params
        foreach (WheelCollider i_WheelCol in m_WheelColliders)
        {
            i_WheelCol.mass = m_WheelWeight;
            i_WheelCol.radius = m_WheelRadius;
            i_WheelCol.suspensionDistance = m_WheelSuspensionDistance;
            i_WheelCol.forceAppPointDistance = m_WheelForceApplyHeight;

            JointSpring l_SuspensionSpring = i_WheelCol.suspensionSpring;
            l_SuspensionSpring.spring = m_WheelSpringForce;
            l_SuspensionSpring.damper = m_WheelSpringDamper;
            i_WheelCol.suspensionSpring = l_SuspensionSpring;

            WheelFrictionCurve l_ForwardFrictionCurve = i_WheelCol.forwardFriction;
            l_ForwardFrictionCurve.stiffness = m_WheelSlip;
            i_WheelCol.forwardFriction = l_ForwardFrictionCurve;

            WheelFrictionCurve l_SidewaysFrictionCurve = i_WheelCol.sidewaysFriction;
            l_SidewaysFrictionCurve.stiffness = m_WheelSlip;
            i_WheelCol.sidewaysFriction = l_SidewaysFrictionCurve;
        }
        foreach (MeshFilter i_WheelMF in m_WheelMeshFilters)
        {
            i_WheelMF.mesh = m_WheelMesh;
        }
        foreach (Renderer i_WheelRend in m_WheelMeshRenderers)
        {
            i_WheelRend.material = m_WheelMaterial;
        }
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
