using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public GameObject SpawnPoint;
    public GameObject EnemyToSpawn;
    public float spawnCool;
    void Start()
    {
        var Hero = Resources.Load<GameObject>("Heros/Hero");
        Instantiate(Hero);

        if(spawnCool > 0)
        {
            InvokeRepeating("SpawnEnemy", 0, spawnCool);
        }
    }

    
    public GameObject SpawnEnemy()
    {
        if(SpawnPoint != null)
        {
            return Instantiate(EnemyToSpawn, SpawnPoint.transform.position, Quaternion.identity);
        }
        return null;
    }
}
