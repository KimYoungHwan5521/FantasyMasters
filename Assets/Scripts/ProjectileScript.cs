using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public GameObject summoner;
    GameObject target;
    Vector2 spawnPoint;
    public float projectileSpeed = 10;

    public void SetProjectile(GameObject _summoner, GameObject _target, bool _isCritical = false)
    {
        summoner = _summoner;
        spawnPoint = summoner.transform.position;
        target = _target;
        isCritical = _isCritical;
        print($"sum, tar: {summoner.tag}, {target.tag}");
        StartCoroutine(Launch());
    }

    bool isTargetOutOfRange = false;
    IEnumerator Launch()
    {
        float limit = 3;
        while(limit > 0)
        {
            if(summoner.tag == "Player") isTargetOutOfRange = Vector2.Distance(transform.position, spawnPoint) > summoner.GetComponent<HeroScript>().atkRange;
            else if(summoner.tag == "Minion") isTargetOutOfRange = Vector2.Distance(transform.position, spawnPoint) > summoner.GetComponent<MinionScript>().minionAtkRange;
            else if(summoner.tag == "Enemy") isTargetOutOfRange = Vector2.Distance(transform.position, spawnPoint) > summoner.GetComponent<EnemyScript>().enemyAtkRange;
            else print("Unknown projectile sumonner");
            if(isTargetOutOfRange)
            {
                Destroy(transform.parent.gameObject);
            }
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
            limit -= Time.deltaTime;
            yield return null;
        }
        Destroy(transform.parent.gameObject);
    }

    public bool isCritical = false;
    void OnCollisionEnter2D(Collision2D collision)
    {
        float dmg = 0;
        float isCf;
        if(isCritical) isCf = 1;
        else isCf = 0;
        if(summoner.tag == "Player") 
        {
            if(collision.gameObject.tag == "Enemy")
            {
                dmg = summoner.GetComponent<HeroScript>().atkDmg;
                if(isCritical) 
                {
                    dmg *= summoner.GetComponent<HeroScript>().criticalDmg;
                    if(summoner.GetComponent<HeroScript>().abilities.Contains("0003"))
                    {
                        summoner.GetComponent<HeroScript>().AddStatus("0000");
                    }
                }
                collision.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg, 0.1f + 0.2f * isCf, isCritical);
            }
        }
        else if(summoner.tag == "Minion")
        {
            if(collision.gameObject.tag == "Enemy")
            {
                dmg = summoner.GetComponent<MinionScript>().minionAtkDmg;
                if(isCritical) 
                {
                    dmg *= summoner.GetComponent<MinionScript>().minionCriticalDmg;
                    // if(summoner.GetComponent<MinionScript>().minionAbilities.Find(x => x == "0003") != null)
                    // {
                    //     summoner.GetComponent<MinionScript>().AddStatus("0000");
                    // }
                }
                collision.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg, 0.1f + 0.2f * isCf, isCritical);
            }
        }
        else if(summoner.tag == "Enemy")
        {
            dmg = summoner.GetComponent<EnemyScript>().enemyAtkDmg;
            if(collision.gameObject.tag == "Player")
            {
                collision.gameObject.GetComponent<HeroScript>().BeAttacked(dmg);
                if(summoner.GetComponent<EnemyScript>().enemyAbilities.Contains("0001"))
                {
                    collision.gameObject.GetComponent<HeroScript>().AddStatus("0001");
                }
            }
            else if(collision.gameObject.tag == "Minion")
            {
                collision.gameObject.GetComponent<MinionScript>().BeAttacked(dmg);
            }
        }
        Destroy(transform.parent.gameObject);
    } 
}
