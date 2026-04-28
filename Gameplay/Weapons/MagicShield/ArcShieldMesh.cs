using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class ArcShieldMesh : MonoBehaviour
{
    // ═══════════════════════════════════════
    //  Serialized Fields
    // ═══════════════════════════════════════

    [Header("=== Shield Shape ===")]
    [Range(0.5f, 10f)]
    [SerializeField] private float radius = 2f;

    [Range(0.05f, 1f)]
    [SerializeField] private float thickness = 0.25f;

    [Range(10f, 360f)]
    [SerializeField] private float arcAngle = 180f;

    [Range(16, 128)]
    [SerializeField] private int segments = 64;

    [Tooltip("호의 양쪽 끝을 둥글게 처리")]
    [SerializeField] private bool roundedCaps = true;

    [Tooltip("라운드 캡 세그먼트 수")]
    [Range(4, 16)]
    [SerializeField] private int capSegments = 8;

    [Header("=== Appearance ===")]
    [SerializeField] private Color shieldColor = new Color(1f, 0.82f, 0.23f, 1f);
    [SerializeField] private Material shieldMaterial;

    [Tooltip("내구도가 0일 때 쉴드의 알파 값")]
    [Range(0f, 1f)]
    [SerializeField] private float brokenAlpha = 0.2f;

    [Header("=== Durability ===")]
    [Range(1, 20)]
    [SerializeField] private int maxDurability = 5;

    [Range(0.02f, 0.3f)]
    [SerializeField] private float dotRadius = 0.08f;

    [Range(6, 24)]
    [SerializeField] private int dotSegments = 12;

    [SerializeField] private Color dotColor = Color.white;

    [Range(0f, 1f)]
    [SerializeField] private float dotPosition = 0.3f;

    [Range(1f, 45f)]
    [SerializeField] private float dotSpacing = 15f;

    [Header("=== Collision ===")]
    [Range(8, 48)]
    [SerializeField] private int colliderSegments = 16;

    // ═══════════════════════════════════════
    //  Constants
    // ═══════════════════════════════════════

    private const float DEG2RAD = Mathf.PI / 180f;
    private const float PI2 = Mathf.PI * 2f;

    // ═══════════════════════════════════════
    //  Arc mesh
    // ═══════════════════════════════════════

    private MeshFilter _arcMF;
    private MeshRenderer _arcMR;
    private Mesh _arcMesh;

    private Vector3[] _arcVerts;
    private Color[] _arcColors;
    private Vector2[] _arcUVs;
    private int[] _arcTris;

    private bool _arcDirty = true;
    private int _arcAllocVerts;
    private int _arcAllocTris;

    private int _currentDurability = 5;

    // Arc 스냅샷
    private float _sRadius, _sThickness, _sArcAngle, _sBrokenAlpha;
    private int _sSegments, _sCapSegments;
    private Color _sShieldColor;
    private bool _sRoundedCaps;

    // ═══════════════════════════════════════
    //  Dot mesh
    // ═══════════════════════════════════════

    private GameObject _dotGO;
    private MeshFilter _dotMF;
    private MeshRenderer _dotMR;
    private Mesh _dotMesh;

    private Vector3[] _dotVerts;
    private Color[] _dotColors;
    private Vector2[] _dotUVs;
    private int[] _dotTris;

    private bool _dotDirty = true;
    private int _dotAllocVerts;
    private int _dotAllocTris;

    private float[] _dotCos;
    private float[] _dotSin;
    private int _cachedDotSegs;

    private int _sCurDurability, _sMaxDurability, _sDotSegments;
    private float _sDotRadius, _sDotPosition, _sDotSpacing;
    private Color _sDotColor;
    private float _sDotDepRadius, _sDotDepThickness;

    // ═══════════════════════════════════════
    //  Collider
    // ═══════════════════════════════════════

    private EdgeCollider2D _edgeCollider;
    private Vector2[] _colliderPoints;
    private int _colliderAllocPoints;

    // ═══════════════════════════════════════
    //  Lifecycle
    // ═══════════════════════════════════════

    void Awake()
    {
        InitArc();
        InitDot();
        InitCollider();
        BuildDotLUT();
        RebuildArc();
        RebuildDot();
        RebuildCollider();
        SnapshotArc();
        SnapshotDot();
    }

    void Update()
    {
        DetectChanges();

        if (_arcDirty)
        {
            RebuildArc();
            RebuildCollider();
            _arcDirty = false;
            SnapshotArc();
            _dotDirty = true;
        }

        if (_dotDirty)
        {
            RebuildDot();
            _dotDirty = false;
            SnapshotDot();
        }
    }

    // ═══════════════════════════════════════
    //  Init
    // ═══════════════════════════════════════

    void InitArc()
    {
        _arcMF = GetComponent<MeshFilter>();
        _arcMR = GetComponent<MeshRenderer>();

        _arcMesh = new Mesh { name = "ArcShield_Arc" };
        _arcMesh.MarkDynamic();
        _arcMF.mesh = _arcMesh;

        _arcMR.material = ResolveMaterial();
        _arcMR.sortingOrder = 5;
    }

    void InitDot()
    {
        _dotGO = new GameObject("ShieldDots");
        _dotGO.transform.SetParent(transform, false);

        _dotMF = _dotGO.AddComponent<MeshFilter>();
        _dotMR = _dotGO.AddComponent<MeshRenderer>();

        _dotMesh = new Mesh { name = "ArcShield_Dots" };
        _dotMesh.MarkDynamic();
        _dotMF.mesh = _dotMesh;

        _dotMR.material = ResolveMaterial();
        _dotMR.sortingOrder = 6;
    }

    void InitCollider()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
        _edgeCollider.isTrigger = true;
    }

    Material ResolveMaterial()
    {
        if (shieldMaterial != null) return shieldMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                     ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default")
                     ?? Shader.Find("Sprites/Default");
        return new Material(shader);
    }

    // ═══════════════════════════════════════
    //  Change Detection
    // ═══════════════════════════════════════

    void DetectChanges()
    {
        if (radius != _sRadius
            || thickness != _sThickness
            || arcAngle != _sArcAngle
            || segments != _sSegments
            || shieldColor.GetHashCode() != _sShieldColor.GetHashCode()
            || brokenAlpha != _sBrokenAlpha
            || _currentDurability != _sCurDurability
            || roundedCaps != _sRoundedCaps
            || capSegments != _sCapSegments)
        {
            _arcDirty = true;
        }

        if (!_dotDirty)
        {
            if (_currentDurability != _sCurDurability
                || maxDurability != _sMaxDurability
                || dotSegments != _sDotSegments
                || dotRadius != _sDotRadius
                || dotPosition != _sDotPosition
                || dotSpacing != _sDotSpacing
                || dotColor.GetHashCode() != _sDotColor.GetHashCode()
                || radius != _sDotDepRadius
                || thickness != _sDotDepThickness)
            {
                _dotDirty = true;
            }
        }
    }

    void SnapshotArc()
    {
        _sRadius = radius; _sThickness = thickness;
        _sArcAngle = arcAngle; _sSegments = segments;
        _sShieldColor = shieldColor; _sBrokenAlpha = brokenAlpha;
        _sRoundedCaps = roundedCaps; _sCapSegments = capSegments;
    }

    void SnapshotDot()
    {
        _sCurDurability = _currentDurability; _sMaxDurability = maxDurability;
        _sDotSegments = dotSegments; _sDotRadius = dotRadius;
        _sDotPosition = dotPosition; _sDotSpacing = dotSpacing;
        _sDotColor = dotColor;
        _sDotDepRadius = radius; _sDotDepThickness = thickness;
    }

    // ═══════════════════════════════════════
    //  Dot LUT
    // ═══════════════════════════════════════

    void BuildDotLUT()
    {
        if (_cachedDotSegs == dotSegments && _dotCos != null) return;

        _dotCos = new float[dotSegments];
        _dotSin = new float[dotSegments];
        float step = PI2 / dotSegments;

        for (int i = 0; i < dotSegments; i++)
        {
            float a = step * i;
            _dotCos[i] = Mathf.Cos(a);
            _dotSin[i] = Mathf.Sin(a);
        }
        _cachedDotSegs = dotSegments;
    }

    // ═══════════════════════════════════════
    //  Arc Rebuild (with optional rounded caps)
    // ═══════════════════════════════════════

    void RebuildArc()
    {
        int ppr = segments + 1;

        // 캡 버텍스/트라이 수 계산 (캡 1개: center + capSegments+1 rim = capSegments+2 verts, capSegments tris)
        int capVertsEach = roundedCaps ? (capSegments + 2) : 0;
        int capTrisEach = roundedCaps ? (capSegments * 3) : 0;
        int totalCapVerts = capVertsEach * 2;
        int totalCapTris = capTrisEach * 2;

        int arcVertCount = 2 * ppr;
        int arcTriCount = segments * 6;

        int vertCount = arcVertCount + totalCapVerts;
        int triCount = arcTriCount + totalCapTris;

        if (_arcVerts == null || _arcAllocVerts < vertCount)
        {
            _arcVerts = new Vector3[vertCount];
            _arcColors = new Color[vertCount];
            _arcUVs = new Vector2[vertCount];
            _arcAllocVerts = vertCount;
        }
        if (_arcTris == null || _arcAllocTris < triCount)
        {
            _arcTris = new int[triCount];
            _arcAllocTris = triCount;
        }

        float innerR = radius - thickness * 0.5f;
        float outerR = radius + thickness * 0.5f;
        float halfArc = arcAngle * 0.5f;
        float angStep = arcAngle / segments;
        float capR = thickness * 0.5f;

        float alpha = _currentDurability <= 0 ? brokenAlpha : shieldColor.a;

        Color outerCol = shieldColor;
        outerCol.a = alpha;

        Color innerCol;
        innerCol.r = shieldColor.r * 0.7f;
        innerCol.g = shieldColor.g * 0.7f;
        innerCol.b = shieldColor.b * 0.7f;
        innerCol.a = alpha;

        // 캡에 사용할 평균 색상
        Color capCol;
        capCol.r = (outerCol.r + innerCol.r) * 0.5f;
        capCol.g = (outerCol.g + innerCol.g) * 0.5f;
        capCol.b = (outerCol.b + innerCol.b) * 0.5f;
        capCol.a = alpha;

        // ── 메인 호 ──
        for (int i = 0; i < ppr; i++)
        {
            float rad = (-halfArc + angStep * i) * DEG2RAD;
            float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
            float t = (float)i / segments;

            int ii = i, oi = ppr + i;

            _arcVerts[ii].x = c * innerR; _arcVerts[ii].y = s * innerR; _arcVerts[ii].z = 0f;
            _arcVerts[oi].x = c * outerR; _arcVerts[oi].y = s * outerR; _arcVerts[oi].z = 0f;

            _arcUVs[ii].x = t; _arcUVs[ii].y = 0f;
            _arcUVs[oi].x = t; _arcUVs[oi].y = 1f;

            _arcColors[ii] = innerCol;
            _arcColors[oi] = outerCol;
        }

        // ── 호 트라이앵글 ──
        int ti = 0;
        for (int i = 0; i < segments; i++)
        {
            int i0 = i, i1 = i + 1, o0 = ppr + i, o1 = ppr + i + 1;
            _arcTris[ti] = i0; _arcTris[ti+1] = o0; _arcTris[ti+2] = o1;
            _arcTris[ti+3] = i0; _arcTris[ti+4] = o1; _arcTris[ti+5] = i1;
            ti += 6;
        }

        // ── 라운드 캡 ──
        if (roundedCaps)
        {
            // 캡은 호의 양쪽 끝에서 inner→outer를 잇는 반원.
            // 캡 중심: inner와 outer의 중점 (radius 위치)
            // 캡 반지름: thickness / 2
            // 반원은 호가 없는 바깥쪽으로 열림.
            //
            // 반지름 방향 각도 = endAngle (원점에서 캡 중심을 향하는 각도)
            // inner 점: endAngle 방향으로 radius - capR
            // outer 점: endAngle 방향으로 radius + capR
            // inner 점의 극좌표 각도 (캡 중심 기준): endAngle + π (= 중심에서 안쪽)
            // outer 점의 극좌표 각도 (캡 중심 기준): endAngle (= 중심에서 바깥쪽)

            // --- 시작 캡 (각도 = -halfArc) ---
            // 호가 반시계(+) 방향으로 진행하므로, 시작 캡의 바깥쪽 = 시계(-) 방향
            // outer(endAngle) → 시계 방향 반원 → inner(endAngle+π)
            {
                float ea = -halfArc * DEG2RAD;
                float cx = Mathf.Cos(ea) * radius;
                float cy = Mathf.Sin(ea) * radius;

                // outer 각도(캡 로컬) = ea, inner 각도 = ea + π
                // 시계 방향 = ea → ea - π (= ea + π를 시계로 돌아감)
                float sweepFrom = ea;       // outer 쪽에서 시작
                float sweepTo = ea - Mathf.PI; // 시계 방향으로 inner까지

                int vBase = arcVertCount;
                _arcVerts[vBase].x = cx; _arcVerts[vBase].y = cy; _arcVerts[vBase].z = 0f;
                _arcColors[vBase] = capCol;
                _arcUVs[vBase].x = 0.5f; _arcUVs[vBase].y = 0.5f;

                for (int s = 0; s <= capSegments; s++)
                {
                    float t2 = (float)s / capSegments;
                    float a = Mathf.Lerp(sweepFrom, sweepTo, t2);
                    int vi = vBase + 1 + s;
                    _arcVerts[vi].x = cx + Mathf.Cos(a) * capR;
                    _arcVerts[vi].y = cy + Mathf.Sin(a) * capR;
                    _arcVerts[vi].z = 0f;
                    _arcColors[vi] = capCol;
                    _arcUVs[vi].x = 0.5f + Mathf.Cos(a) * 0.5f;
                    _arcUVs[vi].y = 0.5f + Mathf.Sin(a) * 0.5f;
                }

                for (int s = 0; s < capSegments; s++)
                {
                    _arcTris[ti] = vBase;
                    _arcTris[ti+1] = vBase + 1 + s + 1;
                    _arcTris[ti+2] = vBase + 1 + s;
                    ti += 3;
                }
            }

            // --- 끝 캡 (각도 = +halfArc) ---
            // 호 진행 방향 쪽이 바깥 = 반시계(+) 방향
            // inner(ea+π) → 반시계 방향 반원 → outer(ea)
            // = ea → ea + π (반시계)
            {
                float ea = halfArc * DEG2RAD;
                float cx = Mathf.Cos(ea) * radius;
                float cy = Mathf.Sin(ea) * radius;

                float sweepFrom = ea;       // outer 쪽에서 시작
                float sweepTo = ea + Mathf.PI; // 반시계 방향으로 inner까지

                int vBase = arcVertCount + capVertsEach;
                _arcVerts[vBase].x = cx; _arcVerts[vBase].y = cy; _arcVerts[vBase].z = 0f;
                _arcColors[vBase] = capCol;
                _arcUVs[vBase].x = 0.5f; _arcUVs[vBase].y = 0.5f;

                for (int s = 0; s <= capSegments; s++)
                {
                    float t2 = (float)s / capSegments;
                    float a = Mathf.Lerp(sweepFrom, sweepTo, t2);
                    int vi = vBase + 1 + s;
                    _arcVerts[vi].x = cx + Mathf.Cos(a) * capR;
                    _arcVerts[vi].y = cy + Mathf.Sin(a) * capR;
                    _arcVerts[vi].z = 0f;
                    _arcColors[vi] = capCol;
                    _arcUVs[vi].x = 0.5f + Mathf.Cos(a) * 0.5f;
                    _arcUVs[vi].y = 0.5f + Mathf.Sin(a) * 0.5f;
                }

                for (int s = 0; s < capSegments; s++)
                {
                    _arcTris[ti] = vBase;
                    _arcTris[ti+1] = vBase + 1 + s;
                    _arcTris[ti+2] = vBase + 1 + s + 1;
                    ti += 3;
                }
            }
        }

        _arcMesh.Clear();
        _arcMesh.SetVertices(_arcVerts, 0, vertCount);
        _arcMesh.SetColors(_arcColors, 0, vertCount);
        _arcMesh.SetUVs(0, _arcUVs, 0, vertCount);
        _arcMesh.SetTriangles(_arcTris, 0, triCount, 0);
        _arcMesh.RecalculateBounds();
    }

    // ═══════════════════════════════════════
    //  Collider Rebuild
    // ═══════════════════════════════════════

    void RebuildCollider()
    {
        int pointCount = colliderSegments + 1;

        if (_colliderPoints == null || _colliderAllocPoints < pointCount)
        {
            _colliderPoints = new Vector2[pointCount];
            _colliderAllocPoints = pointCount;
        }

        float halfArc = arcAngle * 0.5f;
        float angStep = arcAngle / colliderSegments;

        for (int i = 0; i < pointCount; i++)
        {
            float rad = (-halfArc + angStep * i) * DEG2RAD;
            _colliderPoints[i].x = Mathf.Cos(rad) * radius;
            _colliderPoints[i].y = Mathf.Sin(rad) * radius;
        }

        _edgeCollider.edgeRadius = thickness * 0.5f;
        _edgeCollider.SetPoints(new System.Collections.Generic.List<Vector2>(_colliderPoints));
    }

    // ═══════════════════════════════════════
    //  Dot Rebuild
    // ═══════════════════════════════════════

    void RebuildDot()
    {
        if (dotSegments != _cachedDotSegs) BuildDotLUT();

        int totalDots = _currentDurability > 0 ? _currentDurability : 0;
        int vertsPer = dotSegments + 1;
        int vertCount = totalDots * vertsPer;
        int triCount = totalDots * dotSegments * 3;

        if (totalDots == 0)
        {
            _dotMesh.Clear();
            return;
        }

        if (_dotVerts == null || _dotAllocVerts < vertCount)
        {
            _dotVerts = new Vector3[vertCount];
            _dotColors = new Color[vertCount];
            _dotUVs = new Vector2[vertCount];
            _dotAllocVerts = vertCount;
        }

        if (_dotTris == null || _dotAllocTris < triCount
            || totalDots != _sCurDurability || dotSegments != _sDotSegments)
        {
            if (_dotTris == null || _dotTris.Length < triCount)
            {
                _dotTris = new int[triCount];
                _dotAllocTris = triCount;
            }

            int dti = 0;
            for (int d = 0; d < totalDots; d++)
            {
                int vBase = d * vertsPer;
                for (int s = 0; s < dotSegments; s++)
                {
                    int next = (s + 1) % dotSegments;
                    _dotTris[dti] = vBase;
                    _dotTris[dti+1] = vBase + 1 + s;
                    _dotTris[dti+2] = vBase + 1 + next;
                    dti += 3;
                }
            }
        }

        float innerR = radius - thickness * 0.5f;
        float outerR = radius + thickness * 0.5f;
        float placeR = Mathf.Lerp(innerR, outerR, dotPosition);
        float span = (totalDots > 1) ? (totalDots - 1) * dotSpacing : 0f;
        float startAng = -span * 0.5f;

        for (int d = 0; d < totalDots; d++)
        {
            float dAng = (totalDots == 1) ? 0f : startAng + dotSpacing * d;
            float dRad = dAng * DEG2RAD;
            float cx = Mathf.Cos(dRad) * placeR;
            float cy = Mathf.Sin(dRad) * placeR;

            int vBase = d * vertsPer;

            _dotVerts[vBase].x = cx; _dotVerts[vBase].y = cy; _dotVerts[vBase].z = 0f;
            _dotColors[vBase] = dotColor;
            _dotUVs[vBase].x = 0.5f; _dotUVs[vBase].y = 0.5f;

            for (int s = 0; s < dotSegments; s++)
            {
                int vi = vBase + 1 + s;
                _dotVerts[vi].x = cx + _dotCos[s] * dotRadius;
                _dotVerts[vi].y = cy + _dotSin[s] * dotRadius;
                _dotVerts[vi].z = 0f;
                _dotColors[vi] = dotColor;
                _dotUVs[vi].x = 0.5f + _dotCos[s] * 0.5f;
                _dotUVs[vi].y = 0.5f + _dotSin[s] * 0.5f;
            }
        }

        _dotMesh.Clear();
        _dotMesh.SetVertices(_dotVerts, 0, vertCount);
        _dotMesh.SetColors(_dotColors, 0, vertCount);
        _dotMesh.SetUVs(0, _dotUVs, 0, vertCount);
        _dotMesh.SetTriangles(_dotTris, 0, triCount, 0);
        _dotMesh.RecalculateBounds();
    }

    // ═══════════════════════════════════════
    //  Public API
    // ═══════════════════════════════════════

    public void SetRadius(float v)
    {
        v = Mathf.Max(0.1f, v);
        if (radius != v) { radius = v; _arcDirty = true; }
    }

    public void SetThickness(float v)
    {
        v = Mathf.Max(0.01f, v);
        if (thickness != v) { thickness = v; _arcDirty = true; }
    }

    public void SetColor(Color c)
    {
        if (shieldColor.GetHashCode() != c.GetHashCode()) { shieldColor = c; _arcDirty = true; }
    }

    public void SetArcAngle(float v)
    {
        v = Mathf.Clamp(v, 10f, 360f);
        if (arcAngle != v) { arcAngle = v; _arcDirty = true; }
    }

    public void SetBrokenAlpha(float v)
    {
        v = Mathf.Clamp01(v);
        if (brokenAlpha != v) { brokenAlpha = v; if (_currentDurability <= 0) _arcDirty = true; }
    }

    public void SetRoundedCaps(bool v)
    {
        if (roundedCaps != v) { roundedCaps = v; _arcDirty = true; }
    }

    public void SetDurability(int v)
    {
        v = Mathf.Clamp(v, 0, maxDurability);
        if (_currentDurability != v) { _currentDurability = v; _arcDirty = true; _dotDirty = true; }
    }

    public void TakeDamage()
    {
        if (_currentDurability > 0) { _currentDurability--; _arcDirty = true; _dotDirty = true; }
    }

    public void Repair()
    {
        if (_currentDurability < maxDurability) { _currentDurability++; _arcDirty = true; _dotDirty = true; }
    }

    public void RepairFull()
    {
        if (_currentDurability != maxDurability) { _currentDurability = maxDurability; _arcDirty = true; _dotDirty = true; }
    }

    public void SetMaxDurability(int v)
    {
        v = Mathf.Max(1, v);
        if (maxDurability != v)
        {
            maxDurability = v;
            if (_currentDurability > maxDurability) _currentDurability = maxDurability;
            _arcDirty = true;
            _dotDirty = true;
        }
    }

    public void SetDotColor(Color c)
    {
        if (dotColor.GetHashCode() != c.GetHashCode()) { dotColor = c; _dotDirty = true; }
    }

    public bool IsBroken => _currentDurability <= 0;
    public int CurrentDurability => _currentDurability;
}