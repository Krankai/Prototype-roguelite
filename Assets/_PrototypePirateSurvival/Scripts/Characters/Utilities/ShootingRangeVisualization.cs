using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;
using UnityEngine;

public class ShootingRangeVisualization : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [Header("Bindings")]
    [SerializeField]
    private CharacterHandleWeapon _handleWeaponAbility;
    [SerializeField]
    private MeshFilter _shootingRangeMeshFilter;

    [Header("Visualization Settings")]
    public float Radius = 0f;
    public float Elevation = 0f;
    [Tooltip("Angle for the shooting cone")]
    public float RangeAngle = 360f;

    [Space]
    public bool IsActivated;
    public bool IsFaceRight;

    [Space, SerializeField]
    private float MeshResolution = 0.5f;

    private Mesh _shootingRangeMesh;
    private bool _isAllowedDrawing = false;


    private void Start()
    {
        _shootingRangeMesh = new Mesh { name = "Shooting Range Mesh" };
        _shootingRangeMeshFilter.mesh = _shootingRangeMesh;

        //Invoke(nameof(TriggerDrawing), 1f);
    }

    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }

    private void TriggerDrawing()
    {
        _isAllowedDrawing = true;

        if (IsActivated)
        {
            DrawMeshShootingRange();
        }
        else
        {
            EraseDrawnMeshShootingRange();
        }
    }

    //private void LateUpdate()
    //{
    //    //if (_isAllowedDrawing && IsActivated)
    //    //{
    //    //    DrawMeshShootingRange();
    //    //}
    //}

    private void DrawMeshShootingRange()
    {
        int stepCount = Mathf.RoundToInt(RangeAngle * MeshResolution);
        float stepAngleSize = RangeAngle / stepCount;

        var viewPoints = new List<Vector3>();
        var pointInArc = new Vector3(0, Elevation, 0);

        var originPoint = _handleWeaponAbility.CurrentWeapon.transform.position;
        var originDirection = IsFaceRight ? Vector3.right : Vector3.left;
        var currentRotationAngle = Vector3.SignedAngle(transform.right, originDirection, transform.up);

        for (int i = 0; i < stepCount + 1; ++i)
        {
            //var angle = transform.eulerAngles.y - RangeAngle / 2f + stepAngleSize * i;
            var angle = currentRotationAngle - RangeAngle / 2f + stepAngleSize * i;

            float x = originPoint.x + Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = originPoint.z + Radius * Mathf.Sin(angle * Mathf.Deg2Rad);

            pointInArc.x = x;
            pointInArc.z = z;

            if (pointInArc.x is  not float.NaN && pointInArc.z is not float.NaN)
            {
                viewPoints.Add(pointInArc);
            }
        }

        var vertexCount = viewPoints.Count + 1;
        var vertices = new Vector3[vertexCount];
        var triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = transform.InverseTransformPoint(originPoint);
        vertices[0].y = 0;

        for (int i = 0, count = vertexCount - 1; i < count; ++i)
        {
            var localViewPoint = transform.InverseTransformPoint(viewPoints[i]);
            vertices[i + 1] = localViewPoint;

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = i + 1;
            }
        }

        _shootingRangeMesh.Clear();
        _shootingRangeMesh.vertices = vertices;
        _shootingRangeMesh.triangles = triangles;
        _shootingRangeMesh.RecalculateNormals();
    }

    private void EraseDrawnMeshShootingRange()
    {
        _shootingRangeMesh.Clear();
        _shootingRangeMesh.vertices = default;
        _shootingRangeMesh.triangles = default;
        _shootingRangeMesh.RecalculateNormals();
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        if (eventType.EventName.Equals("DrawShootingRange", System.StringComparison.OrdinalIgnoreCase))
        {
            TriggerDrawing();
        }
    }


    //private void OnDrawGizmos()
    //{
    //    float radius = 5;
    //    float meshResolution = 1f;
    //    const float shootingRangeAngle = 60f;

    //    int stepCount = Mathf.RoundToInt(shootingRangeAngle * meshResolution);
    //    float stepAngleSize = shootingRangeAngle / stepCount;

    //    for (int i = 0; i < stepCount; ++i)
    //    {
    //        var angle = transform.eulerAngles.y - shootingRangeAngle / 2f + stepAngleSize * i;
    //        float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
    //        float z = radius * Mathf.Sin(angle * Mathf.Deg2Rad);

    //        Gizmos.color = UnityEngine.Color.blue;
    //        Gizmos.DrawCube(new Vector3(x, 0, z), new Vector3(0.1f, 0.1f, 0.1f));

    //        if (i == 2) break;
    //    }
    //}
}
