using UnityEngine;
using UnityEngine.Events;

public class ColliderDetector : MonoBehaviour
{
    public UnityEvent<Collision> m_OnCollisionEnter;
    public UnityEvent<Collision> m_OnCollisionStay;
    public UnityEvent<Collision> m_OnCollisionExit;

    private void OnCollisionEnter(Collision i_Collision)
    {
        m_OnCollisionEnter?.Invoke(i_Collision);
        Debug.Log("aaaaaaa");
    }
    private void OnCollisionStay(Collision i_Collision)
    {
        m_OnCollisionStay?.Invoke(i_Collision);
        Debug.Log("bbbbbb");
    }
    private void OnCollisionExit(Collision i_Collision)
    {
        m_OnCollisionExit?.Invoke(i_Collision);
        Debug.Log("cccccc");
    }
}
