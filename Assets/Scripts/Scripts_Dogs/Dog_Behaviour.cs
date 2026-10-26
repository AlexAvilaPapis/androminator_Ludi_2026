using UnityEngine;

public class Dog_Behaviour : MonoBehaviour
{
    [SerializeField] float _SpringStrength = 1.0f;
    [SerializeField] Vector3 _ForcePosition = new Vector3(0,1,0);
    Rigidbody m_Rigidbody;



    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        m_Rigidbody.AddForceAtPosition(Vector3.up * _SpringStrength, transform.position + _ForcePosition);
    }
}
