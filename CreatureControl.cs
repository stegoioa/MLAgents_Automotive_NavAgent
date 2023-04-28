using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CreatureControl : Agent
{
    public GameObject Target;
    public GameObject CubeSide1;
    public GameObject CubeSide2;

    class ClonedTransform
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public ClonedTransform(Transform t)
        {
            Position = t.position;
            Rotation = t.rotation;
        }

        public void Set(Transform t)
        {
            t.position = Position;
            t.rotation = Rotation;
        }
    }

    private ClonedTransform AgentTransform;
    private ClonedTransform CubeSide1Transform;
    private ClonedTransform CubeSide2Transform;

    private float HingeRotation1;
    private float HingeRotation2;

    private float PreviousDistance;
    private float PreviousOrientation;
    public bool EnableLearning = true;

    // Start is called before the first frame update
    void Start()
    {
        AgentTransform = new ClonedTransform(transform);
        CubeSide1Transform = new ClonedTransform(CubeSide1.transform);
        CubeSide2Transform = new ClonedTransform(CubeSide2.transform);
        PreviousDistance = float.MaxValue;
        PreviousOrientation = float.MaxValue;
        HingeRotation1 = 0.0f;
        HingeRotation2 = 0.0f;
        GetComponent<DecisionRequester>().DecisionPeriod = 20;
        Debug.Log("DecisionPeriod=" + GetComponent<DecisionRequester>().DecisionPeriod);
    }

    void SubmitReward(float reward)
    {
        if (reward > 0) AddReward(reward);
        else AddReward(2 * reward);
    }

    // Update is called once per frame
    void Update()
    {
        if (EnableLearning == false) return;

        var distance = GetDistanceFromTarget(gameObject);

        if (PreviousDistance != float.MaxValue)
        {
            SubmitReward((PreviousDistance - distance) * 10);
        }

        PreviousDistance = distance;

        var orientation = GetForwardOrientationToTarget(gameObject);

        if (PreviousOrientation != float.MaxValue)
        {
            SubmitReward(PreviousOrientation - orientation);
        }

        PreviousOrientation = orientation;
    }

    void OnValidate()
    {
    }

    public float GetDistanceFromTarget(GameObject go)
    {
        return (go.transform.position - Target.transform.position).magnitude;
    }

    public Vector3 GetSideVector(GameObject go)
    {
        return Quaternion.AngleAxis(90.0f, Vector3.up) * go.transform.forward;
    }

    public float GetForwardOrientationToTarget(GameObject go)
    {
        return Vector3.Angle(
            Target.transform.position - go.transform.position,
            go.transform.forward
        );
    }

    public float GetSideOrientationToTarget(GameObject go)
    {
        return Vector3.Angle(
            Target.transform.position - go.transform.position,
            GetSideVector(go)
        );
    }

    public float GetAngleToTarget(GameObject go)
    {
        var d1 = Target.transform.position - transform.position;
        var d2 = go.transform.position - transform.position;

        return Vector3.Angle(d1, d2);
    }

    public float GetAngleToUp(GameObject go)
    {
        var d = go.transform.position - transform.position;

        return Vector3.Angle(d, new Vector3(0, 1, 0));
    }

    public float GetDistanceFromGround(GameObject go)
    {
        return go.transform.position.y - Target.transform.position.y;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(GetDistanceFromTarget(gameObject)); // 1
        sensor.AddObservation(GetForwardOrientationToTarget(gameObject)); // 2
        sensor.AddObservation(GetSideOrientationToTarget(gameObject)); // 3
        sensor.AddObservation(HingeRotation1); // 4
        sensor.AddObservation(HingeRotation2); // 5

        //sensor.AddObservation(GetDistanceFromTarget(CubeSide1)); // 1
        //sensor.AddObservation(GetDistanceFromTarget(CubeSide2));  // 2
        //sensor.AddObservation(GetDistanceFromTarget(gameObject));  // 3

        //sensor.AddObservation(GetAngleToTarget(CubeSide1));  // 4
        //sensor.AddObservation(GetAngleToTarget(CubeSide2));  // 5

        //sensor.AddObservation(GetAngleToUp(CubeSide1));  // 6
        //sensor.AddObservation(GetAngleToUp(CubeSide2));  // 7
        //sensor.AddObservation(GetAngleToUp(Target));  // 8

        //sensor.AddObservation(GetDistanceFromGround(CubeSide1));  // 9
        //sensor.AddObservation(GetDistanceFromGround(CubeSide2));  // 10
        //sensor.AddObservation(GetDistanceFromGround(gameObject));  // 11
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        var i = 0;
        HingeRotation1 = vectorAction[i++];
        HingeRotation2 = vectorAction[i++];

        //Debug.Log("Action=" + HingeRotation1 + ";" + HingeRotation2);

        var RotationFactor = 500.0f;        
        float hingeRotation1 = HingeRotation1 * RotationFactor;
        float hingeRotation2 = HingeRotation2 * RotationFactor;

        SetHingeRotation(CubeSide1, hingeRotation1);
        SetHingeRotation(CubeSide2, hingeRotation2);
    }

    private void SetHingeRotation(GameObject cube, float rotation)
    {
        var hj = cube.GetComponent<HingeJoint>();

        hj.motor = new JointMotor()
        {
            force = 1000,
            targetVelocity = rotation
        };
    }

    private void ResetRigidBody(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody>();

        rb.ResetCenterOfMass();
        rb.ResetInertiaTensor();
    }

    public override void OnEpisodeBegin()
    {
        if (EnableLearning == false) return;

        ResetRigidBody(gameObject);
        ResetRigidBody(CubeSide1);
        ResetRigidBody(CubeSide2);
        SetHingeRotation(CubeSide1, 0);
        SetHingeRotation(CubeSide2, 0);

        AgentTransform.Set(transform);
        CubeSide1Transform.Set(CubeSide1.transform);
        CubeSide2Transform.Set(CubeSide2.transform);

        var d = UnityEngine.Random.Range(25.0f, 50.0f);
        var a = UnityEngine.Random.Range(0.0f, 2 * Mathf.PI);

        Target.transform.position = transform.position + new Vector3(Mathf.Cos(a) * d, 0.0f, Mathf.Sin(a) * d);

        PreviousDistance = float.MaxValue;
        PreviousOrientation = float.MaxValue;
    }
}
