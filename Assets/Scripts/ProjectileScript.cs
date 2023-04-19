using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProjectileScript : MonoBehaviour
{
    SummonerInfo summoner;
    GameObject target;
    Vector3 targetInitialPosition;
    public bool isCritical = false;
    public string note = "";
    Vector2 spawnPoint;
    public float projectileSpeed = 10;
    public bool launchReady = false;
    public float range;
    public float abilityDmg;
    public bool ignoreArmor;
    GameObject Hero;

    void Start()
    {
        Hero = GameObject.FindWithTag("Player");
    }

    public class SummonerInfo
    {
        public string tag;
        public float atkDmg;
        public float atkRange;
        public float criticalDmg;
        public List<string> abilities;
    }

    public void SetProjectile(GameObject _summoner, GameObject _target, bool _isCritical = false, string _note = "Basic", float _abilityDmg = -1, bool _ignoreArmor = false)
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
        if(target) targetInitialPosition = _target.GetComponent<Collider2D>().bounds.center;
        isCritical = _isCritical;
        note = _note;
        if(note == "Basic") range = summoner.atkRange;
        else if(note == "Straight") range = 10;
        else if(note == "LifeDrain") range = 100;
        else print("wrong projectile note");
        abilityDmg = _abilityDmg;
        ignoreArmor = _ignoreArmor;
        StartCoroutine(Launch());
    }

    bool isTargetOutOfRange = false;
    IEnumerator Launch()
    {
        launchReady = true;
        float limit = 3;
        if(note == "LifeDrain") limit = 60;
        Vector3 randomVector = new Vector3(Random.Range(-1.0f,1.0f), Random.Range(-1.0f,1.0f), transform.position.z);
        while(limit > 0)
        {
            isTargetOutOfRange = Vector2.Distance(transform.position, spawnPoint) > range;
            if(isTargetOutOfRange)
            {
                Destroy(transform.parent.gameObject);
            }
            Vector2 moveDirection = Vector2.zero;
            if(note == "Straight")
            {
                if(target) moveDirection = targetInitialPosition - transform.position;
                else moveDirection = randomVector;
                
                moveDirection.Normalize();
                transform.parent.transform.Translate(moveDirection * Time.deltaTime * projectileSpeed);
                transform.position = transform.parent.transform.position;

            }
            else
            {
                if(target)
                {
                    float thp = 0;
                    if(target.tag == "Minion") thp = target.GetComponent<MinionScript>().minionNowHP;
                    else if(target.tag == "Enemy") thp = target.GetComponent<EnemyScript>().enemyNowHP;
                    else if(target.tag == "Player") thp = target.GetComponent<HeroScript>().nowHP;
                    if(thp > 0)
                    {
                        float AngleRad = Mathf.Atan2(target.GetComponent<Transform>().position.y - transform.position.y, target.GetComponent<Transform>().position.x - transform.position.x);
                        float AngleDeg = (180 / Mathf.PI) * AngleRad;
                        transform.rotation = Quaternion.Euler(0, 0, AngleDeg);

                        moveDirection = target.GetComponent<Collider2D>().bounds.center - transform.GetComponentInChildren<Collider2D>().bounds.center;
                        moveDirection.Normalize();
                        transform.parent.transform.Translate(moveDirection * Time.deltaTime * projectileSpeed);
                        transform.position = transform.parent.transform.position;
                    }
                    else Destroy(transform.parent.gameObject);
                }
                else
                {
                    Destroy(transform.parent.gameObject);
                }
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
            dmg = summoner.atkDmg;
            if(isCritical) dmg *= summoner.criticalDmg;
            if(summoner.tag == "Player") 
            {
                if(collision.gameObject.tag == "Enemy")
                {
                    if(isCritical) 
                    {
                        if(summoner.abilities.Contains("0003"))
                        {
                            GameObject.FindWithTag("Player").GetComponent<HeroScript>().AddStatus("0000");
                        }
                    }

                    if(note == "Basic")
                    {
                        if(summoner.abilities.Contains("0004")) collision.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg + collision.gameObject.GetComponent<EnemyScript>().enemyArmor, 0.1f + 0.2f * isCf, isCritical);
                        else collision.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg, 0.1f + 0.2f * isCf, isCritical);

                        if(summoner.abilities.Contains("0016")) 
                        {
                            Hero.GetComponent<HeroScript>().BeHealed((dmg - collision.gameObject.GetComponent<EnemyScript>().enemyArmor) / 10);
                        }
                        if(summoner.abilities.Contains("0005"))
                        {
                            collision.gameObject.GetComponent<EnemyScript>().AddStatus("0002");
                        }
                        if(summoner.abilities.Contains("0006"))
                        {
                            collision.gameObject.GetComponent<EnemyScript>().AddStatus("0003");
                        }
                        if(summoner.abilities.Contains("0025") && !collision.gameObject.GetComponent<EnemyScript>().enemyAbilities.Contains("0027"))
                        {
                            collision.gameObject.GetComponent<EnemyScript>().AddStatus("0007");
                        }
                        if(summoner.abilities.Contains("0026") && !collision.gameObject.GetComponent<EnemyScript>().enemyAbilities.Contains("0028"))
                        {
                            collision.gameObject.GetComponent<EnemyScript>().AddStatus("0008");
                        }
                        
                        if(summoner.abilities.Contains("0011")) collision.gameObject.GetComponent<EnemyScript>().attackedByZombie = true;
                    }
                    else
                    {
                        if(ignoreArmor) collision.gameObject.GetComponent<EnemyScript>().BeAttacked(abilityDmg + collision.gameObject.GetComponent<EnemyScript>().enemyArmor, 0.1f, isCritical);
                        else collision.gameObject.GetComponent<EnemyScript>().BeAttacked(abilityDmg, 0.1f, isCritical);
                    }
                    if(summoner.abilities.Contains("0031") && collision.gameObject.GetComponent<EnemyScript>().enemyNowHP <= 0 && Hero.GetComponent<HeroScript>().target != null && note == "Basic")
                    {
                        SetProjectile(Hero, Hero.GetComponent<HeroScript>().target, isCritical, "Basic");
                    }
                    else Destroy(transform.parent.gameObject);
                }
            }
            else if(summoner.tag == "Minion")
            {
                if(collision.gameObject.tag == "Enemy")
                {
                    if(summoner.abilities.Contains("0004"))
                    {
                        collision.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg + GetComponent<Collider>().GetComponent<EnemyScript>().enemyArmor, 0.1f + 0.2f * isCf, isCritical);
                    }
                    else
                    {
                        collision.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg, 0.1f + 0.2f * isCf, isCritical);
                    }
                    
                    if(summoner.abilities.Contains("0005"))
                    {
                        collision.gameObject.GetComponent<EnemyScript>().AddStatus("0002");
                    }
                    if(summoner.abilities.Contains("0006"))
                    {
                        collision.gameObject.GetComponent<EnemyScript>().AddStatus("0003");
                    }
                    if(summoner.abilities.Contains("0025") && !collision.gameObject.GetComponent<EnemyScript>().enemyAbilities.Contains("0027"))
                    {
                        collision.gameObject.GetComponent<EnemyScript>().AddStatus("0007");
                    }
                    if(summoner.abilities.Contains("0026") && !collision.gameObject.GetComponent<EnemyScript>().enemyAbilities.Contains("0028"))
                    {
                        collision.gameObject.GetComponent<EnemyScript>().AddStatus("0008");
                    }

                    if(summoner.abilities.Contains("0011"))
                    {
                        collision.gameObject.GetComponent<EnemyScript>().attackedByZombie = true;
                    }
                    Destroy(transform.parent.gameObject);
                }
            }
            else if(summoner.tag == "Enemy")
            {
                if(collision.gameObject.tag == "Player")
                {
                    if(note == "LifeDrain")
                    {
                        collision.gameObject.GetComponent<HeroScript>().BeHealed(80);
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

                        if(summoner.abilities.Contains("0005"))
                        {
                            collision.gameObject.GetComponent<HeroScript>().AddStatus("0002");
                        }
                        if(summoner.abilities.Contains("0006"))
                        {
                            collision.gameObject.GetComponent<HeroScript>().AddStatus("0003");
                        }
                        if(summoner.abilities.Contains("0025") && !collision.gameObject.GetComponent<HeroScript>().abilities.Contains("0027"))
                        {
                            collision.gameObject.GetComponent<HeroScript>().AddStatus("0007");
                        }
                        if(summoner.abilities.Contains("0026") && !collision.gameObject.GetComponent<HeroScript>().abilities.Contains("0028"))
                        {
                            collision.gameObject.GetComponent<HeroScript>().AddStatus("0008");
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
                    
                    if(summoner.abilities.Contains("0005"))
                    {
                        collision.gameObject.GetComponent<MinionScript>().AddStatus("0002");
                    }
                    if(summoner.abilities.Contains("0006"))
                    {
                        collision.gameObject.GetComponent<MinionScript>().AddStatus("0003");
                    }
                    if(summoner.abilities.Contains("0025") && !collision.gameObject.GetComponent<MinionScript>().minionAbilities.Contains("0027"))
                    {
                        collision.gameObject.GetComponent<MinionScript>().AddStatus("0007");
                    }
                    if(summoner.abilities.Contains("0026") && !collision.gameObject.GetComponent<MinionScript>().minionAbilities.Contains("0028"))
                    {
                        collision.gameObject.GetComponent<MinionScript>().AddStatus("0008");
                    }

                    if(summoner.abilities.Contains("0011"))
                    {
                        collision.gameObject.GetComponent<MinionScript>().attackedByZombie = true;
                    }
                    Destroy(transform.parent.gameObject);

                }
            }
        }
    } 
}
