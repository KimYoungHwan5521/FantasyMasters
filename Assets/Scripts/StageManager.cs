using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public GameObject SpawnPoint;
    public GameObject EnemyToSpawn;
    public float spawnCool;
    public int _heroID;
    void Start()
    {
        _heroID = DataManager.selectedHeroID;
        string stringID;
        int cntDgit = 0;
        int copy_heroID = _heroID;
        for(int i=0; i<4; i++)
        {
            if(copy_heroID / 10 > 0)
            {
                cntDgit++;
                copy_heroID /= 10;
            }
            else break;
        }
        stringID = "";
        for(int i=0;i<3 - cntDgit / 10; i++)
        {
            stringID += "0";
        }
        stringID += _heroID.ToString();
        var Hero = Resources.Load<GameObject>($"Heros/Hero{stringID}");
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
