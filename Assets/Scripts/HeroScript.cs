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
    public bool attacked = false;
    // public Image nowHPbar;
    
    void Start()
    {
        moveSpeed = 8;
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
            transform.localScale = new Vector2(1, 1);
            animator.SetBool("right", true);
        }
        else
        {
            animator.SetBool("right", false);
        }
        if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector2.left;
            transform.localScale = new Vector2(-1, 1);
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
}
