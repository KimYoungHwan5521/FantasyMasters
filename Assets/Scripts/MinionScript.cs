using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionScript : MonoBehaviour
{
    DataManager DataManager;
    public int _minionID;
    public string minionNameKR;
    public string[] minionAttributes;
    public float minionMaxHP;
    public float minionNowHP;
    public float minionExistencetime;
    public int minionAtkType;
    public float minionAtkDmg;
    public float minionAtkSpeed;
    public float minionAtkRange;
    public float minionCriticalDmg;
    public float minionCriticalChance;
    public float minionArmor;
    public float minionMoveSpeed;
    public string minionAbilityKR;

    Animator animator;
    public RectTransform HPBar;

    private float curTime;
    public float atkCoolTime;
    
    void Start()
    {
        string stringID;
        int cntDgit = 0;
        int copy_minionID = _minionID;
        for(int i=0; i<4; i++)
        {
            if(copy_minionID / 10 > 0)
            {
                cntDgit++;
                copy_minionID /= 10;
            }
            else break;
        }
        stringID = "";
        for(int i=0;i<3 - cntDgit / 10; i++)
        {
            stringID += "0";
        }
        stringID += _minionID.ToString();
        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        int idx = DataManager.AllMinionList.FindIndex(x => x.minionID == stringID);
        Minion minionInfo = DataManager.AllMinionList[idx];
        minionNameKR = minionInfo.minionNameKR;
        minionAttributes = minionInfo.minionAttributes;
        minionMaxHP = float.Parse(minionInfo.minionMaxHP);
        minionNowHP = minionMaxHP;
        minionExistencetime = float.Parse(minionInfo.minionExistencetime);
        minionAtkType = int.Parse(minionInfo.minionAtkType);
        minionAtkDmg = float.Parse(minionInfo.minionAtkDmg);
        minionAtkSpeed = float.Parse(minionInfo.minionAtkSpeed);
        atkCoolTime = 10 / minionAtkSpeed;
        minionAtkRange = float.Parse(minionInfo.minionAtkRange);
        boxSize = new Vector2(minionAtkRange, minionAtkRange);
        minionCriticalDmg = float.Parse(minionInfo.minionCriticalDmg);
        minionCriticalChance = float.Parse(minionInfo.minionCriticalChance);
        minionArmor = float.Parse(minionInfo.minionArmor);
        minionMoveSpeed = float.Parse(minionInfo.minionMoveSpeed);
        minionAbilityKR = minionInfo.minionAbilityKR;

        animator = GetComponent<Animator>();
        HPBar = Instantiate(Resources.Load<RectTransform>("UIs/HPBar"), new Vector3(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);

        target = null;
        InvokeRepeating("UpdateTarget", 0, 0.25f);

    }

    Vector2 moveDirection;
    GameObject target;
    bool isCritical = false;
    void Update()
    {
        if(minionNowHP <= 0)
        {
            HPBar.GetComponent<Image>().fillAmount = 0;
            animator.SetTrigger("Dead");
            animator.SetBool("isMoving", false);
        }
        else
        {
            HPBar.position = Camera.main.WorldToScreenPoint(new Vector3(transform.GetComponent<Collider2D>().bounds.center.x, transform.GetComponent<Collider2D>().bounds.center.y - GetComponent<BoxCollider2D>().size.y * 2, 0));
            HPBar.GetComponent<Image>().fillAmount = minionNowHP / minionMaxHP;
            if(target != null)
            {
                if(Vector2.Distance(transform.GetComponent<Collider2D>().bounds.center, target.GetComponent<Collider2D>().bounds.center) < minionAtkRange)
                {
                    // animator.SetBool("isMoving", false);
                    if(curTime <= 0)
                    {
                        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(transform.GetComponent<Collider2D>().bounds.center, boxSize, 0);
                        foreach(Collider2D collider in collider2Ds)
                        {
                            if(collider.tag == "Enemy")
                            {
                                if(Random.Range(0, 100) < minionCriticalChance) isCritical = true;
                                else isCritical = false;
                                if(isCritical)
                                {
                                    collider.GetComponent<EnemyScript>().BeAttacked(minionAtkDmg * minionCriticalDmg, 0.6f);
                                }
                                else
                                {
                                    collider.GetComponent<EnemyScript>().BeAttacked(minionAtkDmg, 0.3f);
                                }
                            }
                        }
                        animator.SetTrigger("Attack");
                        if(isCritical) animator.SetBool("isCritical", true);
                        else animator.SetBool("isCritical", false);
                        curTime = atkCoolTime;
                    }
                    else
                    {
                        curTime -= Time.deltaTime;
                    }
                    
                }
                else
                {
                    animator.SetBool("isMoving", true);
                    moveDirection = target.GetComponent<Collider2D>().bounds.center - GetComponent<Collider2D>().bounds.center;
                    if(target.GetComponent<Collider2D>().bounds.center.x < GetComponent<Collider2D>().bounds.center.x)
                    {
                        if(transform.localScale.x > 0)
                        {
                            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                        }
                    }
                    else
                    {
                        if(transform.localScale.x < 0)
                        {
                            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                        }
                    }
                    moveDirection.Normalize();
                    transform.Translate(moveDirection * Time.deltaTime * minionMoveSpeed);
                }
            }
            else
            {
                animator.SetBool("isMoving", false);
            }
        }
    }

    public Vector2 TrackingBox;
    private void UpdateTarget()
    {
        Collider2D[] tempCols = Physics2D.OverlapBoxAll(GetComponent<Collider2D>().bounds.center, TrackingBox, 0);
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

    public Vector2 boxSize;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetComponent<Collider2D>().bounds.center, boxSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(GetComponent<Collider2D>().bounds.center, TrackingBox);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            EnemyScript colES = collision.gameObject.GetComponent<EnemyScript>();
            minionNowHP -= colES.enemyCollisionDmg;
            // print($"minionNowHP: {minionNowHP}");
            OnDamaged(collision.transform.position);
        }
    }

    void OnDamaged(Vector2 targetPos)
    {
        gameObject.layer = 10;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        Invoke("OffDamaged", 0.3f);
    }

    void OffDamaged()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.layer = 9;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }
}
