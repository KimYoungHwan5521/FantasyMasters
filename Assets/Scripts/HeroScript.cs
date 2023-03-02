using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroScript : MonoBehaviour
{
    public GameObject Hero;
    Animator animator;
    GameObject Map;

    public float moveSpeed;
    public float maxHP;
    public float nowHP;
    public float atkDmg;
    public float atkSpeed = 1;
    // public Image nowHPbar;
    public int playerLook = 0;

    Rigidbody2D rigid;

    private float curTime;
    public float atkCoolTime = 0.3f;
    public Vector2 boxSize;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        moveSpeed = 6;
        maxHP = 100;
        nowHP = 100;
        atkDmg = 10;
        animator = GetComponent<Animator>();
        transform.position = new Vector2(0, 0);

        Map = GameObject.Find("Map");
        InvokeRepeating("UpdateTarget", 0, 0.25f);
        boxSize = new Vector2(2, 2);
    }

    GameObject target = null;
    void Update()
    {
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
            animator.SetBool("right", true);
        }
        else
        {
            animator.SetBool("right", false);
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
            animator.SetBool("left", true);
        }
        else
        {
            animator.SetBool("left", false);
        }
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector2.up;
            animator.SetBool("up", true);
            animator.SetBool("down", false);
            
        }
        if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector2.down;
            animator.SetBool("down", true);
            animator.SetBool("up", false);

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
                if(Mathf.Abs(targetPos.x) > Mathf.Abs(targetPos.y)) 
                {
                    if(targetPos.x * transform.localScale.x < 0) transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                    if(targetPos.x >0) playerLook = 1;
                    else playerLook = 3;
                    
                    if(playerLook == 1)
                    {
                        animator.SetBool("left", false);
                        animator.SetBool("right", true);
                    }
                    else
                    {
                        animator.SetBool("left", true);
                        animator.SetBool("right", false);
                    }
                }
                else
                {
                    if(targetPos.y > 0)
                    {
                        playerLook = 2;
                        animator.SetBool("up", true);
                        animator.SetBool("down", false);
                    }
                    else
                    {
                        playerLook = 0;
                        animator.SetBool("up", false);
                        animator.SetBool("down", true);
                    }
                }
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(transform.position, boxSize, 0);
                foreach(Collider2D collider in collider2Ds)
                {
                    if(collider.tag == "Enemy")
                    {
                        collider.GetComponent<EnemyScript>().BeAttacked(atkDmg, 0.3f);
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


}
