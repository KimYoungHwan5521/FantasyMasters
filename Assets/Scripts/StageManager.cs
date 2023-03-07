using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public GameObject[] SpawnPoint;
    public float spawnCool;
    public int _heroID;
    List<Stage> CurStageList;
    public string mapID = "0000";
    public int stageNumber = 1;
    public string stageInfo;
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

        CurStageList = DataManager.AllStageList.FindAll(x => x.mapID == "0000");
        stageInfo = CurStageList.Find(x => x.stageNumber == stageNumber.ToString()).stageInfo;

        StartCoroutine(SpawnEnemy(stageInfo));

    }
    
    IEnumerator SpawnEnemy(string _stageInfo)
    {
        List<int> spawnPointID = new List<int>();
        for(int i=0;i<8;i++) spawnPointID.Add(i);
        string[] stageSections = _stageInfo.Split('|');
        for(int i=0; i<stageSections.Length; i++)
        {
            string[] spawnMobs = stageSections[i].Split(',');
            for(int j=0; j<spawnMobs.Length; j++)
            {
                string[] spawnInfo = spawnMobs[j].Split(' ');
                for(int k=0; k<int.Parse(spawnInfo[1]); k++)
                {
                    int r = Random.Range(0, spawnPointID.Count);
                    Instantiate(Resources.Load<GameObject>($"Enemies/Enemy{spawnInfo[0]}"), SpawnPoint[spawnPointID[r]].transform.position, Quaternion.identity);
                    spawnPointID.RemoveAt(r);
                    if(spawnPointID.Count == 0) 
                    {
                        for(int x=0;x<8;x++) spawnPointID.Add(x);
                    }
                }
                spawnPointID = new List<int>();
            }
            for(int x=0;x<8;x++) spawnPointID.Add(x);
            yield return new WaitForSeconds(5);
        }
    }
}
