using UnityEngine;

public class DecalProyector : MonoBehaviour
{
    [Header("Decal")]
    [SerializeField, Tooltip("Texture must be readable")] Texture2D m_DecalMask;
    [SerializeField, Tooltip("Size in pixels")] int m_DecalMaskSize = 128;
    [SerializeField] Color m_TintColor = Color.blue;

    [Header("Raycast")]
    [SerializeField] LayerMask m_HitLayerMask;
    [SerializeField] float m_RaycastDistance = 10f;

    [Header("Terrain")]
    [SerializeField] string m_TerrainName = "Terrain";
    [SerializeField, Tooltip("Texture format: RGBA32")] string m_TextureProperty = "_MaskTex";

    [Header("Gizmo")]
    [SerializeField] float m_TerrainMeshWidth = 1000.0f;
    [SerializeField] int m_TerrainTextureWidth = 1024;

    Material m_TerrainMaterial;
    Texture2D m_TerrainMaskTexture;
    Texture2D m_DupeTerrainMaskTexture;
    Color32[] m_TerrainMaskPixels;
    Color32[] m_DecalMaskPixels;

    private void Awake()
    {
        PaintDecal();
    }
    private void Start()
    {
        //PaintDecal();
    }

    public void PaintDecal()
    {
        // RAYCAST TO TERRAIN -----------
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit l_Hit, m_RaycastDistance, m_HitLayerMask);
        if (!l_Hit.transform.name.Equals(m_TerrainName))
            return;
        // ------------------------------


        // GET TERRAIN MASK TEXTURE -----
        Renderer l_TerrainRenderer = l_Hit.transform.GetComponent<Renderer>();
        m_TerrainMaterial = l_TerrainRenderer.material;

        Texture l_TerrainMaskTexture = m_TerrainMaterial.GetTexture(m_TextureProperty);
        m_TerrainMaskTexture = (Texture2D)l_TerrainMaskTexture;
        // ------------------------------


        // DUPLICATE TERRAIN MASK -------
        m_DupeTerrainMaskTexture = new Texture2D(m_TerrainMaskTexture.width, m_TerrainMaskTexture.height, TextureFormat.RGBA32, false);
        m_DupeTerrainMaskTexture.SetPixels32(m_TerrainMaskTexture.GetPixels32());
        m_DupeTerrainMaskTexture.Apply(false, false);
        // ------------------------------


        // GET MASKS PIXELS -------------
        m_TerrainMaskPixels = m_DupeTerrainMaskTexture.GetPixels32();
        m_DecalMaskPixels = m_DecalMask.GetPixels32();
        // ------------------------------


        int l_TerrainMaskWidth = m_DupeTerrainMaskTexture.width;
        int l_TerrainMaskHeight = m_DupeTerrainMaskTexture.height;

        int l_DecalMaskWidth = m_DecalMask.width;
        int l_DecalMaskHeight = m_DecalMask.height;


        // UV COORDS TO PIXEL INDEX -----
        Vector2 l_HitUVCoords = l_Hit.textureCoord;

        int l_HitPixelIndexX = Mathf.RoundToInt(l_HitUVCoords.x * (l_TerrainMaskWidth - 1));
        int l_HitPixelIndexY = Mathf.RoundToInt(l_HitUVCoords.y * (l_TerrainMaskHeight - 1));
        // ------------------------------


        // DECAL INTO TERRAIN COORDS ----
        int l_DecalWidth = Mathf.Clamp(m_DecalMaskSize, 1, l_TerrainMaskWidth);
        int l_DecalHeight = Mathf.Clamp(Mathf.RoundToInt((float)l_DecalWidth * l_DecalMaskHeight / l_DecalMaskWidth), 1, l_TerrainMaskHeight);

        int l_DecalStartCoordsX = l_HitPixelIndexX - l_DecalWidth / 2;
        int l_DecalStartCoordsY = l_HitPixelIndexY - l_DecalHeight / 2;
        // ------------------------------


        // ITERATE IN PROYECTION ZONE ---
        for (int y = 0; y < l_DecalHeight; y++)
        {
            // Y IS ROW
            int l_DecalTerrainCoordsY = l_DecalStartCoordsY + y;
            if (l_DecalTerrainCoordsY < 0 || l_DecalTerrainCoordsY >= l_TerrainMaskHeight)
                continue;

            int l_DecalCoordsY = Mathf.Clamp(Mathf.FloorToInt((float)y / l_DecalHeight * l_DecalMaskHeight), 0, l_DecalMaskHeight - 1);

            for (int x = 0; x < l_DecalWidth; x++)
            {
                // X IS COLUMN
                int l_DecalTerrainCoordsX = l_DecalStartCoordsX + x;
                if (l_DecalTerrainCoordsX < 0 || l_DecalTerrainCoordsX >= l_TerrainMaskWidth)
                    continue;

                int l_DecalCoordsX = Mathf.Clamp(Mathf.FloorToInt((float)x / l_DecalWidth * l_DecalMaskWidth), 0, l_DecalMaskWidth - 1);

                // GET CURRENT PIXEL COLOR --
                Color l_DecalMaskColor = m_DecalMaskPixels[l_DecalCoordsY * l_DecalMaskWidth + l_DecalCoordsX];

                int L_TerrainMaskIndex = l_DecalTerrainCoordsY * l_TerrainMaskWidth + l_DecalTerrainCoordsX;

                Color l_TerrainMaskColor = m_TerrainMaskPixels[L_TerrainMaskIndex];

                Color result = l_TerrainMaskColor + m_TintColor * l_DecalMaskColor.r;

                m_TerrainMaskPixels[L_TerrainMaskIndex] = result;
                // --------------------------
            }
        }
        // ------------------------------


        // APPLY AND SEND TO GPU --------
        m_DupeTerrainMaskTexture.SetPixels32(m_TerrainMaskPixels);
        m_DupeTerrainMaskTexture.Apply(false, false);
        // ------------------------------


        // UPDATE TERRAIN MASK ----------
        m_TerrainMaterial.SetTexture("_MaskTex", m_DupeTerrainMaskTexture);
        // ------------------------------
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        float l_GizmoWidth = m_DecalMaskSize * m_TerrainMeshWidth / m_TerrainTextureWidth;

        Gizmos.DrawWireCube(transform.position + (Vector3.down * m_RaycastDistance) / 2, 
                            new Vector3(l_GizmoWidth, m_RaycastDistance, l_GizmoWidth));
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * m_RaycastDistance);
    }
}