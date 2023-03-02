using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    DataManager DataManager;
    public int _enemyID;
    public string enemyNameKR;
    public float enemyMaxHP;
    public float enemyNowHP;
    public float enemyAtkDmg;
    public float enemyCollisionDmg;
    public float enemyAtkSpeed;
    public float enemyAtkRange;
    public float enemyArmor;
    public float enemyMoveSpeed;

    Animator animator;

    void Start()
    {
        string stringID;
        int cntDigit = 0;
        int copy_enemyID = _enemyID;
        for(int i=0; i < 4; i++)
        {
            if(copy_enemyID / 10 > 0)
            {
                cntDigit++;
                copy_enemyID /= 10;
            }
            else break;
        }
        stringID = "";
        for(int i=0; i < 3 - cntDigit / 10; i++)
        {
            stringID += "0";
        }
        stringID += _enemyID.ToString();

        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        Hero = GameObject.FindWithTag("Player");
        int idx = DataManager.AllEnemyList.FindIndex(x => x.enemyID == stringID);
        Enemy enemyInfo = DataManager.AllEnemyList[idx];
        enemyNameKR = enemyInfo.enemyNameKR;
        enemyMaxHP = float.Parse(enemyInfo.enemyMaxHP);
        enemyNowHP = enemyMaxHP;
        enemyAtkDmg = float.Parse(enemyInfo.enemyAtkDmg);
        enemyCollisionDmg = float.Parse(enemyInfo.enemyCollisionDmg);
        enemyAtkSpeed = float.Parse(enemyInfo.enemyAtkSpeed);
        enemyAtkRange = float.Parse(enemyInfo.enemyAtkRange);
        enemyArmor = float.Parse(enemyInfo.enemyArmor);
        enemyMoveSpeed = float.Parse(enemyInfo.enemyMoveSpeed);

        animator = GetComponent<Animator>();

        // 타겟 추적
        target = Hero;
        InvokeRepeating("UpdateTarget", 0, 0.25f);

    }

    Vector2 moveDirection;
    GameObject Hero;
    GameObject target;
    void Update()
    {
        if(enemyNowHP <= 0)
        {
            animator.SetTrigger("Dead");
        }
        else
        {
            if(target != null)
            {
                moveDirection = target.transform.position - transform.position;
                if(target.transform.position.x < transform.position.x)
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
                transform.Translate(moveDirection * Time.deltaTime * enemyMoveSpeed);
            }
        }
    }

    private void UpdateTarget()
    {
        Collider2D[] tempCols = Physics2D.OverlapBoxAll(transform.position, boxSize, 0);
        int cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Player" || tempCols[i].tag == "Minion") cnt++;
        }
        Collider2D[] cols = new Collider2D[cnt];
        cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Player" || tempCols[i].tag == "Minion") 
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
        if(target == null) target = Hero;
    }

    public void BeAttacked(float dmg, float knockback)
    {
        enemyNowHP -= dmg;
        print($"enemyNowHP: {enemyNowHP}");
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 0, 0, 1);
        Vector2 moveDirection = transform.position - Hero.transform.position;
        moveDirection.Normalize();
        transform.Translate(moveDirection * knockback);
        Invoke("OffDamaged", 0.3f);
    }
    
    void OffDamaged()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }



    Vector2 center;
    public Vector2 boxSize;
    private void OnDrawGizmos()
    {
        center = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, boxSize);
    }
}
