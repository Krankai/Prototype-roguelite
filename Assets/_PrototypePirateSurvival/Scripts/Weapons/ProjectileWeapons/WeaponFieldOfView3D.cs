using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;
using UnityEngine;

public class WeaponFieldOfView3D : MonoBehaviour, MMEventListener<MMGameEvent>
{
    public bool IsActivated;

    [Header("Visual")]
    public float ArcRadius = 5f;
    public float ArcCentralAngle = 30f;

    [Header("Mesh")]
    [SerializeField]
    private MeshFilter _fovMeshFilter;
    [SerializeField]
    private float _meshResolution = 0.5f;
    private Mesh _fovMesh;

    [Header("Collider")]
    [SerializeField]
    private MeshCollider _meshCollider;


    protected virtual void Start()
    {
        if (_fovMeshFilter == default)
        {
            _fovMeshFilter = gameObject.MMGetComponentNoAlloc<MeshFilter>();
        }

        if (_meshCollider == default)
        {
            _meshCollider = gameObject.MMGetComponentNoAlloc<MeshCollider>();
        }    

        Initialization();
    }

    protected virtual void Initialization()
    {
        _fovMesh = new Mesh { name = $"Fov Mesh [{gameObject.name}]" };
        _fovMeshFilter.mesh = _fovMesh;

        Invoke(nameof(Visualize), 1f);
    }

    public virtual void Visualize()
    {
        if (IsActivated)
        {
            DrawFieldOfViewMesh();
        }
        else
        {
            EraseFieldOfViewMesh();
        }
    }

    protected virtual void DrawFieldOfViewMesh()
    {
        int stepCount = Mathf.RoundToInt(ArcCentralAngle * _meshResolution);
        float stepAngleSize = ArcCentralAngle / stepCount;

        var viewPoints = new List<Vector3>();

        var character = transform.GetComponentInParent<Character>();
        var elevation = (character != default) ? character.transform.position.y - 0.6f : 0f;

        // TODO: auto-set this
        //var elevation = (character.CharacterType == Character.CharacterTypes.Player) ? -0.5f : -1.5f;

        var pointInArc = Vector3.zero;

        var originPoint = transform.position;
        var originDirection = Vector3.right;
        var currentRotationAngle = Vector3.SignedAngle(originDirection, transform.forward, -transform.up);

        for (int i = 0; i < stepCount + 1; ++i)
        {
            var angle = currentRotationAngle - ArcCentralAngle / 2f + stepAngleSize * i;

            float x = originPoint.x + ArcRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = originPoint.z + ArcRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

            pointInArc.x = x;
            pointInArc.z = z;

            if (pointInArc.x is not float.NaN && pointInArc.z is not float.NaN)
            {
                viewPoints.Add(pointInArc);
            }
        }

        var vertexCount = viewPoints.Count + 1;
        var vertices = new Vector3[vertexCount];
        var triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = transform.InverseTransformPoint(originPoint);
        vertices[0].y = elevation;

        for (int i = 0, count = vertexCount - 1; i < count; ++i)
        {
            var localViewPoint = transform.InverseTransformPoint(viewPoints[i]);
            vertices[i + 1] = localViewPoint;
            vertices[i + 1].y = elevation;

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = i + 1;
            }
        }

        _fovMesh.Clear();
        _fovMesh.vertices = vertices;
        _fovMesh.triangles = triangles;
        _fovMesh.RecalculateNormals();

        _meshCollider.sharedMesh = _fovMesh;
    }

    protected virtual void EraseFieldOfViewMesh()
    {
        _fovMesh.Clear();
        _fovMesh.vertices = default;
        _fovMesh.triangles = default;
        _fovMesh.RecalculateNormals();

        _meshCollider.sharedMesh = default;
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        if (eventType.EventName.Equals("VisualizeFieldOfView", System.StringComparison.OrdinalIgnoreCase))
        {
            Visualize();
        }
    }


    #region Debug Visualization
    //private void OnDrawGizmos()
    //{
    //    if (IsActivated)
    //    {
    //        var origin = transform.position;
    //        var direction = transform.forward;

    //        Gizmos.color = Color.blue;
    //        Gizmos.DrawLine(origin, origin + 5 * direction);
    //    }
    //}
    #endregion Debug Visualization
}
