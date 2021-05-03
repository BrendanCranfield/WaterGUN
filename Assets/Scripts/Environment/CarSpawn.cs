using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawn : MonoBehaviour
{

    [SerializeField] GameObject prefab;
    [SerializeField] float spawnTimer = 5f;


    private void Start()
    {
        StartSpawn();
    }

    void Spawn()
    {
        Instantiate(prefab, transform.position, Quaternion.identity);
    }

    void StartSpawn()
    {
        InvokeRepeating("Spawn", spawnTimer, Random.Range(3, 10));
    }
}
