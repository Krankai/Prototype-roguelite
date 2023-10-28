using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class ShootingRangeVisualization : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField]
    private CharacterHandleWeapon _handleWeaponAbility;
    [SerializeField]
    private MeshFilter _shootingRangeMeshFilter;

    [Header("Visualization Settings")]
    public float Radius = 0f;
    public float Elevation = 0f;
    [SerializeField]
    [Tooltip("Angle for the shooting cone")]
    private float RangeAngle = 360f;
    [SerializeField]
    private float MeshResolution = 0.5f;

    private Mesh _shootingRangeMesh;
    private bool _isAllowedDrawing = false;


    private void Start()
    {
        _shootingRangeMesh = new Mesh { name = "Shooting Range Mesh" };
        _shootingRangeMeshFilter.mesh = _shootingRangeMesh;

        Invoke(nameof(TriggerDrawing), 1f);
    }

    private void TriggerDrawing()
    {
        _isAllowedDrawing = true;
    }

    private void LateUpdate()
    {
        if (_isAllowedDrawing)
        {
            DrawMeshShootingRange();
        }
    }

    private void DrawMeshShootingRange()
    {
        int stepCount = Mathf.RoundToInt(RangeAngle * MeshResolution);
        float stepAngleSize = RangeAngle / stepCount;

        var viewPoints = new List<Vector3>();
        var pointInArc = new Vector3(0, Elevation, 0);

        var originPoint = _handleWeaponAbility.CurrentWeapon.transform.position;
        var currentRotationAngle = Vector3.SignedAngle(transform.right, Vector3.right, transform.up);

        for (int i = 0; i < stepCount + 1; ++i)
        {
            //var angle = transform.eulerAngles.y - RangeAngle / 2f + stepAngleSize * i;
            var angle = currentRotationAngle - RangeAngle / 2f + stepAngleSize * i;

            float x = originPoint.x + Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = originPoint.z + Radius * Mathf.Sin(angle * Mathf.Deg2Rad);

            pointInArc.x = x;
            pointInArc.z = z;

            viewPoints.Add(pointInArc);
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
