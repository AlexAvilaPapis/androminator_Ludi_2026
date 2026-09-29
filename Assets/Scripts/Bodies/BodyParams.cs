using UnityEngine;
using UnityEngine.LowLevelPhysics;

[CreateAssetMenu(fileName = "BodyParams", menuName = "Scriptable Objects/BodyParams")]
public class BodyParams : ScriptableObject
{
    public float m_BodyWeight = 1.0f;
    public Mesh m_BodyMesh;
    public Material m_BodyMaterial;
}
