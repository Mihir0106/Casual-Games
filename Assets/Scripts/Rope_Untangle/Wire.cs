using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [SerializeField] Transform startTransform, endTransform;
    [SerializeField] int segmentCount = 10;
    [SerializeField] float totalLength = 10;

    [SerializeField] float radius = 0.5f;

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

    private void Start()
    {
    }

    private void Update()
    {
        if (prevSegmentCount != segmentCount)
        {
            RemoveSegment();
            segments = new Transform[segmentCount];
            GenerateSegments(); 
        }
        prevSegmentCount = segmentCount;

        
        if (totalLength != prevTotalLength || prevDrag != drag || prevTotalWt != totalWt || prevAngularDrag != angularDrag)
        {
            UpdateWire();
        }

        prevTotalLength = totalLength;
        prevDrag = drag;
        prevTotalWt = totalWt;
        prevAngularDrag = angularDrag;

        if (prevRadius != radius &&  usePhysics)
        {
            UpdateRadius();
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
