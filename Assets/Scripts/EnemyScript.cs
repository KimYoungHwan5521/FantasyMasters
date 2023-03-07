using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour
{
    DataManager DataManager;
    public int _enemyID;
    public string enemyNameKR;
    public float enemyMaxHP;
    public float enemyNowHP;
    public int enemyAtkType;
    public float enemyAtkDmg;
    public float enemyCollisionDmg;
    public float enemyAtkSpeed;
    public float enemyAtkRange;
    public float enemyArmor;
    public float enemyMoveSpeed;

    Animator animator;
    public RectTransform HPBar;

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
        enemyAtkType = int.Parse(enemyInfo.enemyAtkType);
        enemyAtkDmg = float.Parse(enemyInfo.enemyAtkDmg);
        enemyCollisionDmg = float.Parse(enemyInfo.enemyCollisionDmg);
        enemyAtkSpeed = float.Parse(enemyInfo.enemyAtkSpeed);
        enemyAtkRange = float.Parse(enemyInfo.enemyAtkRange);
        if(enemyAtkRange < 2) boxSize = new Vector2(2, 2);
        else boxSize = new Vector2(enemyAtkRange, enemyAtkRange);
        enemyArmor = float.Parse(enemyInfo.enemyArmor);
        enemyMoveSpeed = float.Parse(enemyInfo.enemyMoveSpeed);

        animator = GetComponent<Animator>();
        HPBar = Instantiate(Resources.Load<RectTransform>("UIs/HPBar"), new Vector3(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);

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
            HPBar.GetComponent<Image>().fillAmount = 0;
            animator.SetBool("isMoving", false);
            animator.SetTrigger("Dead");
        }
        else
        {
            HPBar.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y - GetComponent<BoxCollider2D>().size.y * 2, 0));
            HPBar.GetComponent<Image>().fillAmount = enemyNowHP / enemyMaxHP;
            if(target != null)
            {
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
                transform.Translate(moveDirection * Time.deltaTime * enemyMoveSpeed);
                animator.SetBool("isMoving", true);
            }
        }
    }

    private void UpdateTarget()
    {
        Collider2D[] tempCols = Physics2D.OverlapBoxAll(GetComponent<Collider2D>().bounds.center, boxSize, 0);
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
        if(target == null) target = Hero;
    }

    public void BeAttacked(float dmg, float knockback)
    {
        enemyNowHP -= dmg;
        print($"enemyNowHP: {enemyNowHP}");
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 0, 0, 1);
        Vector2 moveDirection = GetComponent<Collider2D>().bounds.center - Hero.GetComponent<Collider2D>().bounds.center;
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
        center = GetComponent<Collider2D>().bounds.center;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, boxSize);
    }
}
