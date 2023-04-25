using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class StageManager : MonoBehaviour
{
    NoticeUI _notice;
    SoundManager SoundManager;
    public GameObject[] SpawnPoint;
    public float spawnCool;
    public int _heroID;
    List<Stage> CurStageList;
    public string mapID = "0000";
    public int stageNumber = 1;
    public string stageInfo;
    public Text StageTime;
    public int stageTime = 60;
    public class Product
    {
        public string productType;
        public string inheritanceID;
        public string productName;
        public string[] attributes;
        public string rareDegree;
        public string explain;
    }
    public List<Product> AllProductList;
    
    GameObject Hero;
    void Awake()
    {
        _notice = FindObjectOfType<NoticeUI>();
        SoundManager = GameObject.Find("DataManager").GetComponent<SoundManager>();
    }

    public static int mapMinionKillEnemies = 0;
    public static int mapMinionDeath = 0;
    public static int mapEnemyDeath = 0;
    public static int stageMinionDeath = 0;
    public static int stageEnemyDeath = 0;

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
        AllProductList = new List<Product>();
        for(int i=0; i<DataManager.AllAbilityList.Count; i++)
        {
            if(DataManager.AllAbilityList[i].abilityRareDegree != "")
            {
                Product pd = new Product();
                pd.productType = "능력";
                pd.inheritanceID = DataManager.AllAbilityList[i].abilityID;
                pd.productName = DataManager.AllAbilityList[i].abilityNameKR;
                pd.attributes = DataManager.AllAbilityList[i].abilityAttributes;
                pd.rareDegree = DataManager.AllAbilityList[i].abilityRareDegree;
                pd.explain = DataManager.AllAbilityList[i].abilityExplainKR;
                AllProductList.Add(pd);
            }
        }
        for(int i=0; i<DataManager.AllItemList.Count; i++)
        {
            if(DataManager.AllItemList[i].itemRareDegree != null)
            {
                Product pd = new Product();
                pd.productType = "아이템";
                pd.inheritanceID = DataManager.AllItemList[i].itemID;
                pd.productName = DataManager.AllItemList[i].itemNameKR;
                pd.rareDegree = DataManager.AllItemList[i].itemRareDegree;
                pd.attributes = DataManager.AllItemList[i].itemAttributes;
                pd.explain = DataManager.AllItemList[i].itemExplainKR;
                AllProductList.Add(pd);
            }
        }
        StartCoroutine(StageStart());
    }

    bool deathEvent = true;
    void Update()
    {
        if(Hero.GetComponent<HeroScript>().nowHP <= 0 && deathEvent)
        {
            deathEvent = false;
            StartCoroutine(DeathEvent());
        }
    }

    IEnumerator DeathEvent()
    {
        if(Hero.GetComponent<HeroScript>().resurrection > 0)
        {
            Hero.GetComponent<HeroScript>().resurrection--;
            Instantiate(Resources.Load<GameObject>("Effects/Resurrection"), Hero.transform.position, Quaternion.identity);
            SoundManager.PlaySE(Resources.Load<AudioClip>("Sounds/SE/RPG_Essentials_Free/8_Buffs_Heals_SFX/30_Revive_03"));
            Hero.layer = 12;
            Hero.GetComponent<HeroScript>().controllable = false;
            Hero.GetComponent<HeroScript>().attackable = false;
            yield return new WaitForSeconds(3);
            Hero.GetComponent<HeroScript>().nowHP = Hero.GetComponent<HeroScript>().maxHP;
            Hero.GetComponent<HeroScript>().controllable = true;
            Hero.GetComponent<HeroScript>().attackable = true;
            yield return new WaitForSeconds(3);
            Hero.layer = 11;
            deathEvent = true;
        }
        else
        {
            // GameOver
        }
    }

    IEnumerator StageStart()
    {
        stageInfo = CurStageList.Find(x => x.stageNumber == stageNumber.ToString()).stageInfo;
        stageEnemyDeath = 0;
        stageMinionDeath = 0;
        Hero.GetComponent<HeroScript>().nowHP = Hero.GetComponent<HeroScript>().maxHP - Hero.GetComponent<HeroScript>().tempMaxHPCV;
        Hero.GetComponent<HeroScript>().tempMaxHPCV = 0;
        Hero.transform.position = new Vector2(0, 0);
        FloatingText CountText;
        for(int i=3; i>-1; i--)
        {
            CountText = Instantiate(Resources.Load<FloatingText>("Effects/FloatingText"), new Vector2(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);
            CountText.gameObject.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0));
            CountText.SetText($"<size=100>{i.ToString()}</size>", "#FF0000");
            yield return new WaitForSeconds(1);
            if(i==0) CountText.SetText("<size=100>Start!</size>", "#FF0000");
        }
        List<string> hAbilities = Hero.GetComponent<HeroScript>().abilities;
        if(hAbilities.Contains("0019")) SoundManager.PlaySE(Resources.Load<AudioClip>("Sounds/SE/FreeSFX/GameSFX/Animal Insects/Retro Wolf B 02"));
        stageTime = stageInfo.Split('|').Length * 5;
        // stageTime = 60;
        StartCoroutine(StageTimer());
        if(hAbilities.Contains("0000"))
        {
            StartCoroutine(SummonMinion("0000", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0000").abilityCoolTime)));
        }
        if(hAbilities.Contains("0002"))
        {
            StartCoroutine(SummonMinion("0001", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0002").abilityCoolTime)));
        }
        if(hAbilities.Contains("0008"))
        {
            StartCoroutine(LifeDrain(float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0008").abilityCoolTime)));
        }
        if(hAbilities.Contains("0012"))
        {
            StartCoroutine(Polymorph("0010", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0012").abilityCoolTime)));
        }
        if(hAbilities.Contains("0014"))
        {
            StartCoroutine(Fear(float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0014").abilityCoolTime)));
        }
        if(hAbilities.Contains("0018"))
        {
            StartCoroutine(SummonProjectile("SkullThrowing",float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0018").abilityCoolTime), 1000));
        }
        if(hAbilities.Contains("0021"))
        {
            StartCoroutine(SummonMinion("0002", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0021").abilityCoolTime), 3));
        }
        if(hAbilities.Contains("0022"))
        {
            StartCoroutine(BloodTransfusion(float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0022").abilityCoolTime)));
        }
        if(hAbilities.Contains("0023"))
        {
            Hero.GetComponent<HeroScript>().AddStatus("0006");
        }
        if(hAbilities.Contains("0029"))
        {
            StartCoroutine(SummonMinion("0004", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0029").abilityCoolTime)));
        }
        if(hAbilities.Contains("0034"))
        {
            StartCoroutine(SummonMinion("0005", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0034").abilityCoolTime), 2));
        }
        if(hAbilities.Contains("0035"))
        {
            StartCoroutine(SummonMinion("0006", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0035").abilityCoolTime), 2));
        }
        if(hAbilities.Contains("0036"))
        {
            StartCoroutine(SummonTrap(Hero, "0000", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0036").abilityCoolTime)));
        }
        if(hAbilities.Contains("0037"))
        {
            StartCoroutine(SummonTrap(Hero, "0001", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0037").abilityCoolTime)));
        }
        if(hAbilities.Contains("0038"))
        {
            StartCoroutine(SummonMinion("0007", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0038").abilityCoolTime)));
        }
        if(hAbilities.Contains("0039"))
        {
            StartCoroutine(SummonMinion("0008", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0039").abilityCoolTime)));
            StartCoroutine(SummonMinion("0009", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0039").abilityCoolTime)));
            StartCoroutine(SummonMinion("0010", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0039").abilityCoolTime)));
            StartCoroutine(SummonMinion("0011", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0039").abilityCoolTime)));
        }
        if(hAbilities.Contains("0041"))
        {
            StartCoroutine(SummonOrbiting("LeafIncantation", 5, 2, 40, true));
        }
        if(hAbilities.Contains("0043"))
        {
            StartCoroutine(SummonProjectile("LeafBlade",float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0043").abilityCoolTime), 40, true));
        }
        if(hAbilities.Contains("0045"))
        {
            StartCoroutine(Heal(float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0045").abilityCoolTime), Hero.GetComponent<HeroScript>().armor * 10));
        }
        if(hAbilities.Contains("0047"))
        {
            StartCoroutine(SummonMinion("0013", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0047").abilityCoolTime), 3));
        }
        if(hAbilities.Contains("0048"))
        {
            StartCoroutine(SummonTrap(Hero, "0002", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0048").abilityCoolTime)));
        }
        if(hAbilities.Contains("0050"))
        {
            StartCoroutine(SummonMinion("0014", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0050").abilityCoolTime), 1, 30, false));
        }
        if(hAbilities.Contains("0052"))
        {
            StartCoroutine(SummonMinion("0015", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0052").abilityCoolTime)));
        }
        if(hAbilities.Contains("0053"))
        {
            StartCoroutine(SummonMinion("0016", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0053").abilityCoolTime)));
        }
        if(hAbilities.Contains("0054"))
        {
            StartCoroutine(SummonMinion("0017", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0054").abilityCoolTime)));
        }
        if(hAbilities.Contains("0055"))
        {
            StartCoroutine(Buffs("0013", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0055").abilityCoolTime)));
        }
        if(hAbilities.Contains("0056"))
        {
            StartCoroutine(Buffs("0014", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0056").abilityCoolTime)));
        }
        if(hAbilities.Contains("0057"))
        {
            StartCoroutine(Buffs("0015", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0057").abilityCoolTime)));
        }
        if(hAbilities.Contains("0058"))
        {
            StartCoroutine(SummonMinion("0018", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0058").abilityCoolTime)));
        }
        if(hAbilities.Contains("0059"))
        {
            StartCoroutine(SummonProjectile("Baseball",float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0059").abilityCoolTime), 400));
        }
        if(hAbilities.Contains("0060"))
        {
            StartCoroutine(SummonOrbiting("BowlingBall", 1, 2.4f, 600));
        }
        if(hAbilities.Contains("0061"))
        {
            StartCoroutine(Buffs("0017", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0061").abilityCoolTime), 0, "hero"));
        }
        if(hAbilities.Contains("0062"))
        {
            StartCoroutine(Buffs("0018", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0062").abilityCoolTime), 30, "hero"));
        }
        if(hAbilities.Contains("0063"))
        {
            StartCoroutine(AreaDamage("MissileBombing", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0063").abilityCoolTime), 2, 600));
        }
        if(hAbilities.Contains("0064"))
        {
            StartCoroutine(SummonTrap(Hero, "0003", float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0064").abilityCoolTime)));
        }
        if(hAbilities.Contains("0065"))
        {
            StartCoroutine(Strike(float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0065").abilityCoolTime)));
        }
        if(hAbilities.Contains("0066"))
        {
            StartCoroutine(AreaHeal(Hero, float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0066").abilityCoolTime), 4, 200));
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
            }
            spawnPointID = new List<int>();
            for(int x=0;x<8;x++) spawnPointID.Add(x);
            yield return new WaitForSeconds(5);
        }
    }

    IEnumerator StageEnd()
    {
        int exception = 0;
        while(true)
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
            GameObject[] T = GameObject.FindGameObjectsWithTag("Trap");
            foreach(GameObject t in T)
            {
                t.GetComponent<Animator>().SetBool("Activate", true);
            }
            GameObject[] O = GameObject.FindGameObjectsWithTag("Orbiting");
            foreach(GameObject o in O)
            {
                Destroy(o.transform.parent.gameObject);
            }
            exception++;
            if(exception > 100) break;
        }
        for(int i=0; i<Hero.GetComponent<HeroScript>().HeroStatus.Count; i++)
        {
            Hero.GetComponent<HeroScript>().RemoveStatus(Hero.GetComponent<HeroScript>().HeroStatus[i].statusID);
            Hero.GetComponent<HeroScript>().HeroStatus.RemoveAt(i);
            Destroy(Hero.GetComponent<HeroScript>().StatusSprites.transform.GetChild(i).gameObject);
        }
        stageNumber++;
        if(CurStageList.Find(x => x.stageNumber == stageNumber.ToString()) != null) yield return StartCoroutine(ReadyToNextStage());
        else yield return StartCoroutine(ClearMap());
    }

    public GameObject Shop;
    public GameObject[] ProductsSimple;
    public List<Product> CurProductList;
    public GameObject[] HaveAbilities;
    public GameObject[] HaveItems;
    public GameObject HeroStats;
    IEnumerator ReadyToNextStage()
    {
        CurProductList = new List<Product>();
        selectedProduct = -1;
        string[] hAttributes = Hero.GetComponent<HeroScript>().attributes;
        int exception = 0;
        int ip=0;
        while(CurProductList.Count < 5)
        {
            if(exception > 100) break;
            int rd = 0;
            int random = Random.Range(0, 1000);
            if(random < 700) rd = 0;
            else if(random < 910) rd = 1;
            else if(random < 973) rd = 2;
            else rd = 3;
            List<Product> tempPdl = new List<Product>();
            if(ip == 0)
            {
                // 상점 첫 번째 칸은 영웅 속성 카드 등장 보장(순속성은 두 번째 칸까지)
                tempPdl = AllProductList.FindAll(x => x.attributes.ToList().Contains(hAttributes[0]));
            }
            else if(ip == 1)
            {
                if(hAttributes.Length > 1)
                {
                    // 듀얼 속성일 경우 두 번째 칸은 영웅의 두 번째 속성 카드 등장 보장
                    tempPdl = AllProductList.FindAll(x => x.attributes.ToList().Contains(hAttributes[1]));
                }
                else
                {
                    tempPdl = AllProductList.FindAll(x => x.attributes.ToList().Contains(hAttributes[0]));
                }
            }
            else if(ip == 2)
            {
                // 세 번째 칸은 자신 속성 또는 무속성 카드 등장 가능(듀얼 속성의 경우 자신의 속성 둘중 무작위로 등장)
                if(hAttributes.Length > 1)
                {
                    tempPdl = AllProductList.FindAll(x => x.attributes.ToList().Contains(hAttributes[0]) || x.attributes.ToList().Contains(hAttributes[1]) || x.attributes.ToList().Contains("무속성"));
                }
                else
                {
                    tempPdl = AllProductList.FindAll(x => x.attributes.ToList().Contains(hAttributes[0]) || x.attributes.ToList().Contains("무속성"));
                }
            }
            else if(ip == 3)
            {
                // 네 번째 칸은 모든 속성 등장 가능
                tempPdl = AllProductList.ToList();
            }
            else
            {
                // 다섯 번째 칸은 자신의 속성 제외한 카드만 등장(무속성 포함)
                if(hAttributes.Length > 1)
                {
                    tempPdl = AllProductList.FindAll(x => !(x.attributes.ToList().Contains(hAttributes[0])) && !(x.attributes.ToList().Contains(hAttributes[1])));    
                }
                else
                {
                    tempPdl = AllProductList.FindAll(x => !(x.attributes.ToList().Contains(hAttributes[0])));
                }
            }
            tempPdl = tempPdl.FindAll(x => x.rareDegree == rd.ToString());
            if(tempPdl.Count > 0)
            {
                int r = Random.Range(0, tempPdl.Count);
                bool condition = true;
                print($"i, tempPdl[r].productName : {ip}, {tempPdl[r].productName}");
                if(tempPdl[r].productType == "능력") condition = !Hero.GetComponent<HeroScript>().abilities.Contains(tempPdl[r].inheritanceID);
                else if(condition)
                {
                    for(int j=0; j<Hero.GetComponent<HeroScript>().HeroItems.Count; j++)
                    {
                        if(Hero.GetComponent<HeroScript>().HeroItems[j].itemID == tempPdl[r].inheritanceID)
                        {
                            condition = false;
                            break;
                        }
                    }
                }
                if(!CurProductList.Contains(tempPdl[r]) && condition) CurProductList.Add(tempPdl[r]);
                else ip--;
            }
            ip++;
            exception++;
        }
        for(int i=0; i<CurProductList.Count; i++)
        {
            Image ProductImage = ProductsSimple[i].GetComponentsInChildren<Image>()[2];
            if(CurProductList[i].productType == "아이템") ProductImage.sprite = Resources.Load<Sprite>($"UIs/Icons/Products/Items/Item{CurProductList[i].inheritanceID}");
            else ProductImage.sprite = Resources.Load<Sprite>($"UIs/Icons/Products/Abilities/Ability{CurProductList[i].inheritanceID}");
            Text[] ProductSimpleTexts = ProductsSimple[i].GetComponentsInChildren<Text>();
            ProductSimpleTexts[0].text = CurProductList[i].productName;
            ProductSimpleTexts[1].text = "";
            for(int j=0; j<CurProductList[i].attributes.Length; j++)
            {
                if(j>0) ProductSimpleTexts[1].text += " ";
                if(CurProductList[i].attributes[j] == "암흑") ProductSimpleTexts[1].text += "<color=purple>암흑</color>";
                else if(CurProductList[i].attributes[j] == "불") ProductSimpleTexts[1].text += "<color=red>불</color>";
                else if(CurProductList[i].attributes[j] == "물") ProductSimpleTexts[1].text += "<color=blue>물</color>";
                else if(CurProductList[i].attributes[j] == "숲") ProductSimpleTexts[1].text += "<color=green>숲</color>";
                else if(CurProductList[i].attributes[j] == "금속") ProductSimpleTexts[1].text += "<color=silver>금속</color>";
                else if(CurProductList[i].attributes[j] == "대지") ProductSimpleTexts[1].text += "<color=olive>대지</color>";
                else if(CurProductList[i].attributes[j] == "빛") ProductSimpleTexts[1].text += "<color=yellow>빛</color>";
                else ProductSimpleTexts[1].text += CurProductList[i].attributes[j];
            }
            ProductSimpleTexts[2].text = CurProductList[i].productType;
            if(CurProductList[i].rareDegree == "0") ProductSimpleTexts[3].text = "일반";
            else if(CurProductList[i].rareDegree == "1") ProductSimpleTexts[3].text = "<color=blue>희귀</color>";
            else if(CurProductList[i].rareDegree == "2") ProductSimpleTexts[3].text = "<color=purple>신화</color>";
            else ProductSimpleTexts[3].text = "<color=orange>전설</color>";
            ProductSimpleTexts[4].text = CurProductList[i].explain;
        }
        List<string> hAbilities = Hero.GetComponent<HeroScript>().abilities;
        for(int i=0; i<hAbilities.Count; i++)
        {
            HaveAbilities[i].SetActive(i < hAbilities.Count);
            Image HaveAbilityImage = HaveAbilities[i].GetComponentsInChildren<Image>()[1];
            HaveAbilityImage.sprite = Resources.Load<Sprite>($"UIs/Icons/Products/Abilities/Ability{hAbilities[i]}");
        }
        List<Item> hItems = Hero.GetComponent<HeroScript>().HeroItems;
        for(int i=0; i<hItems.Count; i++)
        {
            HaveItems[i].SetActive(i<hItems.Count);
            Image HaveItemImage = HaveItems[i].GetComponentsInChildren<Image>()[1];
            HaveItemImage.sprite = Resources.Load<Sprite>($"UIs/Icons/Products/Items/Item{hItems[i].itemID}");
        }
        Text[] heroStatsText = HeroStats.GetComponentsInChildren<Text>();
        heroStatsText[2].text = $"{(Hero.GetComponent<HeroScript>().maxHP - Hero.GetComponent<HeroScript>().tempMaxHPCV)}";
        heroStatsText[4].text = $"{Hero.GetComponent<HeroScript>().HPRegeneration}";
        if(DataManager.AllHeroList[Hero.GetComponent<HeroScript>()._heroID].heroAtkDmg.Contains("x")) heroStatsText[6].text = $"{Hero.GetComponent<HeroScript>().atkDmg}x{DataManager.AllHeroList[Hero.GetComponent<HeroScript>()._heroID].heroAtkDmg.Split('x')[1]}";
        else heroStatsText[6].text = $"{Hero.GetComponent<HeroScript>().atkDmg}";
        heroStatsText[8].text = $"{Hero.GetComponent<HeroScript>().atkSpeed}";
        heroStatsText[10].text = $"{Hero.GetComponent<HeroScript>().atkRange}";
        heroStatsText[12].text = $"{Hero.GetComponent<HeroScript>().criticalDmg * 100}%";
        heroStatsText[14].text = $"{Hero.GetComponent<HeroScript>().criticalChance}%";
        heroStatsText[16].text = $"{Hero.GetComponent<HeroScript>().armor}";
        heroStatsText[18].text = $"{Hero.GetComponent<HeroScript>().moveSpeed}";
        Shop.SetActive(true);
        yield return null;
    }

    public GameObject Explain;
    public void MouseEnterHaveAbility(int ID)
    {
        Text ExplainText = Explain.GetComponentInChildren<Text>();
        ExplainText.text = "";
        ExplainText.text += $"<b>{DataManager.AllAbilityList.Find(x => x.abilityID == Hero.GetComponent<HeroScript>().abilities[ID]).abilityNameKR}</b>\n";
        ExplainText.text += DataManager.AllAbilityList.Find(x => x.abilityID == Hero.GetComponent<HeroScript>().abilities[ID]).abilityExplainKR;
        Explain.GetComponent<RectTransform>().anchoredPosition = new Vector2(HaveAbilities[ID].GetComponent<RectTransform>().anchoredPosition.x + HaveAbilities[ID].GetComponent<RectTransform>().rect.width, HaveAbilities[ID].GetComponent<RectTransform>().transform.position.y + Explain.GetComponent<RectTransform>().rect.height);
        Explain.SetActive(true);
    }

    public void MouseEnterHaveItems(int ID)
    {
        Text ExplainText = Explain.GetComponentInChildren<Text>();
        ExplainText.text = "";
        ExplainText.text += $"<b>{DataManager.AllItemList.Find(x => x.itemID == Hero.GetComponent<HeroScript>().HeroItems[ID].itemID).itemNameKR}</b>\n";
        ExplainText.text += DataManager.AllItemList.Find(x => x.itemID == Hero.GetComponent<HeroScript>().HeroItems[ID].itemID).itemExplainKR;
        Explain.GetComponent<RectTransform>().anchoredPosition = new Vector2(HaveItems[ID].GetComponent<RectTransform>().anchoredPosition.x + HaveItems[ID].GetComponent<RectTransform>().rect.width, HaveItems[ID].GetComponent<RectTransform>().transform.position.y + Explain.GetComponent<RectTransform>().rect.height);
        Explain.SetActive(true);
    }

    public void MouseExitHaveAbility()
    {
        Explain.SetActive(false);
    }

    public GameObject ProductDetail;
    public GameObject MinionInfo;
    public GameObject AbilityInfo;
    public GameObject StatusInfo;
    public void MouseEnterProductSimple(int num)
    {
        if(CurProductList[num].productType == "능력") 
        {
            if(DataManager.AllAbilityList.Find(x => x.abilityID == CurProductList[num].inheritanceID).relatedMinion[0] != "")
            {
                string tempMID = DataManager.AllAbilityList.Find(x => x.abilityID == CurProductList[num].inheritanceID).relatedMinion[0];
                Text[] minionInfoTexts = MinionInfo.GetComponentsInChildren<Text>();
                minionInfoTexts[0].text = DataManager.AllMinionList.Find(x => x.minionID == tempMID).minionNameKR;
                minionInfoTexts[2].text = DataManager.AllMinionList.Find(x => x.minionID == tempMID).minionMaxHP;
                minionInfoTexts[4].text = DataManager.AllMinionList.Find(x => x.minionID == tempMID).minionAtkDmg;
                minionInfoTexts[6].text = DataManager.AllMinionList.Find(x => x.minionID == tempMID).minionAtkSpeed;
                minionInfoTexts[8].text = Mathf.Round(float.Parse(DataManager.AllMinionList.Find(x => x.minionID == tempMID).minionCriticalDmg) * 100).ToString() + "%";
                minionInfoTexts[10].text = DataManager.AllMinionList.Find(x => x.minionID == tempMID).minionCriticalChance + "%";
                minionInfoTexts[12].text = DataManager.AllMinionList.Find(x => x.minionID == tempMID).minionArmor;
                minionInfoTexts[14].text = DataManager.AllMinionList.Find(x => x.minionID == tempMID).minionMoveSpeed;
                MinionInfo.SetActive(true);
                if(DataManager.AllMinionList.Find(x => x.minionID == tempMID).minionAbilities[0] != "")
                {
                    string tempAID = DataManager.AllMinionList.Find(x => x.minionID == tempMID).minionAbilities[0];
                    minionInfoTexts[16].text = DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).abilityNameKR;
                    Text[] abilityInfoTexts = AbilityInfo.GetComponentsInChildren<Text>();
                    abilityInfoTexts[0].text = DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).abilityNameKR;
                    abilityInfoTexts[1].text = DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).abilityExplainKR;
                    AbilityInfo.SetActive(true);
                    if(DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).relatedStatus[0] != "")
                    {
                        string tempSID = DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).relatedStatus[0];
                        Text[] statusInfoTexts = StatusInfo.GetComponentsInChildren<Text>();
                        statusInfoTexts[0].text = DataManager.AllStatusList.Find(x => x.statusID == tempSID).statusNameKR;
                        statusInfoTexts[1].text = DataManager.AllStatusList.Find(x => x.statusID == tempSID).statusExplainKR;
                        StatusInfo.SetActive(true);
                    }
                    else if(DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).relatedTrap[0] != "")
                    {
                        string tempSID = DataManager.AllTrapList.Find(x => x.trapID == DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).relatedTrap[0]).trapStatus[0];
                        Text[] statusInfoTexts = StatusInfo.GetComponentsInChildren<Text>();
                        statusInfoTexts[0].text = DataManager.AllStatusList.Find(x => x.statusID == tempSID).statusNameKR;
                        statusInfoTexts[1].text = DataManager.AllStatusList.Find(x => x.statusID == tempSID).statusExplainKR;
                        StatusInfo.SetActive(true);
                    }
                    else StatusInfo.SetActive(false);
                }
                else 
                {
                    minionInfoTexts[16].text = "없음";
                    AbilityInfo.SetActive(false);
                    StatusInfo.SetActive(false);
                }
            }
            else if(DataManager.AllAbilityList.Find(x => x.abilityID == CurProductList[num].inheritanceID).relatedTrap[0] != "")
            {
                MinionInfo.SetActive(false);
                AbilityInfo.SetActive(false);
                if(DataManager.AllTrapList.Find(x => x.trapID == DataManager.AllAbilityList.Find(x => x.abilityID == CurProductList[num].inheritanceID).relatedTrap[0]).trapStatus[0] != null)
                {
                    string tempSID = DataManager.AllTrapList.Find(x => x.trapID == DataManager.AllAbilityList.Find(x => x.abilityID == CurProductList[num].inheritanceID).relatedTrap[0]).trapStatus[0];
                    Text[] statusInfoTexts = StatusInfo.GetComponentsInChildren<Text>();
                    statusInfoTexts[0].text = DataManager.AllStatusList.Find(x => x.statusID == tempSID).statusNameKR;
                    statusInfoTexts[1].text = DataManager.AllStatusList.Find(x => x.statusID == tempSID).statusExplainKR;
                    StatusInfo.SetActive(true);
                }
                else StatusInfo.SetActive(false);
            }
            else
            {
                MinionInfo.SetActive(false);
                AbilityInfo.SetActive(false);
                if(DataManager.AllAbilityList.Find(x => x.abilityID == CurProductList[num].inheritanceID).relatedStatus[0] != "")
                {
                    string tempSID = DataManager.AllAbilityList.Find(x => x.abilityID == CurProductList[num].inheritanceID).relatedStatus[0];
                    Text[] statusInfoTexts = StatusInfo.GetComponentsInChildren<Text>();
                    statusInfoTexts[0].text = DataManager.AllStatusList.Find(x => x.statusID == tempSID).statusNameKR;
                    statusInfoTexts[1].text = DataManager.AllStatusList.Find(x => x.statusID == tempSID).statusExplainKR;
                    StatusInfo.SetActive(true);
                }
                else StatusInfo.SetActive(false);
            }

        }
        else 
        {
            MinionInfo.SetActive(false);
            if(DataManager.AllItemList.Find(x => x.itemID == CurProductList[num].inheritanceID).itemAbilities[0] != "")
            {
                string tempAID = DataManager.AllItemList.Find(x => x.itemID == CurProductList[num].inheritanceID).itemAbilities[0];
                Text[] abilityInfoTexts = AbilityInfo.GetComponentsInChildren<Text>();
                abilityInfoTexts[0].text = DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).abilityNameKR;
                abilityInfoTexts[1].text = DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).abilityExplainKR;
                AbilityInfo.SetActive(true);
                if(DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).relatedStatus[0] != "")
                {
                    string tempSID = DataManager.AllAbilityList.Find(x => x.abilityID == tempAID).relatedStatus[0];
                    Text[] statusInfoTexts = StatusInfo.GetComponentsInChildren<Text>();
                    statusInfoTexts[0].text = DataManager.AllStatusList.Find(x => x.statusID == tempSID).statusNameKR;
                    statusInfoTexts[1].text = DataManager.AllStatusList.Find(x => x.statusID == tempSID).statusExplainKR;
                    StatusInfo.SetActive(true);
                }
                else StatusInfo.SetActive(false);
            }
            else
            {
                AbilityInfo.SetActive(false);
                StatusInfo.SetActive(false);
            }
        }
        if(num < 4) ProductDetail.GetComponent<RectTransform>().anchoredPosition = new Vector2(ProductsSimple[num].GetComponent<RectTransform>().anchoredPosition.x + ProductsSimple[num].GetComponent<RectTransform>().rect.width + 75, ProductsSimple[num].GetComponent<RectTransform>().anchoredPosition.y - 30);
        else ProductDetail.GetComponent<RectTransform>().anchoredPosition = new Vector2(ProductsSimple[num].GetComponent<RectTransform>().anchoredPosition.x - ProductsSimple[num].GetComponent<RectTransform>().rect.width + 75, ProductsSimple[num].GetComponent<RectTransform>().anchoredPosition.y - 30);
        if(MinionInfo.activeSelf || AbilityInfo.activeSelf || StatusInfo.activeSelf) ProductDetail.SetActive(true);
    }

    public void MouseExitProductSimple()
    {
        ProductDetail.SetActive(false);
    }

    public int selectedProduct;
    public void SelectProduct(int _pdnum)
    {
        selectedProduct = _pdnum;
        for(int i=0; i < 5; i++)
        {
            if(i == _pdnum)
            {
                ProductsSimple[i].GetComponent<Outline>().effectColor = new Color(0, 1, 0, 1);
                ProductsSimple[i].GetComponent<Outline>().effectDistance = new Vector2(3, -3);
            }
            else
            {
                ProductsSimple[i].GetComponent<Outline>().effectColor = new Color(0, 0, 0, 1);
                ProductsSimple[i].GetComponent<Outline>().effectDistance = new Vector2(1, -1);
            }
        }
    }

    public void OnClickStartNextStage()
    {
        if(selectedProduct != -1)
        {
            if(CurProductList[selectedProduct].productType == "능력")
            {
                Hero.GetComponent<HeroScript>().abilities.Add(CurProductList[selectedProduct].inheritanceID);
                if(CurProductList[selectedProduct].inheritanceID == "0013") Hero.GetComponent<HeroScript>().resurrection++;
            }
            else
            {
                Hero.GetComponent<HeroScript>().AddItem(CurProductList[selectedProduct].inheritanceID);
            }
            Shop.SetActive(false);
            StartCoroutine(StageStart());
        }
        else
        {
            _notice.SUB("보상을 선택 해주세요");
        }
    }

    IEnumerator ClearMap()
    {
        yield return null;
    }
    
    // ↓↓↓ Abilities ↓↓↓
    IEnumerator SummonMinion(string _minionID, float summonCoolTime, int n = 1, float wait = 0, bool repeat = true)
    {
        yield return new WaitForSeconds(wait);
        var minionToSummon = Resources.Load<GameObject>($"Minions/Minion{_minionID}");
        while(true)
        {
            if(StageTime.text == "0") break;
            Vector3 summonPositon = Hero.GetComponent<Collider2D>().bounds.center;
            Instantiate(minionToSummon, summonPositon, Quaternion.identity);
            if(!repeat) break;
            yield return new WaitForSeconds(summonCoolTime);
        }
    }

    IEnumerator LifeDrain(float coolTime)
    {
        var effect = Resources.Load<GameObject>($"Projectiles/ProjectileLifeDrain");
        while(true)
        {   
            if(StageTime.text == "0") break;
            GameObject te = GameObject.FindWithTag("Enemy");
            if(te != null)
            {
                te.GetComponent<EnemyScript>().enemyNowHP -= 80;
                GameObject Pjt = Instantiate(effect, te.transform.position, Quaternion.identity);
                Pjt.GetComponentInChildren<ProjectileScript>().SetProjectile(te, Hero, false, "LifeDrain");
                yield return new WaitForSeconds(coolTime);
            }
            else yield return new WaitForSeconds(1);
        }
    }

    IEnumerator Polymorph(string polymorphTo, float coolTime)
    {
        while(true)
        {
            if(StageTime.text == "0") break;
            GameObject te = GameObject.FindWithTag("Enemy");
            if(te != null)
            {
                Instantiate(Resources.Load<GameObject>($"Enemies/Enemy{polymorphTo}"), te.transform.position, Quaternion.identity);
                Destroy(te.GetComponent<EnemyScript>().HPBar.gameObject);
                Destroy(te.GetComponent<EnemyScript>().StatusBar.gameObject);
                Destroy(te.gameObject);
                
                AudioClip s = Resources.Load<AudioClip>("Sounds/SE/mutant_frog/mutant_frog-1");
                SoundManager.PlaySE(s);
                yield return new WaitForSeconds(coolTime);
            }
            else yield return new WaitForSeconds(1);
        }
    }

    IEnumerator Fear(float coolTime)
    {
        while(true)
        {
            if(StageTime.text == "0") break;
            Collider2D[] cols = Physics2D.OverlapCircleAll(Hero.GetComponent<Collider2D>().bounds.center, 2);
            AudioClip s = Resources.Load<AudioClip>("Sounds/SE/80-CC0-creature-SFX/misc_05");
            SoundManager.PlaySE(s);
            foreach(Collider2D col in cols)
            {
                if(col.tag == "Enemy")
                {
                    col.GetComponent<EnemyScript>().AddStatus("0004");
                }
            }
            yield return new WaitForSeconds(coolTime);
        }
    }

    IEnumerator SummonProjectile(string _projectileName, float summonCoolTime, float _projectileDmg, bool _ignoreArmor = false)
    {
        var projectileSummon = Resources.Load<GameObject>($"Projectiles/Projectile{_projectileName}");
        while(true)
        {
            if(StageTime.text == "0") break;
            GameObject tg = Hero.GetComponent<HeroScript>().target;
            Vector3 summonPositon = Hero.GetComponent<Collider2D>().bounds.center;
            GameObject pjt = Instantiate(projectileSummon, summonPositon, Quaternion.identity);
            pjt.GetComponentInChildren<ProjectileScript>().SetProjectile(Hero, tg, false, "Straight", _projectileDmg, _ignoreArmor);
            if(Hero.GetComponent<HeroScript>().abilities.Contains("0024"))
            {
                GameObject tg2 = Hero.GetComponent<HeroScript>().target2;
                GameObject pjt2 = Instantiate(projectileSummon, summonPositon, Quaternion.identity);
                pjt2.GetComponentInChildren<ProjectileScript>().SetProjectile(Hero, tg2, false, "Straight", _projectileDmg, _ignoreArmor);
            }
            yield return new WaitForSeconds(summonCoolTime);
        }
    }

    IEnumerator BloodTransfusion(float coolTime)
    {
        while(true)
        {
            if(StageTime.text == "0") break;
            GameObject tm = GameObject.FindWithTag("Minion");
            if(tm != null && Hero.GetComponent<HeroScript>().nowHP <= Hero.GetComponent<HeroScript>().maxHP / 2)
            {
                AudioClip s = Resources.Load<AudioClip>("Sounds/SE/80-CC0-creature-SFX/apple_bite");
                SoundManager.PlaySE(s);
                Instantiate(Resources.Load<GameObject>("Effects/Predate"), tm.GetComponent<Collider2D>().bounds.center, Quaternion.identity);
                float h = tm.GetComponent<MinionScript>().minionMaxHP;
                Hero.GetComponent<HeroScript>().nowHP += h;
                RectTransform text = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), tm.GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
                text.GetComponent<FloatingText>().SetText($"{h}", "#00FF00");
                text.position = Camera.main.WorldToScreenPoint(new Vector3(Hero.GetComponent<Collider2D>().bounds.center.x, Hero.GetComponent<Collider2D>().bounds.center.y, 0));
                tm.GetComponent<MinionScript>().minionNowHP = 0;
                yield return new WaitForSeconds(coolTime);
            }
            else yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator SummonTrap(GameObject summoner, string _trapID, float summonCoolTime)
    {
        var trapToSummon = Resources.Load<GameObject>($"Traps/Trap{_trapID}");
        while(true)
        {
            if(StageTime.text == "0") break;
            if(summoner != null)
            {
                Vector3 summonPositon;
                if(summoner.tag == "Player") summonPositon = new Vector3(summoner.GetComponent<Collider2D>().bounds.center.x + Random.Range(-5f,5f), summoner.GetComponent<Collider2D>().bounds.center.y + Random.Range(-5f,5f), summoner.GetComponent<Collider2D>().bounds.center.z);
                else summonPositon = new Vector3(summoner.GetComponent<Collider2D>().bounds.center.x, summoner.GetComponent<Collider2D>().bounds.center.y, summoner.GetComponent<Collider2D>().bounds.center.z);
                Instantiate(trapToSummon, summonPositon, Quaternion.identity);
            }
            else break;

            yield return new WaitForSeconds(summonCoolTime);
        }
    }

    IEnumerator SummonOrbiting(string _orbitingName, int _orbitingNumber, float _orbitalDistance, float _orbitingDmg, bool _ignoreArmor = false)
    {
        var orbitalSummon = Resources.Load<GameObject>($"Orbitings/Orbiting{_orbitingName}");
        for(int i=0; i<_orbitingNumber; i++)
        {
            Vector3 summonPositon = Hero.GetComponent<Collider2D>().bounds.center + new Vector3(_orbitalDistance * Mathf.Sin(Mathf.Deg2Rad * (i * (360 / _orbitingNumber))), _orbitalDistance * Mathf.Cos(Mathf.Deg2Rad * (i * (360 / _orbitingNumber))), 0);
            GameObject o = Instantiate(orbitalSummon, summonPositon, Quaternion.identity);
            o.GetComponentInChildren<OrbitingScript>().SetOrbiting(Hero, _orbitalDistance, i * (360 / _orbitingNumber), _orbitingDmg, _ignoreArmor);
        }
        yield return null;
    }

    IEnumerator Heal(float _coolTime, float _value)
    {
        while(true)
        {
            if(StageTime.text == "0") break;
            Hero.GetComponent<HeroScript>().BeHealed(_value);
            yield return new WaitForSeconds(_coolTime);
        }
    }

    IEnumerator Buffs(string _statusID, float _coolTime, float wait = 0, string _buffTarget = "minion")
    {
        yield return new WaitForSeconds(wait + 0.1f);
        while(true)
        {
            if(StageTime.text == "0") break;
            if(_buffTarget == "minion")
            {
                GameObject[] M = GameObject.FindGameObjectsWithTag("Minion");
                if(M.Length > 0)
                {
                    foreach(GameObject m in M)
                    {
                        m.GetComponent<MinionScript>().AddStatus(_statusID);
                    }
                }
            }
            else if(_buffTarget == "hero")
            {
                GameObject p = GameObject.FindWithTag("Player");
                if(p != null) p.GetComponent<HeroScript>().AddStatus(_statusID);
            }
            yield return new WaitForSeconds(_coolTime);
        }
    }

    IEnumerator AreaDamage(string _name, float _coolTime, float _range, float _dmg)
    {
        while(true)
        {
            if(StageTime.text == "0") break;
            GameObject t = Hero.GetComponent<HeroScript>().target;
            Vector2 tp = Vector2.zero;
            if(t != null) tp = t.transform.position;
            else tp = new Vector2(Random.Range(-10f, 10f), Random.Range(-5f, 5f));
            GameObject s = Instantiate(Resources.Load<GameObject>("AreaDamage/AreaDamage"), tp, Quaternion.identity);
            s.transform.localScale = new Vector2(s.transform.localScale.x * _range, s.transform.localScale.y * _range);
            GameObject spjt = Instantiate(Resources.Load<GameObject>($"AreaDamage/AreaDamage{_name}"), new Vector2(tp.x, tp.y + 10), Quaternion.identity);
            AudioClip se = null;
            if(_name == "MissileBombing")
            {
                se = Resources.Load<AudioClip>("Sounds/SE/rocket_launches/rocket_launche_1");
            }
            if(se != null) SoundManager.PlaySE2(se);
            for(int i=0; i<50; i++)
            {
                spjt.transform.Translate(Vector2.down * 0.2f);
                yield return new WaitForSeconds(0.01f);
            }
            Instantiate(Resources.Load<GameObject>("Effects/Explosion01"), s.transform.position, Quaternion.identity);
            if(_name == "MissileBombing")
            {
                se = Resources.Load<AudioClip>("Sounds/SE/explosions/explosion07");
            }
            if(se != null) SoundManager.PlaySE2(se);
            Collider2D[] cols = Physics2D.OverlapCircleAll(s.transform.position, _range * 0.5f);
            foreach(Collider2D col in cols)
            {
                if(col.tag == "Enemy")
                {
                    col.GetComponent<EnemyScript>().BeAttacked(_dmg, 0.5f);
                }
            }
            Destroy(spjt);
            Destroy(s);
            yield return new WaitForSeconds(_coolTime);
        }
    }

    IEnumerator Strike(float _coolTime)
    {
        yield return new WaitForSeconds(_coolTime);
        while(true)
        {
            if(StageTime.text == "0") break;
            GameObject o = Instantiate(Resources.Load<GameObject>("ETC/Strike"), new Vector2(Camera.main.transform.position.x + 15, Camera.main.transform.position.y), Quaternion.identity);
            AudioClip se = Resources.Load<AudioClip>("Sounds/SE/bowling/qubodup-bowling-roll/qubodup-bowling_roll");
            if(se != null) SoundManager.PlaySE2(se);
            for(int i=0; i<140; i++)
            {
                o.transform.Translate(Vector2.left * 0.1f);
                yield return new WaitForSeconds(0.01f);
            }
            Collider2D[] cols = Physics2D.OverlapCircleAll(o.transform.position, 4f);
            foreach(Collider2D col in cols)
            {
                if(col.tag == "Enemy")
                {
                    col.GetComponent<EnemyScript>().BeAttacked(1700, 1f);
                }
            }
            se = Resources.Load<AudioClip>("Sounds/SE/bowling/juskiddink-bowling-strike/strike3n-5");
            if(se != null) SoundManager.PlaySE2(se);
            Destroy(o);
            yield return new WaitForSeconds(_coolTime);
        }
    }

    IEnumerator AreaHeal(GameObject healer, float _coolTime, float _range, float _value)
    {
        while(true)
        {
            if(StageTime.text == "0") break;
            GameObject ef = Instantiate(Resources.Load<GameObject>("Effects/AreaHeal"), healer.transform.position, Quaternion.identity);
            ef.transform.localScale = new Vector2(ef.transform.localScale.x * _range, ef.transform.localScale.y * _range);
            AudioClip s = Resources.Load<AudioClip>("Sounds/SE/RPG_Essentials_Free/8_Buffs_Heals_SFX/02_Heal_02");
            SoundManager.PlaySE(s);
            Collider2D[] cols = Physics2D.OverlapCircleAll(healer.transform.position, _range * 0.5f);
            foreach(Collider2D col in cols)
            {
                if(col.tag == "Player")
                {
                    col.GetComponent<HeroScript>().BeHealed(_value);
                }
                else if(col.tag == "Minion")
                {
                    col.GetComponent<MinionScript>().BeHealed(_value);
                }
            }
            yield return new WaitForSeconds(_coolTime);
        }
        
    }
}
