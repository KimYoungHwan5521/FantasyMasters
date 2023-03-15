using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HeroScript : MonoBehaviour
{
    Animator animator;
    GameObject Map;
    DataManager DataManager;

    Hero heroInfo;
    public int _heroID;
    public string stringID;
    public string heroNameKR;
    public string[] attributes;
    public float maxHP;
    public float maxHPCV = 0;
    public float nowHP;
    public float HPRegeneration;
    public int atkType;
    public float atkDmg;
    public float atkDmgCV = 0;
    public float atkSpeed;
    public float atkSpeedCVM = 1;
    public float atkRange;
    public float criticalDmg;
    public float criticalChance;
    public float armor;
    public float armorCV = 0;
    public float moveSpeed;
    public float moveSpeedCVM = 1;
    public List<string> abilities;
    public List<StatusV> HeroStatus;
    public List<Item> HeroItems;
    public GameObject StatusSprites;

    private float curTime;
    public float atkCoolTime;
    public Vector2 boxSize;

    public Image HPbar;
    public Text TextMaxHP;
    public Text TextNowHP;

    public class StatusV
    {
        public string statusID;
        public string statusNameKR;
        public string statusExplainKR;
        public string[] buffStat;
        public float[] buffValue;
        public float buffTime;
    }
    
    void Start()
    {
        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        _heroID = DataManager.selectedHeroID;
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
        int idx = DataManager.AllHeroList.FindIndex(x => x.heroID == stringID);
        heroInfo = DataManager.AllHeroList[idx];
        heroNameKR = heroInfo.heroNameKR;
        attributes = heroInfo.heroAttributes;
        maxHP = float.Parse(heroInfo.heroMaxHP);
        nowHP = maxHP;
        HPRegeneration = float.Parse(heroInfo.heroHPRegeneration);
        atkType = int.Parse(heroInfo.heroAtkType);
        atkDmg = float.Parse(heroInfo.heroAtkDmg.Split('x')[0]);
        atkSpeed = float.Parse(heroInfo.heroAtkSpeed);
        atkCoolTime = 10 / atkSpeed;
        atkRange = float.Parse(heroInfo.heroAtkRange);
        boxSize = new Vector2(atkRange, atkRange);
        criticalDmg = float.Parse(heroInfo.heroCriticalDmg);
        criticalChance = float.Parse(heroInfo.heroCriticalChance);
        armor = float.Parse(heroInfo.heroArmor);
        moveSpeed = float.Parse(heroInfo.heroMoveSpeed);
        abilities = heroInfo.heroAbilities.ToList();
        HeroStatus = new List<StatusV>();
        StatusSprites = GameObject.Find("HeroStatus");
        HeroItems = new List<Item>();

        animator = GetComponentInChildren<Animator>();
        transform.position = new Vector2(0, 0);

        Map = GameObject.Find("Map");
        HPbar = GameObject.Find("HeroHPBar").GetComponent<Image>();
        TextMaxHP = GameObject.Find("HeroMaxHPText").GetComponent<Text>();
        TextNowHP = GameObject.Find("HeroNowHPText").GetComponent<Text>();
        InvokeRepeating("UpdateTarget", 0, 0.25f);
        InvokeRepeating("HPRegenerationMethod", 0, 1);
    }

    public GameObject target = null;
    public bool isCritical = false;
    private int projectileCount = 0;
    void Update()
    {
        TextMaxHP.text = Mathf.Ceil(maxHP).ToString();
        TextNowHP.text = Mathf.Ceil(nowHP).ToString();
        HPbar.fillAmount = nowHP / maxHP;
        if(nowHP / maxHP < 0.3f) HPbar.color = Color.red;
        else HPbar.color = Color.green;
        atkSpeed = float.Parse(DataManager.AllHeroList[_heroID].heroAtkSpeed) * atkSpeedCVM;
        animator.SetFloat("AttackSpeed", atkSpeedCVM);
        atkCoolTime = 10 / atkSpeed;
        moveSpeed = float.Parse(DataManager.AllHeroList[_heroID].heroMoveSpeed) * moveSpeedCVM;
        animator.SetFloat("MoveSpeed", moveSpeedCVM);
        atkDmg = float.Parse(DataManager.AllHeroList[_heroID].heroAtkDmg.Split('x')[0]) + atkDmgCV;
        armor = float.Parse(DataManager.AllHeroList[_heroID].heroArmor) + armorCV;

        // status timer
        if(HeroStatus.Count > 0)
        {
            for(int i=0; i<HeroStatus.Count; i++)
            {
                HeroStatus[i].buffTime -= Time.deltaTime;
                if(HeroStatus[i].buffTime <= 0)
                {
                    RemoveStatus(HeroStatus[i].statusID);
                    HeroStatus.RemoveAt(i);
                    Destroy(StatusSprites.transform.GetChild(i).gameObject);
                }
            }
        }

        // 이동
        Vector2 moveDirection = Vector2.zero;

        if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            moveDirection += Vector2.right;
            if(transform.localScale.x < 0)
            {
                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
            }
            else
            {
                transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
            }
        }
        if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector2.left;
            if(transform.localScale.x > 0)
            {
                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
            }
            else
            {
                transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
            }
        }
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector2.up;
            
        }
        if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector2.down;

        }
        moveDirection.Normalize();
        transform.Translate(moveDirection * Time.deltaTime * moveSpeed);

        animator.SetBool("isMoving", moveDirection.magnitude > 0);
        
        // 공격
        if(curTime <= 0)
        {
            if(target != null)
            {
                Vector2 targetPos = target.GetComponent<Collider2D>().bounds.center - GetComponent<Collider2D>().bounds.center;

                if(targetPos.x * transform.localScale.x < 0) transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                if(UnityEngine.Random.Range(0, 100) < criticalChance) isCritical = true;
                else isCritical = false;
                if(isCritical) animator.SetBool("isCritical", true);
                else animator.SetBool("isCritical", false);
                if(atkType == 2) projectileCount++;
                animator.SetTrigger("Attack");
                curTime = atkCoolTime;
            }
            else
            {
                animator.ResetTrigger("Attack");
            }
        }
        else
        {
            curTime -= Time.deltaTime;
        }
    }
    
    private void UpdateTarget()
    {
        Vector2 box = new Vector2(atkRange, atkRange);
        Collider2D[] tempCols = Physics2D.OverlapBoxAll(GetComponent<Collider2D>().bounds.center, box, 0);
        int cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Enemy") cnt++;
        }
        Collider2D[] cols = new Collider2D[cnt];
        cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Enemy") 
            {
                cols[cnt] = tempCols[i];
                cnt++;
            }
        }
        if(cols.Length > 0)
        {
            float minDistance = Vector2.Distance(GetComponent<Collider2D>().bounds.center, cols[0].GetComponent<Collider2D>().bounds.center);
            int minDisIdx = 0;
            for(int i=0; i < cols.Length; i++)
            {
                if(Vector2.Distance(GetComponent<Collider2D>().bounds.center, cols[i].GetComponent<Collider2D>().bounds.center) < minDistance)
                {
                    minDistance = Vector2.Distance(GetComponent<Collider2D>().bounds.center, cols[i].GetComponent<Collider2D>().bounds.center);
                    minDisIdx = i;
                }
            }
            target = cols[minDisIdx].gameObject;
        }
    }

    private void HPRegenerationMethod()
    {
        nowHP += 1;
        if(nowHP > maxHP) nowHP = maxHP;
    }

    private void MeleeAttack()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(GetComponent<Collider2D>().bounds.center, boxSize, 0);
        foreach(Collider2D collider in collider2Ds)
        {
            if(collider.tag == "Enemy")
            {
                if(isCritical)
                {
                    collider.GetComponent<EnemyScript>().BeAttacked(atkDmg * criticalDmg, 0.6f, isCritical);
                }
                else
                {
                    collider.GetComponent<EnemyScript>().BeAttacked(atkDmg, 0.3f, isCritical);
                }
            }
        }
    }

    private void RangedAttack()
    {
        if(projectileCount > 0)
        {
            GameObject p = Instantiate(Resources.Load<GameObject>($"Projectiles/ProjectileHero{stringID}"), GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
            p.GetComponentInChildren<ProjectileScript>().SetProjectile(gameObject, target, isCritical);
            // print($"spp, target: {gameObject}, {target}");
            projectileCount--;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetComponent<Collider2D>().bounds.center, boxSize);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            EnemyScript colES = collision.gameObject.GetComponent<EnemyScript>();
            float dmg = 0;
            RectTransform DmgText = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
            DmgText.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
            if(colES.enemyCollisionDmg - armor > 0) 
            {
                dmg = colES.enemyCollisionDmg - armor;
                nowHP -= dmg;
                DmgText.gameObject.GetComponent<FloatingText>().SetText(Mathf.Round(dmg).ToString(), "#FFAAAA");
                OnDamaged();
            }
            else DmgText.gameObject.GetComponent<FloatingText>().SetText(Mathf.Round(dmg).ToString());
        }
    }

    public void BeAttacked(float dmg)
    {
        dmg -= armor;
        RectTransform DmgText = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
        DmgText.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
        if(dmg > 0)
        {
            nowHP -= dmg;
            DmgText.gameObject.GetComponent<FloatingText>().SetText(Mathf.Round(dmg).ToString(), "#FFAAAA");
            OnDamaged();
        }
        else DmgText.gameObject.GetComponent<FloatingText>().SetText("0", "#FFAAAA");
    }

    void OnDamaged()
    {
        //충돌시 플레이어의 레이어가 PlayerInv 레이어로 변함 
        gameObject.layer = 12;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);//무적시간일 때 플레이어가 투명하게
        Invoke("OffDamaged", 0.3f);
    }

    void OffDamaged()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.layer = 11;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void AddStatus(string _statusID)
    {
        int isAlreadyGotIt = HeroStatus.FindIndex(x => x.statusID == _statusID);
        if(isAlreadyGotIt != -1)
        {
            HeroStatus[isAlreadyGotIt].buffTime = float.Parse(DataManager.AllStatusList.Find(x => x.statusID == _statusID).buffTime);
        }
        else
        {
            Status _status = DataManager.AllStatusList.Find(x => x.statusID == _statusID);
            StatusV tempStatus = new StatusV(); 
            tempStatus.statusID = _status.statusID;
            tempStatus.statusNameKR = _status.statusNameKR;
            tempStatus.statusExplainKR = _status.statusExplainKR;
            tempStatus.buffStat = new string[_status.buffStat.Length];
            Array.Copy(_status.buffStat, tempStatus.buffStat, _status.buffStat.Length);
            tempStatus.buffValue = Array.ConvertAll(_status.buffValue, x => float.Parse(x));

            for(int i=0; i<_status.buffStat.Length; i++)
            {
                if(_status.buffStat[i] == "atkSpeedCVM")
                {
                    atkSpeedCVM += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "moveSpeedCVM")
                {
                    moveSpeedCVM += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "atkDmgCV")
                {
                    atkDmgCV += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "armorCV")
                {
                    armorCV += float.Parse(_status.buffValue[i]);
                }
                else
                {
                    print($"wrong buffStat name : '{_status.buffStat[i]}'");
                }
            }
            Instantiate(Resources.Load($"UIs/Icons/Status{_statusID}"), new Vector2(0, 0), Quaternion.identity, GameObject.Find("HeroStatus").transform);
            tempStatus.buffTime = float.Parse(_status.buffTime);
            HeroStatus.Add(tempStatus);
        }
    }

    public void RemoveStatus(string _statusID)
    {
        int idx = HeroStatus.FindIndex(x => x.statusID == _statusID);
        for(int i=0; i<HeroStatus[idx].buffStat.Length; i++)
        {
            if(HeroStatus[idx].buffStat[i] == "atkSpeedCVM")
            {
                atkSpeedCVM -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "moveSpeedCVM")
            {
                moveSpeedCVM -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "atkDmgCV")
            {
                atkDmgCV -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "armorCV")
            {
                armorCV -= HeroStatus[idx].buffValue[i];
            }
        }
    }

    public void AddItem(string _itemID)
    {
        Item _item = DataManager.AllItemList.Find(x => x.itemID == _itemID);
        
        for(int i=0; i<_item.itemBuffStat.Length; i++)
        {
            if(_item.itemBuffStat[i] == "atkSpeedCVM")
            {
                atkSpeedCVM += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "moveSpeedCVM")
            {
                moveSpeedCVM += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "atkDmgCV")
            {
                atkDmgCV += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "armorCV")
            {
                armorCV += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "maxHPCV")
            {
                maxHPCV += float.Parse(_item.itemBuffValue[i]);
                maxHP = float.Parse(DataManager.AllHeroList[_heroID].heroMaxHP) + maxHPCV;
                if(maxHP < 1) maxHP = 1;
            }
            else
            {
                print($"wrong itemBuffStat name : '{_item.itemBuffStat[i]}'");
            }
        }
        for(int i=0; i<_item.itemAbilities.Count; i++)
        {
            abilities.Add(_item.itemAbilities[i]);
        }
        HeroItems.Add(_item);

    }

}
