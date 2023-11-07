using UnityEngine;
using UnityEngine.Serialization;

namespace Rope_Untangle.Unused
{
    public class Wire : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [FormerlySerializedAs("_meshFilter")] [SerializeField] private MeshFilter meshFilter;
    
        [SerializeField] Transform startTransform, endTransform;
        [SerializeField] int segmentCount = 10;
        [SerializeField] float totalLength = 10;

        [SerializeField] float radius = 0.5f;
        [SerializeField] int sides = 4; 

        [SerializeField] float totalWt = 10;
        [SerializeField] float drag = 1;
        [SerializeField] float angularDrag = 1;

        [SerializeField] bool usePhysics = false;

        Transform[] _segments = new Transform[0];
        [SerializeField] Transform segmentParent;
        private int _prevSegmentCount;
        private float _prevDrag;
        private float _prevTotalLength;
        private float _prevTotalWt;
        private float _prevAngularDrag;
        private float _prevRadius;

        private MeshDataRope _meshDataRope;
    
        private Vector3[] _vertices;
        private int[,] _vertexIndicesMap;
        private bool _createTriangle;
        private Mesh _mesh;

        private void Start()
        {
            _vertices = new Vector3[segmentCount * sides * 3];
            GenerateMesh();
        }

        private void Update()
        {
            if (_prevSegmentCount != segmentCount)
            {
                RemoveSegment();
                _segments = new Transform[segmentCount];
                GenerateSegments();
                GenerateMesh(); 
            }
            _prevSegmentCount = segmentCount;

        
            if (totalLength != _prevTotalLength || _prevDrag != drag || _prevTotalWt != totalWt || _prevAngularDrag != angularDrag)
            {
                UpdateWire();
                GenerateMesh();
            }

            _prevTotalLength = totalLength;
            _prevDrag = drag;
            _prevTotalWt = totalWt;
            _prevAngularDrag = angularDrag;

            if (_prevRadius != radius &&  usePhysics)
            {
                UpdateRadius();
                GenerateMesh();
            }

            _prevRadius = radius;
        }
    
        private void UpdateRadius()
        {
            for(int i = 0; i < _segments.Length; i++)
            {
                setRadiusOnSegment(_segments[i], radius);
            }
        }

        void UpdateMesh()
        {
            GenerateVertices();
        }
    
        void GenerateMesh()
        {
            _createTriangle = true;

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
            _mesh = _meshDataRope.CreateMesh();
        
            _createTriangle = false;
        
        
        }
    
        private void GenerateIndicesMap()
        {
            // +1 for extra circle of vertices after the last segment
            // Same for the sides. We need one extra after the last side
            _vertexIndicesMap = new int[segmentCount + 1, sides + 1];
            int meshVertexIndex = 0;
            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                for (int sideIndex = 0; sideIndex < sides; sideIndex++)
                {
                    _vertexIndicesMap[segmentIndex, sideIndex] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }
        private void GenerateVertices()
        {
            for (int i = 0; i < _segments.Length; i++)
            {
                GenerateCircleVerticesAndTriangles(_segments[i],i);
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
                _vertices[vertexIndex] = pointRotatedAtCenterOfTransform;

                if (_createTriangle)
                {
                    _meshDataRope.AddVertex(pointRotatedAtCenterOfTransform,new(0,0),vertexIndex);

                    bool createThisTriangle = segmentIndex < segmentCount - 1;
                    if (createThisTriangle)
                    {
                        int currentIncrement = 1;
                        int a = _vertexIndicesMap[segmentIndex, sideIndex];
                        int b = _vertexIndicesMap[segmentIndex + currentIncrement, sideIndex];
                        int c = _vertexIndicesMap[segmentIndex, sideIndex + currentIncrement];
                        int d = _vertexIndicesMap[segmentIndex + currentIncrement, sideIndex + currentIncrement];
                    
                        bool isLastGap = sideIndex == sides - 1;
                        if (isLastGap)
                        {
                            c = _vertexIndicesMap[segmentIndex, 0];
                            d = _vertexIndicesMap[segmentIndex + currentIncrement, 0];
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
            for(int i = 0; i < _segments.Length; i++)
            {
                if(i!= 0)
                {
                    UpdateLengthOnSegment(_segments[i], totalLength / segmentCount);
                }
                UpdateWeightOnSegment(_segments[i], totalWt, drag, angularDrag);
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
            for(int i = 0; i < _segments.Length; i++)
            {
                if(_segments[i] != null)
                {
                    Destroy(_segments[i].gameObject);
                }
            }
        }


        private void OnDrawGizmos()
        {
            for(int i = 0; i < _segments.Length; i++)
            {
                Gizmos.DrawWireSphere(_segments[i].position, radius);
            }
        
        
            for (int i = 0; i < _vertices.Length; i++)
            {
                if (_vertices[i] != null)
                {
                    Gizmos.DrawSphere(_vertices[i],0.05f);
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
                _segments[i] = segment.transform;

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
        /// <param name="isKinetic"></param>
        /// <param name="isCloseConnected"></param>
        void JoinSegment(Transform current, Transform connectedTransform, bool isKinetic = false, bool isCloseConnected = false)
        {
            if(current.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rigidbody = current.gameObject.AddComponent<Rigidbody>();
                rigidbody.isKinematic = isKinetic;
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
}
