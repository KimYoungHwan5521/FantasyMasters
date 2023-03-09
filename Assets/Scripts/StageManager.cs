using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public GameObject[] SpawnPoint;
    public float spawnCool;
    public int _heroID;
    List<Stage> CurStageList;
    public string mapID = "0000";
    public int stageNumber = 1;
    public string stageInfo;
    public Text StageTime;
    public int stageTime = 60;
    
    GameObject Hero;
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
        Hero = Instantiate(Resources.Load<GameObject>($"Heros/Hero{stringID}"));

        CurStageList = DataManager.AllStageList.FindAll(x => x.mapID == "0000");

        StartCoroutine(StageStart());
    }

    IEnumerator StageStart()
    {
        stageInfo = CurStageList.Find(x => x.stageNumber == stageNumber.ToString()).stageInfo;
        for(int i=3; i>-1; i--)
        {
            FloatingText CountText = Instantiate(Resources.Load<FloatingText>("Effects/FloatingText"), new Vector2(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);
            CountText.gameObject.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0));
            if(i == 0) CountText.SetText("<size=100>Start!</size>", "#FF0000");
            else CountText.SetText($"<size=100>{i.ToString()}</size>", "#FF0000");
            yield return new WaitForSeconds(1);
        }
        stageTime = 60;
        StartCoroutine(StageTimer());
        if(Hero.GetComponent<HeroScript>().abilities.Find(x => x.Equals("0000")) != null)
        {
            StartCoroutine(SummonMinion("0000", 20.0f));
        }
        StartCoroutine(SpawnEnemy(stageInfo));
    }
    
    IEnumerator StageTimer()
    {
        StageTime.gameObject.SetActive(true);
        for(int i=1; i < stageTime + 1; i++)
        {
            StageTime.text = (stageTime - i).ToString();
            yield return new WaitForSeconds(1);
        }
        StageTime.gameObject.SetActive(false);
        StartCoroutine(StageEnd());
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

    IEnumerator StageEnd()
    {
        GameObject[] E = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject e in E)
        {
            e.GetComponent<EnemyScript>().enemyNowHP = 0;
        }
        GameObject[] M = GameObject.FindGameObjectsWithTag("Minion");
        foreach(GameObject m in M)
        {
            m.GetComponent<MinionScript>().minionNowHP = 0;
        }
        stageNumber++;
        if(CurStageList.Find(x => x.stageNumber == stageNumber.ToString()) != null) yield return StartCoroutine(ReadyToNextStage());
        else yield return StartCoroutine(ClearMap());
    }

    public GameObject Shop;
    IEnumerator ReadyToNextStage()
    {
        Shop.SetActive(true);
        yield return null;
    }

    public void OnClickStartNextStage()
    {
        StartCoroutine(StageStart());
    }

    IEnumerator ClearMap()
    {
        yield return null;
    }
    
    IEnumerator SummonMinion(string _minionID, float summonCoolTime)
    {
        var minionToSummon = Resources.Load<GameObject>($"Minions/Minion{_minionID}");
        while(true)
        {
            Vector3 summonPositon = Hero.GetComponent<Collider2D>().bounds.center;
            Instantiate(minionToSummon, summonPositon, Quaternion.identity);
            yield return new WaitForSeconds(summonCoolTime);
        }
    }
}
