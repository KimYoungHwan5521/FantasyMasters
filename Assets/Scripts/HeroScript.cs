using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroScript : MonoBehaviour
{
    public GameObject Hero;
    Animator animator;

    public int moveSpeed;
    public int maxHP;
    public int nowHP;
    public int atkDmg;
    public int atkSpeed = 1;
    public bool attacked = false;
    // public Image nowHPbar;
    
    void Start()
    {
        moveSpeed = 300;
        maxHP = 100;
        nowHP = 100;
        atkDmg = 10;
        animator = GetComponent<Animator>();
        
    }


    void Update()
    {
        Vector2 moveDirection = Vector2.zero;

        if(Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection += Vector2.right;
            transform.localScale = new Vector2(1, 1);
            animator.SetBool("right", true);
            animator.SetBool("left", false);
        }
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection += Vector2.left;
            transform.localScale = new Vector2(-1, 1);
            animator.SetBool("right", false);
            animator.SetBool("left", true);
        }
        if(Input.GetKey(KeyCode.UpArrow))
        {
            moveDirection += Vector2.up;
            animator.SetBool("up", true);
            animator.SetBool("down", false);
            
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            moveDirection += Vector2.down;
            animator.SetBool("up", false);
            animator.SetBool("down", true);

        }
        moveDirection.Normalize();

        transform.Translate(moveDirection * Time.deltaTime * moveSpeed);

        animator.SetBool("isMoving", moveDirection.magnitude > 0);
        
        
    }
}
