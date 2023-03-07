using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    GameObject Player;
    Vector2 spawnPoint;
    GameObject target;
    public float projectileSpeed = 10;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        spawnPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector2.Distance(transform.position, spawnPoint) > Player.GetComponent<HeroScript>().atkRange)
        {
            Destroy(transform.parent.gameObject);
        }
        target = Player.GetComponent<HeroScript>().target;
        if(target)
        {
            float AngleRad = Mathf.Atan2(target.GetComponent<Transform>().position.y - transform.position.y, target.GetComponent<Transform>().position.x - transform.position.x);
            float AngleDeg = (180 / Mathf.PI) * AngleRad;
            transform.rotation = Quaternion.Euler(0, 0, AngleDeg);

            Vector2 moveDirection = Vector2.zero;
            moveDirection = target.transform.position - transform.position;
            moveDirection.Normalize();
            transform.parent.transform.Translate(moveDirection * Time.deltaTime * projectileSpeed);
            transform.position = transform.parent.transform.position;
        }
        else
        {
            Destroy(transform.parent.gameObject);
        }

    }

    bool isCritical = false;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            if(Random.Range(0, 100) < Player.GetComponent<HeroScript>().criticalChance) isCritical = true;
            else isCritical = false;
            if(isCritical)
            {
                collision.gameObject.GetComponent<EnemyScript>().BeAttacked(Player.GetComponent<HeroScript>().atkDmg * Player.GetComponent<HeroScript>().criticalDmg, 0.3f);
                // if(Player.GetComponent<HeroScript>().abilities.Find(x => x == "0003") != null)
                // {
                    
                // }
            }
            else
            {
                collision.gameObject.GetComponent<EnemyScript>().BeAttacked(Player.GetComponent<HeroScript>().atkDmg, 0.1f);
            }
            Destroy(transform.parent.gameObject);
        }
    } 
}
