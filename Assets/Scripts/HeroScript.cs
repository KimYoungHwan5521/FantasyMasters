using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public float nowHP;
    public float HPRegeneration;
    public int atkType;
    public float atkDmg;
    public float atkSpeed;
    public float atkSpeedCVM = 1;
    public float atkRange;
    public float criticalDmg;
    public float criticalChance;
    public float armor;
    public float moveSpeed;
    public List<string> abilities;
    public List<StatusV> HeroStatus;

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
        atkDmg = float.Parse(heroInfo.heroAtkDmg);
        atkSpeed = float.Parse(heroInfo.heroAtkSpeed);
        atkCoolTime = 10 / atkSpeed;
        atkRange = float.Parse(heroInfo.heroAtkRange);
        boxSize = new Vector2(atkRange, atkRange);
        criticalDmg = float.Parse(heroInfo.heroCriticalDmg);
        criticalChance = float.Parse(heroInfo.heroCriticalChance);
        armor = float.Parse(heroInfo.heroArmor);
        moveSpeed = float.Parse(heroInfo.heroMoveSpeed);
        abilities = heroInfo.heroAbilities;
        HeroStatus = new List<StatusV>();
        if(abilities.Find(x => x.Equals("0000")) != null)
        {
            StartCoroutine(SummonMinion("0000", 20.0f));
        }

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

        print($"atkSpeed: {atkSpeed}, atkSpeedCVM: {atkSpeedCVM}");
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
                animator.SetTrigger("Attack");
                curTime = atkCoolTime;
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
                    collider.GetComponent<EnemyScript>().BeAttacked(atkDmg * criticalDmg, 0.6f);
                }
                else
                {
                    collider.GetComponent<EnemyScript>().BeAttacked(atkDmg, 0.3f);
                }
            }
        }
    }

    private void RangedAttack()
    {
        GameObject chk = GameObject.Find($"ProjectileHero{stringID}(Clone)");
        if(!chk)
        {
            Instantiate(Resources.Load<GameObject>($"Projectiles/ProjectileHero{stringID}"), GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
        } 
        else
        {
            if(isCritical) chk.GetComponentInChildren<ProjectileScript>().isCritical = true;
            else chk.GetComponent<ProjectileScript>().isCritical = false;
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
            nowHP -= colES.enemyCollisionDmg;
            print($"PlayerHP: {nowHP}");
            OnDamaged(collision.transform.position);
        }
    }

    void OnDamaged(Vector2 targetPos)
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

    IEnumerator SummonMinion(string _minionID, float summonCoolTime)
    {
        var minionToSummon = Resources.Load<GameObject>($"Minions/Minion{_minionID}");
        while(true)
        {
            Vector3 summonPositon = GetComponent<Collider2D>().bounds.center;
            Instantiate(minionToSummon, summonPositon, Quaternion.identity);
            yield return new WaitForSeconds(summonCoolTime);
        }
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
                    atkSpeedCVM *= float.Parse(_status.buffValue[i]);
                }
                else
                {
                    print($"wrong buffStat name : '{_status.buffStat[i]}'");
                }
            }
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
                atkSpeedCVM /= HeroStatus[idx].buffValue[i];
            }
        }
    }

}
