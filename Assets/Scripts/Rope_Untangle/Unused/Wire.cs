using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter _meshFilter;
    
    [SerializeField] Transform startTransform, endTransform;
    [SerializeField] int segmentCount = 10;
    [SerializeField] float totalLength = 10;

    [SerializeField] float radius = 0.5f;
    [SerializeField] int sides = 4; 

    [SerializeField] float totalWt = 10;
    [SerializeField] float drag = 1;
    [SerializeField] float angularDrag = 1;

    [SerializeField] bool usePhysics = false;

    Transform[] segments = new Transform[0];
    [SerializeField] Transform segmentParent;
    private int prevSegmentCount;
    private float prevDrag;
    private float prevTotalLength;
    private float prevTotalWt;
    private float prevAngularDrag;
    private float prevRadius;

    private MeshDataRope _meshDataRope;
    
    private Vector3[] vertices;
    private int[,] vertexIndicesMap;
    private bool createTriangle;
    private Mesh mesh;

    private void Start()
    {
        vertices = new Vector3[segmentCount * sides * 3];
        GenerateMesh();
    }

    private void Update()
    {
        if (prevSegmentCount != segmentCount)
        {
            RemoveSegment();
            segments = new Transform[segmentCount];
            GenerateSegments();
            GenerateMesh(); 
        }
        prevSegmentCount = segmentCount;

        
        if (totalLength != prevTotalLength || prevDrag != drag || prevTotalWt != totalWt || prevAngularDrag != angularDrag)
        {
            UpdateWire();
            GenerateMesh();
        }

        prevTotalLength = totalLength;
        prevDrag = drag;
        prevTotalWt = totalWt;
        prevAngularDrag = angularDrag;

        if (prevRadius != radius &&  usePhysics)
        {
            UpdateRadius();
            GenerateMesh();
        }

        prevRadius = radius;
    }
    
    private void UpdateRadius()
    {
        for(int i = 0; i < segments.Length; i++)
        {
            setRadiusOnSegment(segments[i], radius);
        }
    }

    void UpdateMesh()
    {
        GenerateVertices();
    }
    
    void GenerateMesh()
    {
        createTriangle = true;

        if (_meshDataRope == null)
        {
            _meshDataRope = new MeshDataRope(sides, segmentCount + 1, false);
        }
        else
        {
            _meshDataRope.ResetMesh(sides, segmentCount + 1, false);
        }
        
        GenerateIndicesMap(); 
        GenerateVertices();
        
        _meshDataRope.ProcessMesh();
        mesh = _meshDataRope.CreateMesh();
        
        createTriangle = false;
        
        
    }
    
    private void GenerateIndicesMap()
    {
        // +1 for extra circle of vertices after the last segment
        // Same for the sides. We need one extra after the last side
        vertexIndicesMap = new int[segmentCount + 1, sides + 1];
        int meshVertexIndex = 0;
        for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
        {
            for (int sideIndex = 0; sideIndex < sides; sideIndex++)
            {
                vertexIndicesMap[segmentIndex, sideIndex] = meshVertexIndex;
                meshVertexIndex++;
            }
        }
    }
    private void GenerateVertices()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            GenerateCircleVerticesAndTriangles(segments[i],i);
        }
    }

    private void GenerateCircleVerticesAndTriangles(Transform segmentTransform,int segmentIndex)
    {
        float angleDiff = 360 / sides;
        Quaternion diffRotation = Quaternion.FromToRotation(Vector3.forward, segmentTransform.forward);

        for (int sideIndex = 0; sideIndex < sides; sideIndex++)
        {
            float angleInRad = sideIndex * angleDiff * Mathf.Deg2Rad;
            float x = -1 * radius * Mathf.Cos(angleInRad);
            float y = radius * Mathf.Sin(angleInRad);

            Vector3 pointOffSet = new(x, y, 0);
            Vector3 pointRotated = diffRotation * pointOffSet;
            Vector3 pointRotatedAtCenterOfTransform = segmentTransform.position + pointRotated;

            int vertexIndex = segmentIndex * sides + sideIndex;
            vertices[vertexIndex] = pointRotatedAtCenterOfTransform;

            if (createTriangle)
            {
                _meshDataRope.AddVertex(pointRotatedAtCenterOfTransform,new(0,0),vertexIndex);

                bool createThisTriangle = segmentIndex < segmentCount - 1;
                if (createThisTriangle)
                {
                    int currentIncrement = 1;
                    int a = vertexIndicesMap[segmentIndex, sideIndex];
                    int b = vertexIndicesMap[segmentIndex + currentIncrement, sideIndex];
                    int c = vertexIndicesMap[segmentIndex, sideIndex + currentIncrement];
                    int d = vertexIndicesMap[segmentIndex + currentIncrement, sideIndex + currentIncrement];
                    
                    bool isLastGap = sideIndex == sides - 1;
                    if (isLastGap)
                    {
                        c = vertexIndicesMap[segmentIndex, 0];
                        d = vertexIndicesMap[segmentIndex + currentIncrement, 0];
                    }
                    _meshDataRope.AddTriangle(a, d, c); 
                    _meshDataRope. AddTriangle(d, a, b);
                }
                
            }
            
        }
        
    }

    private void setRadiusOnSegment(Transform transform, float radius)
    {
        SphereCollider sphereCollider = transform.GetComponent<SphereCollider>();
        sphereCollider.radius = radius;
    }

    private void UpdateWire()
    {
        for(int i = 0; i < segments.Length; i++)
        {
            if(i!= 0)
            {
                UpdateLengthOnSegment(segments[i], totalLength / segmentCount);
            }
            UpdateWeightOnSegment(segments[i], totalWt, drag, angularDrag);
        }
    }

    private void UpdateWeightOnSegment(Transform transform, float totalWt, float drag, float angularDrag)
    {
        Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
        rigidbody.mass = totalWt / segmentCount;
        rigidbody.drag = drag;
        rigidbody.angularDrag = angularDrag;
    }

    private void UpdateLengthOnSegment(Transform transform, float v)
    {
        ConfigurableJoint joint = transform.GetComponent<ConfigurableJoint>();
        if(joint != null)
        {
            joint.connectedAnchor = Vector3.forward * totalLength/segmentCount;
        }
    }

    private void RemoveSegment()
    {
        for(int i = 0; i < segments.Length; i++)
        {
            if(segments[i] != null)
            {
                Destroy(segments[i].gameObject);
            }
        }
    }


    private void OnDrawGizmos()
    {
        for(int i = 0; i < segments.Length; i++)
        {
            Gizmos.DrawWireSphere(segments[i].position, radius);
        }
        
        
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i] != null)
            {
                Gizmos.DrawSphere(vertices[i],0.05f);
            }
        } 
    }

    private void GenerateSegments()
    {
        JoinSegment(startTransform, null, true);

        Transform prevTranform = startTransform;

        Vector3 direction = (endTransform.position - startTransform.position);

        for(int i=0; i < segmentCount; i++)
        {
            GameObject segment = new GameObject($"segment_{i}");
            segment.transform.SetParent(segmentParent);
            segments[i] = segment.transform;

            Vector3 pos = prevTranform.position + (direction / segmentCount);
            segment.transform.position = pos;

            JoinSegment(segment.transform,prevTranform);

            prevTranform = segment.transform;

        }

        JoinSegment(endTransform, prevTranform, true, true);
    }

    /// <summary>
    /// Joint behaviour setup 
    /// </summary>
    /// <param name="current"></param>
    /// <param name="connectedTransform"></param>
    /// <param name="IsKinetic"></param>
    /// <param name="isCloseConnected"></param>
    void JoinSegment(Transform current, Transform connectedTransform, bool IsKinetic = false, bool isCloseConnected = false)
    {
        if(current.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rigidbody = current.gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = IsKinetic;
            rigidbody.mass = totalWt / segmentCount;
            rigidbody.drag = drag;
            rigidbody.angularDrag = angularDrag;

        }

        if (usePhysics)
        {
            SphereCollider sphereCollider = current.gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = radius;
        }

        if(connectedTransform != null)
        {
            ConfigurableJoint joint = current.GetComponent<ConfigurableJoint>();

            if(joint == null)
            {
                joint = current.gameObject.AddComponent<ConfigurableJoint>();
            }
            
            joint.connectedBody = connectedTransform.GetComponent<Rigidbody>();
            
            joint.autoConfigureConnectedAnchor = false;

            if (isCloseConnected)
            {
                joint.connectedAnchor = Vector3.forward * 0.1f;
            }
            else
            {
                joint.connectedAnchor = Vector3.forward * (totalLength / segmentCount);
            }

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            SoftJointLimit softJointLimit = new SoftJointLimit();
            softJointLimit.limit = 0;
            joint.angularZLimit = softJointLimit;

            JointDrive jointDrive = new JointDrive();
            jointDrive.positionDamper = 0;
            jointDrive.positionSpring = 0;
            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;
        }
    }
}
