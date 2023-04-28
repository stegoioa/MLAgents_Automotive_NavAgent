using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentScript : MonoBehaviour
{
    public GameObject NavMeshAgentTarget;
    public GameObject MLAgent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent.destination != NavMeshAgentTarget.transform.position)
        {
            navMeshAgent.destination = NavMeshAgentTarget.transform.position;
        }

        var distanceToMLAgent = (MLAgent.transform.position - transform.position).magnitude;

        if (distanceToMLAgent < 15.0f) navMeshAgent.enabled = true;
        else navMeshAgent.enabled = false;
    }
}
