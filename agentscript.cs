using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class agentscript : Agent
{
    public GameObject Goal;
    private Vector3 InitialPosition;
    private Quaternion InitialRotation;

    // Start is called before the first frame update


    void Start()
    {
        InitialPosition = transform.position;
        InitialRotation = transform.rotation;




       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("New Episode: " + CompletedEpisodes);

        transform.position = InitialPosition;
        transform.rotation = InitialRotation;
    }

    float GetDistanceFromGoal()
    {
        return (transform.position - Goal.transform.position).magnitude;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        var velocityForwards = vectorAction[0];
        var velocitySideways = vectorAction[1];
        var velocityRotation = vectorAction[2];

        var distanceBeforeMovement = GetDistanceFromGoal();

        transform.position += transform.forward * velocityForwards;
        var sideways = Quaternion.AngleAxis(90.0f, Vector3.up) * transform.forward;
        transform.position += sideways * velocitySideways;
        transform.rotation *= Quaternion.AngleAxis(velocityRotation, Vector3.up);

        var distanceAfterMovement = GetDistanceFromGoal();

        AddReward(distanceBeforeMovement - distanceAfterMovement);

        AnalyzeIntersections();
    }

    void AnalyzeIntersections()
    {
        var agentCollider = GetComponent<BoxCollider>();

        foreach (var go in FindObjectsOfType<BoxCollider>())
        {
            if (go != gameObject && go.bounds.Intersects(agentCollider.bounds))
            {
                if (go.gameObject.CompareTag("target"))
                {
                    AddReward(100.0f);
                }

                else if (go.gameObject.CompareTag("obstacle"))
                {
                    AddReward(-100.0f);
                }
            }
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(GetDistanceFromGoal());

        var d = Goal.transform.position - transform.position;
        var angle = Vector3.Angle(d, transform.forward);

        sensor.AddObservation(angle);
    }
}
