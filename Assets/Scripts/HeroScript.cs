using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroScript : MonoBehaviour
{
    public GameObject Hero;
    Animator animator;
    public GameObject Map;

    public int moveSpeed;
    public int maxHP;
    public int nowHP;
    public int atkDmg;
    public int atkSpeed = 1;
    // public Image nowHPbar;

    Rigidbody2D rigid;
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
        
    }


    void Update()
    {
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
            animator.SetBool("up", false);
            animator.SetBool("down", true);

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
        //gameObject는 자기자신을 의미
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
