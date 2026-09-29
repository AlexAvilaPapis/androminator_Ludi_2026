using UnityEngine;

[CreateAssetMenu(fileName = "WheelParams", menuName = "Scriptable Objects/WheelParams")]
public class WheelParams : ScriptableObject
{
    public float m_SpringFullDistance = 1.0f;
    public float m_SpringRestDistance = 0.7f;
    public float m_SpringForce = 10.0f;
    public float m_SpringDamper = 0.5f;
}
