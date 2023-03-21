using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProjectileScript : MonoBehaviour
{
    SummonerInfo summoner;
    GameObject target;
    public bool isCritical = false;
    public string special = "";
    Vector2 spawnPoint;
    public float projectileSpeed = 10;
    public bool launchReady = false;

    public class SummonerInfo
    {
        public string tag;
        public float atkDmg;
        public float atkRange;
        public float criticalDmg;
        public List<string> abilities;
    }

    public void SetProjectile(GameObject _summoner, GameObject _target, bool _isCritical = false, string _special = "")
    {
        summoner = new SummonerInfo();
        summoner.tag = _summoner.tag;
        if(_summoner.tag == "Player")
        {
            summoner.atkDmg = _summoner.GetComponent<HeroScript>().atkDmg;
            summoner.atkRange = _summoner.GetComponent<HeroScript>().atkRange;
            summoner.criticalDmg = _summoner.GetComponent<HeroScript>().criticalDmg;
            summoner.abilities = _summoner.GetComponent<HeroScript>().abilities.ToList();
        }
        else if(_summoner.tag == "Minion")
        {
            summoner.atkDmg = _summoner.GetComponent<MinionScript>().minionAtkDmg;
            summoner.atkRange = _summoner.GetComponent<MinionScript>().minionAtkRange;
            summoner.criticalDmg = _summoner.GetComponent<MinionScript>().minionCriticalDmg;
            summoner.abilities = _summoner.GetComponent<MinionScript>().minionAbilities.ToList();
        }
        else if(_summoner.tag == "Enemy")
        {
            summoner.atkDmg = _summoner.GetComponent<EnemyScript>().enemyAtkDmg;
            summoner.atkRange = _summoner.GetComponent<EnemyScript>().enemyAtkRange;
            summoner.criticalDmg = 1;
            summoner.abilities = _summoner.GetComponent<EnemyScript>().enemyAbilities.ToList();
        }
        else print("Unknown projectile sumonner");
        spawnPoint = _summoner.transform.position;
        target = _target;
        isCritical = _isCritical;
        special = _special;
        StartCoroutine(Launch());
    }

    bool isTargetOutOfRange = false;
    IEnumerator Launch()
    {
        launchReady = true;
        float limit = 10;
        while(limit > 0)
        {
            isTargetOutOfRange = Vector2.Distance(transform.position, spawnPoint) > summoner.atkRange;
            if(isTargetOutOfRange && special != "LifeDrain")
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(launchReady)
        {
            float dmg = 0;
            float isCf = 0;
            if(isCritical) isCf = 1;
            if(summoner.tag == "Player") 
            {
                if(collision.gameObject.tag == "Enemy")
                {
                    dmg = summoner.atkDmg;
                    if(isCritical) 
                    {
                        dmg *= summoner.criticalDmg;

                        if(summoner.abilities.Contains("0003"))
                        {
                            GameObject.FindWithTag("Player").GetComponent<HeroScript>().AddStatus("0000");
                        }
                    }

                    if(summoner.abilities.Contains("0004"))
                    {
                        collision.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg + GetComponent<Collider>().GetComponent<EnemyScript>().enemyArmor, 0.1f + 0.2f * isCf, isCritical);
                    }
                    else
                    {
                        collision.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg, 0.1f + 0.2f * isCf, isCritical);
                    }
                    
                }
                Destroy(transform.parent.gameObject);
            }
            else if(summoner.tag == "Minion")
            {
                if(collision.gameObject.tag == "Enemy")
                {
                    dmg = summoner.atkDmg;
                    if(isCritical) 
                    {
                        dmg *= summoner.criticalDmg;
                        // if(summoner.GetComponent<MinionScript>().minionAbilities.Contains("0003"))
                        // {
                        //     summoner.GetComponent<MinionScript>().AddStatus("0000");
                        // }
                    }

                    if(summoner.abilities.Contains("0004"))
                    {
                        GetComponent<Collider>().GetComponent<EnemyScript>().BeAttacked(dmg + GetComponent<Collider>().GetComponent<EnemyScript>().enemyArmor, 0.1f + 0.2f * isCf, isCritical);
                    }
                    else
                    {
                        GetComponent<Collider>().GetComponent<EnemyScript>().BeAttacked(dmg, 0.1f + 0.2f * isCf, isCritical);
                    }
                    Destroy(transform.parent.gameObject);
                }
            }
            else if(summoner.tag == "Enemy")
            {
                dmg = summoner.atkDmg;
                if(collision.gameObject.tag == "Player")
                {
                    if(special == "LifeDrain")
                    {
                        collision.gameObject.GetComponent<HeroScript>().nowHP += 30;
                        RectTransform LDtext = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
                        LDtext.GetComponent<FloatingText>().SetText("+30", "#00FF00");
                        LDtext.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
                    }
                    else
                    {
                        if(summoner.abilities.Contains("0004"))
                        {
                            collision.gameObject.GetComponent<HeroScript>().BeAttacked(dmg + GetComponent<Collider>().GetComponent<HeroScript>().armor);
                        }
                        else
                        {
                            collision.gameObject.GetComponent<HeroScript>().BeAttacked(dmg);
                        }

                        if(summoner.abilities.Contains("0001"))
                        {
                            collision.gameObject.GetComponent<HeroScript>().AddStatus("0001");
                        }
                    }
                    Destroy(transform.parent.gameObject);
                }
                else if(collision.gameObject.tag == "Minion")
                {
                    if(special == "LifeDrain")
                    {
                        
                    }
                    else
                    {
                        if(summoner.abilities.Contains("0004"))
                        {
                            collision.gameObject.GetComponent<MinionScript>().BeAttacked(dmg + GetComponent<Collider>().GetComponent<HeroScript>().armor);
                        }
                        else
                        {
                            collision.gameObject.GetComponent<MinionScript>().BeAttacked(dmg);
                        }
                        
                        if(summoner.abilities.Contains("0001"))
                        {
                            collision.gameObject.GetComponent<MinionScript>().AddStatus("0001");
                        }
                        Destroy(transform.parent.gameObject);
                    }
                }
            }
        }
    } 
}
