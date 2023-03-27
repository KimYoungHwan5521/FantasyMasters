using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class StageManager : MonoBehaviour
{
    NoticeUI _notice;
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
    }
    public List<Product> AllProductList;
    
    GameObject Hero;
    void Awake()
    {
        _notice = FindObjectOfType<NoticeUI>();
    }

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
        Hero.GetComponent<HeroScript>().nowHP = Hero.GetComponent<HeroScript>().maxHP;
        Hero.transform.position = new Vector2(0, 0);
        for(int i=3; i>-1; i--)
        {
            FloatingText CountText = Instantiate(Resources.Load<FloatingText>("Effects/FloatingText"), new Vector2(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);
            CountText.gameObject.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0));
            if(i == 0) CountText.SetText("<size=100>Start!</size>", "#FF0000");
            else CountText.SetText($"<size=100>{i.ToString()}</size>", "#FF0000");
            yield return new WaitForSeconds(1);
        }
        stageTime = stageInfo.Split('|').Length * 5;
        // stageTime = 60;
        StartCoroutine(StageTimer());
        List<string> hAbilities = Hero.GetComponent<HeroScript>().abilities;
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
            StartCoroutine(SummonProjectile("SkullThrowing",float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0018").abilityCoolTime), 100));
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
            exception++;
            if(exception > 100) break;
        }
        stageNumber++;
        if(CurStageList.Find(x => x.stageNumber == stageNumber.ToString()) != null) yield return StartCoroutine(ReadyToNextStage());
        else yield return StartCoroutine(ClearMap());
    }

    public GameObject Shop;
    public GameObject[] ProductsSimple;
    public List<Product> CurProductList;
    IEnumerator ReadyToNextStage()
    {
        CurProductList = new List<Product>();
        selectedProduct = -1;
        string[] hAttributes = Hero.GetComponent<HeroScript>().attributes;
        for(int i=0;i<5;i++)
        {
            int rd = 0;
            int random = Random.Range(0, 1000);
            if(random < 700) rd = 0;
            else if(random < 910) rd = 1;
            else if(random < 973) rd = 2;
            else rd = 3;
            List<Product> tempPdl = new List<Product>();
            if(i<4)
            {
                if(hAttributes.Length > 1 && i > 1)
                {
                    tempPdl = AllProductList.FindAll(x => x.attributes.ToList().Contains(hAttributes[1]));
                }
                else
                {
                    tempPdl = AllProductList.FindAll(x => x.attributes.ToList().Contains(hAttributes[0]));
                }
            }
            else
            {
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
                print($"i, tempPdl[r].productName : {i}, {tempPdl[r].productName}");
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
            }
        }
        for(int i=0; i<CurProductList.Count; i++)
        {
            Image ProductImage = ProductsSimple[i].GetComponentsInChildren<Image>()[1];
            if(CurProductList[i].productType == "아이템") ProductImage.sprite = Resources.Load<Sprite>($"UIs/Icons/Products/Items/Item{CurProductList[i].inheritanceID}");
            else ProductImage.sprite = Resources.Load<Sprite>($"UIs/Icons/Products/Abilities/Ability{CurProductList[i].inheritanceID}");
            Text[] ProductSimpleTexts = ProductsSimple[i].GetComponentsInChildren<Text>();
            ProductSimpleTexts[0].text = CurProductList[i].productName;
            ProductSimpleTexts[1].text = "";
            for(int j=0; j<CurProductList[i].attributes.Length; j++)
            {
                if(j>0) ProductSimpleTexts[1].text += " ";
                ProductSimpleTexts[1].text += CurProductList[i].attributes[j];
            }
            ProductSimpleTexts[2].text = CurProductList[i].productType;
            if(CurProductList[i].rareDegree == "0") ProductSimpleTexts[3].text = "일반";
            else if(CurProductList[i].rareDegree == "1") ProductSimpleTexts[3].text = "<color=blue>희귀</color>";
            else if(CurProductList[i].rareDegree == "2") ProductSimpleTexts[3].text = "<color=purple>신화</color>";
            else ProductSimpleTexts[3].text = "<color=yellow>전설</color>";
        }
        Shop.SetActive(true);
        yield return null;
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
    IEnumerator SummonMinion(string _minionID, float summonCoolTime, int n = 1)
    {
        var minionToSummon = Resources.Load<GameObject>($"Minions/Minion{_minionID}");
        while(true)
        {
            if(StageTime.text == "0") break;
            Vector3 summonPositon = Hero.GetComponent<Collider2D>().bounds.center;
            for(int i=0; i<n; i++)
            {
                Instantiate(minionToSummon, summonPositon, Quaternion.identity);
            }
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
                te.GetComponent<EnemyScript>().enemyNowHP -= 30;
                GameObject Pjt = Instantiate(effect, te.transform.position, Quaternion.identity);
                Pjt.GetComponentInChildren<ProjectileScript>().SetProjectile(te, Hero, false, "LifeDrain");
                yield return new WaitForSeconds(coolTime);
            }
            else yield return new WaitForSeconds(1);
        }
    }

    IEnumerator Polymorph(string polymorphTo, float coolTime)
    {
        int exception = 0;
        while(true)
        {
            if(exception > 1000) break;
            if(StageTime.text == "0") break;
            GameObject te = GameObject.FindWithTag("Enemy");
            if(te != null)
            {
                Instantiate(Resources.Load<GameObject>($"Enemies/Enemy{polymorphTo}"), te.transform.position, Quaternion.identity);
                Destroy(te.GetComponent<EnemyScript>().HPBar.gameObject);
                Destroy(te.GetComponent<EnemyScript>().StatusBar.gameObject);
                Destroy(te.gameObject);
                yield return new WaitForSeconds(coolTime);
            }
            else yield return new WaitForSeconds(1);
            exception++;
        }
    }

    IEnumerator Fear(float coolTime)
    {
        int exception = 0;
        while(true)
        {
            if(exception > 1000)
            if(StageTime.text == "0") break;
            Collider2D[] cols = Physics2D.OverlapBoxAll(Hero.GetComponent<Collider2D>().bounds.center, new Vector2(3, 3), 0);
            foreach(Collider2D col in cols)
            {
                if(col.tag == "Enemy")
                {
                    col.GetComponent<EnemyScript>().AddStatus("0004");
                }
            }
            yield return new WaitForSeconds(coolTime);
            exception++;
        }
    }

    IEnumerator SummonProjectile(string _projectileName, float summonCoolTime, float _projectileDmg)
    {
        var projectileSummon = Resources.Load<GameObject>($"Projectiles/Projectile{_projectileName}");
        while(true)
        {
            if(StageTime.text == "0") break;
            GameObject tg = Hero.GetComponent<HeroScript>().target;
            Vector3 summonPositon = Hero.GetComponent<Collider2D>().bounds.center;
            GameObject pjt = Instantiate(projectileSummon, summonPositon, Quaternion.identity);
            pjt.GetComponentInChildren<ProjectileScript>().SetProjectile(Hero, tg, false, "Straight", _projectileDmg);
            if(Hero.GetComponent<HeroScript>().abilities.Contains("0024"))
            {
                GameObject tg2 = Hero.GetComponent<HeroScript>().target2;
                GameObject pjt2 = Instantiate(projectileSummon, summonPositon, Quaternion.identity);
                pjt2.GetComponentInChildren<ProjectileScript>().SetProjectile(Hero, tg2, false, "Straight", _projectileDmg);
            }
            yield return new WaitForSeconds(summonCoolTime);
        }
    }

    IEnumerator BloodTransfusion(float coolTime)
    {
        int exception = 0;
        while(true)
        {
            if(exception > 1000)
            if(StageTime.text == "0") break;
            GameObject tm = GameObject.FindWithTag("Minion");
            if(tm != null && Hero.GetComponent<HeroScript>().nowHP <= Hero.GetComponent<HeroScript>().maxHP / 2)
            {
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
            exception++;
        }
    }

}
