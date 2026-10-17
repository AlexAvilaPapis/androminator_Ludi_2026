using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class ContactEntry
{
    public ContactObstacleType type;
    public GameObject prefab;
}

[Serializable]
public class JumpEntry
{
    public JumpObstacleType type;
    public GameObject prefab;
}

public class ObstacleManager : MonoBehaviour
{
    private int MAX_OBSTACLES = 10;
    private const int CONTACT_COUNT = 4;
    private Obstacle[] order = new Obstacle[4];

    [SerializeField] private List<ContactEntry> contactPrefabs = new List<ContactEntry>();
    [SerializeField] private List<JumpEntry> jumpPrefabs = new List<JumpEntry>();

    private Dictionary<ContactObstacleType, GameObject> contactMap;
    private Dictionary<JumpObstacleType, GameObject> jumpMap;

    void Awake()
    {
        contactMap = new Dictionary<ContactObstacleType, GameObject>();
        foreach (var e in contactPrefabs) contactMap[e.type] = e.prefab;

        jumpMap = new Dictionary<JumpObstacleType, GameObject>();
        foreach (var e in jumpPrefabs) jumpMap[e.type] = e.prefab;
    }

    void Start()
    {
        GenerateOrder();
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GenerateOrder();
        }
    }

    public void GenerateOrder()
    {
        order = Generator.GetRandomOrder(MAX_OBSTACLES, CONTACT_COUNT);
        for (int i = 0; i < order.Length; i++)
        {
            Debug.Log($"Obstacle {i}: {order[i]}");
            
        }
        //SpawnOrder();
    }


    public GameObject GetPrefab(Obstacle o)
    {
        GameObject prefab;
        bool found = o.IsContact
            ? contactMap.TryGetValue(o.Contact, out prefab)
            : jumpMap.TryGetValue(o.Jump, out prefab);

        if (!found || prefab == null)
        {
            Debug.LogWarning($"No prefab assigned for {o}");
            return null;
        }
        return prefab;
    }

    public void SpawnOrder(float spacing = 10f)
    {
        for (int i = 0; i < order.Length; i++)
        {
            GameObject prefab = GetPrefab(order[i]);
            if (prefab == null) continue;

            Vector3 pos = transform.position + Vector3.forward * (i * spacing);
            Instantiate(prefab, pos, Quaternion.identity, transform);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        foreach (ContactObstacleType t in Enum.GetValues(typeof(ContactObstacleType)))
            if (!contactPrefabs.Exists(e => e.type == t))
                contactPrefabs.Add(new ContactEntry { type = t });

        foreach (JumpObstacleType t in Enum.GetValues(typeof(JumpObstacleType)))
            if (!jumpPrefabs.Exists(e => e.type == t))
                jumpPrefabs.Add(new JumpEntry { type = t });
    }
#endif
}