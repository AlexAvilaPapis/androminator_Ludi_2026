using UnityEngine;
using UnityEngine.LowLevelPhysics;

[CreateAssetMenu(fileName = "BodyParams", menuName = "Scriptable Objects/BodyParams")]
public class BodyParams : ScriptableObject
{
    [Header("Physics variables")]
    [Tooltip("Mass in kilograms")] public float m_BodyWeight = 1.0f;
    [Tooltip("direction change speed")] public float m_WheelTurnPower = 5.0f;

    [Header("Model variables")]
    public Mesh m_BodyMesh;
    public Material m_BodyMaterial;

    [Header("Wheel position variables")]
    public int m_FrontWheelsAmount = 2;
    public int m_BackWheelsAmount = 2;

    [Tooltip("Horizontal, Vertical, Frontal")] public Vector3 m_FrontWheelPosition = new Vector3(0.8f, -0.5f, 1.0f);
    [Tooltip("Horizontal, Vertical, Frontal")] public Vector3 m_BackWheelPosition = new Vector3(0.8f, -0.5f, -1.0f);
}
