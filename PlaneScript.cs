using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class PlaneScript : MonoBehaviour
{
    public GameObject Obstacles;
    public GameObject ObstaclePrefab;

    public bool Randomize = false;
    public bool SavePrefab = false;

    private bool DoSavePrefab = false;

    // Start is called before the first frame update
    void Start()
    {
        //RandomizeObstacles();
    }

    // Update is called once per frame
    void Update()
    {
        if (DoSavePrefab)
        {
            DoSavePrefab = false;
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/obstacles.prefab");
            PrefabUtility.SaveAsPrefabAsset(Obstacles, path);
        }
    }

    void OnValidate()
    {
        if (Randomize)
        {
            Randomize = false;
            RandomizeObstacles();
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
