using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnExit : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject StageTimer = GameObject.Find("StageTimer");
        if(StageTimer != null)
        {
            if(animator.gameObject.tag == "Minion")
            {
                animator.gameObject.layer = 10;

                if(GameObject.FindWithTag("Player").GetComponent<HeroScript>().abilities.Contains("0009"))
                {
                    Instantiate(Resources.Load<GameObject>($"Effects/Explosion00"), animator.GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
                    Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(animator.GetComponent<Collider2D>().bounds.center, new Vector2(2, 2), 0);
                    foreach(Collider2D collider in collider2Ds)
                    {
                        if(collider.tag == "Enemy")
                        {
                            collider.gameObject.GetComponent<EnemyScript>().BeAttacked(animator.GetComponent<MinionScript>().minionMaxHP * 0.3f, 0.3f, false);
                        }
                    }
                }
                if(animator.GetComponent<MinionScript>().attackedByZombie)
                {
                    int r = Random.Range(0, 5);
                    if(r == 4) Instantiate(Resources.Load<GameObject>($"Enemies/Enemy0008"), animator.GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
                }
            }
            else if(animator.gameObject.tag == "Enemy")
            {
                animator.gameObject.layer = 8;

                if(GameObject.FindWithTag("Player").GetComponent<HeroScript>().abilities.Contains("0010"))
                {
                    Instantiate(Resources.Load<GameObject>($"Minions/Minion0003"), animator.GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
                    Instantiate(Resources.Load<GameObject>($"Minions/Minion0003"), animator.GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
                }
                if(animator.GetComponent<EnemyScript>().attackedByZombie)
                {
                    int r = Random.Range(0, 5);
                    if(r == 4) Instantiate(Resources.Load<GameObject>($"Minions/Minion0002"), animator.GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
                }
            }
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(animator.GetComponent<EnemyScript>() != null)
        {
            GameObject.Destroy(animator.GetComponent<EnemyScript>().HPBar.gameObject);
            GameObject.Destroy(animator.GetComponent<EnemyScript>().StatusBar.gameObject);
        }
        else if(animator.GetComponent<MinionScript>() != null)
        {
            GameObject.Destroy(animator.GetComponent<MinionScript>().HPBar.gameObject);
            GameObject.Destroy(animator.GetComponent<MinionScript>().StatusBar.gameObject);
        }
        GameObject.Destroy(animator.gameObject);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
