using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetScript : MonoBehaviour
{
    public GameObject Target;
    public GameObject MLAgent;

    private NavMeshAgent Agent;

    // Start is called before the first frame update
    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Agent.enabled = (transform.position - MLAgent.transform.position).magnitude < 15.0f;

        if (Agent.enabled && Agent.destination != Target.transform.position)
        {
            Agent.destination = Target.transform.position;
            Debug.Log("Destingation updated");
        }
    }
}
