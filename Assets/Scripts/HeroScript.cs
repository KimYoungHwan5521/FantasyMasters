using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroScript : MonoBehaviour
{
    public GameObject Hero;
    Animator animator;
    public GameObject Map;

    public float moveSpeed;
    public float maxHP;
    public float nowHP;
    public float atkDmg;
    public float atkSpeed = 1;
    // public Image nowHPbar;
    public int playerLook = 0;

    Rigidbody2D rigid;

    private float curTime;
    public float coolTime = 0.5f;
    public Transform pos;
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
    }


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
            if(!(playerLook == 1 || playerLook == 3))
            {
                float tempx = boxSize.x;
                float tempy = boxSize.y;
                boxSize = new Vector2(tempy, tempx);
                Vector2 tempDir = Vector2.zero;
                if(playerLook == 0)
                {
                    tempDir = Vector2.up + Vector2.right;
                }
                else if(playerLook == 2)
                {
                    tempDir = Vector2.down + Vector2.right;
                }
                tempDir.Normalize();
                pos.transform.Translate(tempDir * boxSize);
                playerLook = 1;
            }
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
            if(!(playerLook == 1 || playerLook == 3))
            {
                float tempx = boxSize.x;
                float tempy = boxSize.y;
                boxSize = new Vector2(tempy, tempx);
                Vector2 tempDir = Vector2.zero;
                if(playerLook == 0)
                {
                    tempDir = Vector2.up + Vector2.right;
                }
                else if(playerLook == 2)
                {
                    tempDir = Vector2.down + Vector2.right;
                }
                tempDir.Normalize();
                pos.transform.Translate(tempDir * boxSize);
                playerLook = 3;
            }
        }
        else
        {
            animator.SetBool("left", false);
        }
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector2.up;
            animator.SetBool("up", true);
            if(!(playerLook == 2) && !animator.GetBool("left") && !animator.GetBool("right") && !animator.GetBool("down"))
            {
                Vector2 tempDir = Vector2.zero;
                if(playerLook == 1 || playerLook == 3)
                {
                    tempDir = Vector2.left + Vector2.up;
                    tempDir.Normalize();
                    pos.transform.Translate(tempDir * boxSize);
                    float tempx = boxSize.x;
                    float tempy = boxSize.y;
                    boxSize = new Vector2(tempy, tempx);
                }
                else if(playerLook == 0)
                {
                    float tempx = boxSize.x;
                    float tempy = boxSize.y;
                    boxSize = new Vector2(tempy, tempx);
                    tempDir = Vector2.up + Vector2.right;
                    tempDir.Normalize();
                    pos.transform.Translate(tempDir * boxSize);
                    tempDir = Vector2.up + Vector2.left;
                    tempDir.Normalize();
                    pos.transform.Translate(tempDir * boxSize);
                    boxSize = new Vector2(tempx, tempy);
                }
                playerLook = 2;

            }
            animator.SetBool("down", false);
            
        }
        if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector2.down;
            animator.SetBool("down", true);
            if(!(playerLook == 0) && !animator.GetBool("left") && !animator.GetBool("right") && !animator.GetBool("up"))
            {
                Vector2 tempDir = Vector2.zero;
                if(playerLook == 1 || playerLook == 3)
                {
                    tempDir = Vector2.left + Vector2.down;
                    tempDir.Normalize();
                    pos.transform.Translate(tempDir * boxSize);
                    float tempx = boxSize.x;
                    float tempy = boxSize.y;
                    boxSize = new Vector2(tempy, tempx);
                }
                else if(playerLook == 2)
                {
                    float tempx = boxSize.x;
                    float tempy = boxSize.y;
                    boxSize = new Vector2(tempy, tempx);
                    tempDir = Vector2.down + Vector2.left;
                    tempDir.Normalize();
                    pos.transform.Translate(tempDir * boxSize);
                    tempDir = Vector2.down + Vector2.right;
                    tempDir.Normalize();
                    pos.transform.Translate(tempDir * boxSize);
                    boxSize = new Vector2(tempx, tempy);
                }
                playerLook = 0;
            }
            animator.SetBool("up", false);

        }
        print($"Before: {moveDirection}");
        moveDirection.Normalize();
        print($"After: {moveDirection}");


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
            if(Input.GetKey(KeyCode.Z))
            {
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach(Collider2D collider in collider2Ds)
                {
                    if(collider.tag == "Enemy")
                    {
                        collider.GetComponent<EnemyScript>().BeAttacked(atkDmg, 0.3f);
                    }
                }
                animator.SetTrigger("isAttack");
                curTime = coolTime;
            }
        }
        else
        {
            curTime -= Time.deltaTime;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos.position, boxSize);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            OnDamaged(collision.transform.position);//충돌했을때 x축,y축 넘김
        }
    }

    void OnDamaged(Vector2 targetPos)
    {
        //충돌시 플레이어의 레이어가 PlayerDamaged 레이어로 변함 
        gameObject.layer = 9;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);//무적시간일 때 플레이어가 투명하게
        Invoke("OffDamaged", 0.3f);
    }

    void OffDamaged()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.layer = 8;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }


}
