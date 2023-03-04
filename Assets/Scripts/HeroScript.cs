using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroScript : MonoBehaviour
{
    Animator animator;
    GameObject Map;
    DataManager DataManager;

    public int _heroID;
    public string heroNameKR;
    public string[] attributes;
    public float maxHP;
    public float nowHP;
    public float HPRegeneration;
    public float atkDmg;
    public float atkSpeed;
    public float atkRange;
    public float criticalDmg;
    public float criticalChance;
    public float armor;
    public float moveSpeed;
    public string abilityKR;

    private float curTime;
    public float atkCoolTime;
    public Vector2 boxSize;

    public Image HPbar;
    public Text TextMaxHP;
    public Text TextNowHP;
    
    void Start()
    {
        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
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
        int idx = DataManager.AllHeroList.FindIndex(x => x.heroID == stringID);
        Hero heroInfo = DataManager.AllHeroList[idx];
        heroNameKR = heroInfo.heroNameKR;
        attributes = heroInfo.heroAttributes;
        maxHP = float.Parse(heroInfo.heroMaxHP);
        nowHP = maxHP;
        HPRegeneration = float.Parse(heroInfo.heroHPRegeneration);
        atkDmg = float.Parse(heroInfo.heroAtkDmg);
        atkSpeed = float.Parse(heroInfo.heroAtkSpeed);
        atkCoolTime = 10 / atkSpeed;
        atkRange = float.Parse(heroInfo.heroAtkRange);
        boxSize = new Vector2(atkRange, atkRange);
        criticalDmg = float.Parse(heroInfo.heroCriticalDmg);
        criticalChance = float.Parse(heroInfo.heroCriticalChance);
        armor = float.Parse(heroInfo.heroArmor);
        moveSpeed = float.Parse(heroInfo.heroMoveSpeed);
        abilityKR = heroInfo.heroAbilityKR;

        if(_heroID == 0)
        {
            StartCoroutine(SummonMinion("0000", 20.0f));
        }

        animator = GetComponent<Animator>();
        transform.position = new Vector2(0, 0);

        Map = GameObject.Find("Map");
        HPbar = GameObject.Find("HeroHPBar").GetComponent<Image>();
        TextMaxHP = GameObject.Find("HeroMaxHPText").GetComponent<Text>();
        TextNowHP = GameObject.Find("HeroNowHPText").GetComponent<Text>();
        InvokeRepeating("UpdateTarget", 0, 0.25f);
        InvokeRepeating("HPRegenerationMethod", 0, 1);
    }

    GameObject target = null;
    bool isCritical = false;
    void Update()
    {
        TextMaxHP.text = Mathf.Ceil(maxHP).ToString();
        TextNowHP.text = Mathf.Ceil(nowHP).ToString();
        HPbar.fillAmount = nowHP / maxHP;
        if(nowHP / maxHP < 0.3f) HPbar.color = Color.red;
        else HPbar.color = Color.green;

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


        if(
            transform.position.x + moveDirection.x * Time.deltaTime * moveSpeed + transform.localScale.x * 0.5 > Map.transform.localScale.x * -0.5f
            && transform.position.x + moveDirection.x * Time.deltaTime * moveSpeed + transform.localScale.x * 0.5 < Map.transform.localScale.x * 0.5f
            && transform.position.y + moveDirection.y * Time.deltaTime * moveSpeed + transform.localScale.y * -0.5f > Map.transform.localScale.y * -0.5f
            && transform.position.y + moveDirection.y * Time.deltaTime * moveSpeed + transform.localScale.y * 0.5 < Map.transform.localScale.y * 0.5f
        )
        {
            transform.Translate(moveDirection * Time.deltaTime * moveSpeed);
        }

        animator.SetBool("isMoving", moveDirection.magnitude > 0);
        
        // 공격
        if(curTime <= 0)
        {
            if(target != null)
            {
                Vector2 targetPos = target.transform.position - transform.position;

                if(targetPos.x * transform.localScale.x < 0) transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(transform.position, boxSize, 0);
                foreach(Collider2D collider in collider2Ds)
                {
                    if(collider.tag == "Enemy")
                    {
                        if(Random.Range(0, 100) < criticalChance) isCritical = true;
                        else isCritical = false;
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
        Vector2 box = new Vector2(2, 2);
        Collider2D[] tempCols = Physics2D.OverlapBoxAll(transform.position, box, 0);
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
            float minDistance = Vector2.Distance(transform.position, cols[0].transform.position);
            int minDisIdx = 0;
            for(int i=0; i < cols.Length; i++)
            {
                if(Vector2.Distance(transform.position, cols[i].transform.position) < minDistance)
                {
                    minDistance = Vector2.Distance(transform.position, cols[i].transform.position);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
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
            Vector3 summonPositon = transform.position + Vector3.right;
            Instantiate(minionToSummon, summonPositon, Quaternion.identity);
            yield return new WaitForSeconds(summonCoolTime);
        }
    }

}
