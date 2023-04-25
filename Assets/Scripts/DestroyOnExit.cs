using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnExit : StateMachineBehaviour
{
    SoundManager SoundManager;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager = GameObject.Find("DataManager").GetComponent<SoundManager>();
        GameObject StageTimer = GameObject.Find("StageTimer");
        if(StageTimer != null)
        {
            HeroScript Hero = GameObject.FindWithTag("Player").GetComponent<HeroScript>();
            if(animator.gameObject.tag == "Minion")
            {
                animator.gameObject.layer = 10;

                if(Hero.abilities.Contains("0009"))
                {
                    Instantiate(Resources.Load<GameObject>($"Effects/Explosion00"), animator.GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
                    SoundManager.PlaySE(Resources.Load<AudioClip>("Sounds/SE/acid_burn/acid burn"));
                    Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(animator.GetComponent<Collider2D>().bounds.center, new Vector2(2, 2), 0);
                    bool attacked = false;
                    foreach(Collider2D collider in collider2Ds)
                    {
                        if(collider.tag == "Enemy" && !attacked)
                        {
                            collider.gameObject.GetComponent<EnemyScript>().BeAttacked(animator.GetComponent<MinionScript>().minionMaxHP * 0.3f, 0.3f, false);
                        }
                    }
                    attacked = true;
                    
                }
                if(Hero.abilities.Contains("0042") && animator.GetComponent<MinionScript>()._minionID != 12)
                {
                    Instantiate(Resources.Load<GameObject>($"Minions/Minion0012"), animator.GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
                    Instantiate(Resources.Load<GameObject>($"Minions/Minion0012"), animator.GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
                    Instantiate(Resources.Load<GameObject>($"Minions/Minion0012"), animator.GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
                }
                if(Hero.abilities.Contains("0049"))
                {
                    int r = Random.Range(0,4);
                    if(r == 0) Hero.atkDmgCV += 1;
                    else if(r == 1) Hero.armorCV += 0.5f;
                    else if(r == 2) {Hero.maxHPCV += 5; Hero.BeHealed(5);}
                    else if(r == 4) Hero.atkSpeedCVM += 0.02f;
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

                if(Hero.abilities.Contains("0010"))
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
            StageManager.mapEnemyDeath++;
            StageManager.stageEnemyDeath++;
            GameObject.Destroy(animator.GetComponent<EnemyScript>().HPBar.gameObject);
            GameObject.Destroy(animator.GetComponent<EnemyScript>().StatusBar.gameObject);
        }
        else if(animator.GetComponent<MinionScript>() != null)
        {
            StageManager.mapMinionDeath++;
            StageManager.stageMinionDeath++;
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
