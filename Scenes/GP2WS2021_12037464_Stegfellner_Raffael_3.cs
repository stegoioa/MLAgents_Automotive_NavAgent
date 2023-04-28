using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class GP2WS2021_12037464_Stegfellner_Raffael_3: MonoBehaviour
{
    public GameObject AgentCenter;
    public GameObject AgentWheel1;
    public GameObject AgentWheel2;
    public GameObject NavMeshAgent;
    public GameObject NavMeshTarget;


    public GameObject Obstacles;
    public GameObject ObstaclePrefab;

    public bool Randomize = false;
    public bool SavePrefab = false;

    public bool RandomizeAgentPopsition = false;
    public bool RandomizeTargetPosition = false;

    private bool DoSavePrefab = false;

    public GameObject TestObstacle;
    public GameObject ObstacleSegment1;
    public GameObject ObstacleSegment2;
    public GameObject TestPoint;
    public GameObject ClosestPointOnSegment;

    void PushPointAway(ref Vector3 p, float minDistanceSq, Vector3 minDistanceClosestPoint, out float finaldistance)
    {
        const float MinDistanceThreshold = 10.0f;

        float minDistance = Mathf.Sqrt(minDistanceSq);

        if (minDistance < MinDistanceThreshold && minDistance > 0.0f)
        {
            var d = p - minDistanceClosestPoint;

            d.Normalize();

            float randomvalue = Random.Range(1.0f, 2.0f);

            float randomizedpush = (MinDistanceThreshold - minDistance) * randomvalue;

            finaldistance = randomizedpush + minDistance;

            d *= randomizedpush;

            p += d; // d'

            
        }

        else
        {
            
            finaldistance = minDistance;
        
        }

       
    }

    public float MapValue(float a0, float a1, float b0, float b1, float a)
    {
        return b0 + (b1 - b0) * ((a - a0) / (a1 - a0));
    }

    // Start is called before the first frame update
    void Start()
    {
        //RandomizeObstacles();

        for (var i = 0; i < 10000; i++)
        {
            var p = GetRandomPositionWithMaze();

            var minDistanceSq = float.MaxValue;
            var minDistanceClosestPoint = new Vector3();

            var minDistanceSq2 = float.MaxValue;
            var minDistanceClosestPoint2 = new Vector3();


            for (var j = 0; j < Obstacles.transform.childCount; j++)
            {
                var p1 = new Vector3();
                var p2 = new Vector3();
                GetObstacleSegment(Obstacles.transform.GetChild(j).gameObject, out p1, out p2);

                var closestPoint = GetClosestPointOnLineSegment(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z), new Vector2(p.x, p.z));
                var closestPoint3 = new Vector3(closestPoint.x, 0, closestPoint.y);

                var currentDistanceSq = (p - closestPoint3).sqrMagnitude;

                if (currentDistanceSq < minDistanceSq)
                {
                    minDistanceSq2 = minDistanceSq;
                    minDistanceClosestPoint2 = minDistanceClosestPoint;

                    minDistanceSq = currentDistanceSq;
                    minDistanceClosestPoint = closestPoint3;
                }
            }

            ClosestPointOnSegment.transform.position = minDistanceClosestPoint;
            //Debug.Log("minDistance=" + minDistance);

            float finaldistancefromwall = 0.0f;
            float negligentvalue = 0.0f;

            PushPointAway(ref p, minDistanceSq, minDistanceClosestPoint, out finaldistancefromwall);
            PushPointAway(ref p, minDistanceSq2, minDistanceClosestPoint2, out negligentvalue);

            

            float lerpration = MapValue(10.0f, 30.0f, 0.0f, 1.0f, finaldistancefromwall);

            Color lerpedColor = Color.Lerp(Color.red, Color.green, lerpration);

            Debug.DrawLine(p + new Vector3(0, -10, 0), p + new Vector3(0, 10, 0), lerpedColor, 1000, false);
        }

        /*
        for (var i = 0; i < 100; i++)
        {
            var p = GetRandomPositionWithMaze();

            var minDistanceSq = float.MaxValue;
            var minDistanceClosestPoint = new Vector3();

            for (var j = 0; j < Obstacles.transform.childCount; j++)
            {
                var obstacle = Obstacles.transform.GetChild(j);

                var p1 = new Vector3();
                var p2 = new Vector3();
                GetObstacleSegment(obstacle.gameObject, out p1, out p2);

                var closestPoint = GetClosestPointOnLineSegment(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z), new Vector2(TestPoint.transform.position.x, TestPoint.transform.position.z));
                var closestPoint3 = new Vector3(closestPoint.x, 0, closestPoint.y);

                var currentDistanceSq = (p - closestPoint3).sqrMagnitude;

                if (currentDistanceSq < minDistanceSq)
                {
                    minDistanceSq = currentDistanceSq;
                    minDistanceClosestPoint = closestPoint3;
                }
            }

            var minDistance = Mathf.Sqrt(minDistanceSq);

            if (minDistance > 100.0f)
            {
                Debug.Log("minDistance=" + minDistance);
                Debug.Log("p=" + p);
                Debug.Log("minDistanceClosestPoint=" + minDistanceClosestPoint);
            }

            if (minDistance > 15.0f)
            {
                Debug.DrawLine(p + new Vector3(0, -10, 0), p + new Vector3(0, 10, 0), Color.blue, 1000, false);
            }
        }
        */
    }

    Vector2 GetClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        var ab = b - a;
        var ap = p - a;

        var l = Vector2.Dot(ap, ab);

        var t = Mathf.Clamp01(l / ab.sqrMagnitude);

        return a + ab * t;
    }

    void GetObstacleSegment(GameObject obstacle, out Vector3 end1, out Vector3 end2)
    {
        var obstacleLength = obstacle.transform.localScale.z;
        var obstacleDirection = obstacle.transform.forward;

        end1 = obstacle.transform.position + obstacleDirection * (obstacleLength / 2);
        end2 = obstacle.transform.position - obstacleDirection * (obstacleLength / 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (DoSavePrefab)
        {
            DoSavePrefab = false;

            
        }

        /*
        var p1 = new Vector3();
        var p2 = new Vector3();
        GetObstacleSegment(TestObstacle, out p1, out p2);

        ObstacleSegment1.transform.position = p1;
        ObstacleSegment2.transform.position = p2;

        var closestPoint = GetClosestPointOnLineSegment(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z), new Vector2(TestPoint.transform.position.x, TestPoint.transform.position.z));

        ClosestPointOnSegment.transform.position = new Vector3(closestPoint.x, 0, closestPoint.y);
        */

        /*
        if (DoSavePrefab)
        {
            DoSavePrefab = false;
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/obstacles.prefab");
            PrefabUtility.SaveAsPrefabAsset(Obstacles, path);
        }
        */
    }

    Vector3 GetRandomPositionWithMaze()
    {
        var p = new Vector3(Random.Range(-500.0f, 500.0f), 0, Random.Range(-500.0f, 500.0f));

        return p;
    }

    void OnValidate()
    {
        if (Randomize)
        {
            Randomize = false;
            RandomizeObstacles();
        }

        if (RandomizeAgentPopsition)
        {
            RandomizeAgentPopsition = false;
            var position = GetRandomPositionWithMaze();
            NavMeshAgent.GetComponent<NavMeshAgent>().enabled = false;
            NavMeshAgent.transform.position = position;
            NavMeshAgent.GetComponent<NavMeshAgent>().enabled = true;

            AgentCenter.transform.position += position;
            AgentWheel1.transform.position += position;
            AgentWheel2.transform.position += position;

        }

        if (RandomizeTargetPosition)
        {
            RandomizeTargetPosition = false;
            var position = GetRandomPositionWithMaze();
            NavMeshTarget.transform.position = position;
        }



        if (SavePrefab)
        {
            SavePrefab = false;
            DoSavePrefab = true;
        }
    }

    void RandomizeObstacles()
    {
        for (var i = 0; i < Obstacles.transform.childCount; i++)
        {
            Destroy(Obstacles.transform.GetChild(i).gameObject);
        }

        var center = new Vector3(0, 0, 0);

        for (var i = 0; i < 1000; i++)
        {
            var length = Random.Range(5.0f, 50.0f);

            var translateX = 0.0f;
            var translateZ = 0.0f;

            var scaleX = 1.0f;
            var scaleZ = 1.0f;

            switch (Random.Range(0, 4))
            {
                case 0:
                    translateX = length / 2.0f;
                    scaleX = length;
                    break;
                case 1:
                    translateX = -length / 2.0f;
                    scaleX = length;
                    break;
                case 2:
                    translateZ = length / 2.0f;
                    scaleZ = length;
                    break;
                case 3:
                    translateZ = -length / 2.0f;
                    scaleZ = length;
                    break;
            }

            if (Random.Range(0, 4) == 0)
            {
                center += new Vector3(translateX, 0.0f, translateZ);
                continue;
            }

            center += new Vector3(translateX, 0.0f, translateZ);

            var obstacle = Instantiate(ObstaclePrefab, Obstacles.transform);
            obstacle.transform.position = center;
            obstacle.transform.localScale = new Vector3(scaleX, 10.0f, scaleZ);

            center += new Vector3(translateX, 0.0f, translateZ);
        }
    }
}