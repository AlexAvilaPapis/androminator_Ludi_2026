using UnityEngine;

[CreateAssetMenu(fileName = "WheelParams", menuName = "Scriptable Objects/WheelParams")]
public class WheelParams : ScriptableObject
{
    [Header("Suspension variables")]
    [Tooltip("Max spring distance")]    public float m_SpringFullDistance = 1.0f;
    [Tooltip("Radius of the wheel")]    public float m_SpringRestDistance = 0.7f;
    [Tooltip("Resistance to change")]   public float m_SpringForce = 10.0f;
    [Tooltip("Elasticity of spring")]   public float m_SpringDamper = 0.5f;

    [Header("Steering variables")]
    [Tooltip("Drift resist force")]     public float m_WheelGripFactor = 0.5f;
    [Tooltip("Mass in kilograms")]      public float m_WheelWeight = 1.0f;

    [Header("Model variables")]
    public Mesh m_WheelMesh;
    public Material m_WheelMaterial;
}
