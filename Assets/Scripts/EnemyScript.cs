using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EnemyScript : MonoBehaviour
{
    DataManager DataManager;
    SoundManager SoundManager;
    public int _enemyID;
    public string stringID;
    public string enemyNameKR;
    public float enemyMaxHP;
    public float enemyNowHP;
    public int enemyAtkType;
    public float enemyAtkDmg;
    public float atkDmgCV = 0;
    public float atkDmgCVM = 1;
    public float enemyCollisionDmg;
    public float enemyAtkSpeed;
    public float atkSpeedCVM = 1;
    public float enemyAtkRange;
    public float atkRangeCV = 0;
    public float enemyArmor;
    public float armorCV = 0;
    public int enemyMoveType = 1;
    public float enemyMoveSpeed;
    public float moveSpeedCVM = 1;
    public List<string> enemyAbilities;
    public List<StatusV> EnemyStatus;
    public string atkSound;

    Animator animator;
    public float animatorCV = 1;
    public RectTransform HPBar;
    public RectTransform StatusBar;

    private float curTime;
    public float atkCoolTime;
    private int projectileCount = 0;

    public class StatusV
    {
        public string statusID;
        public string statusNameKR;
        public string statusExplainKR;
        public string[] buffStat;
        public float[] buffValue;
        public float buffTime;
    }

    void Start()
    {
        int cntDigit = 0;
        int copy_enemyID = _enemyID;
        for(int i=0; i < 4; i++)
        {
            if(copy_enemyID / 10 > 0)
            {
                cntDigit++;
                copy_enemyID /= 10;
            }
            else break;
        }
        stringID = "";
        for(int i=0; i < 3 - cntDigit; i++)
        {
            stringID += "0";
        }
        stringID += _enemyID.ToString();

        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        SoundManager = GameObject.Find("DataManager").GetComponent<SoundManager>();
        Hero = GameObject.FindWithTag("Player");
        int idx = DataManager.AllEnemyList.FindIndex(x => x.enemyID == stringID);
        Enemy enemyInfo = DataManager.AllEnemyList[idx];
        enemyNameKR = enemyInfo.enemyNameKR;
        enemyMaxHP = float.Parse(enemyInfo.enemyMaxHP);
        enemyNowHP = enemyMaxHP;
        enemyAtkType = int.Parse(enemyInfo.enemyAtkType);
        enemyAtkDmg = float.Parse(enemyInfo.enemyAtkDmg);
        enemyCollisionDmg = float.Parse(enemyInfo.enemyCollisionDmg);
        enemyAtkSpeed = float.Parse(enemyInfo.enemyAtkSpeed);
        atkCoolTime = 10 / enemyAtkSpeed;
        enemyAtkRange = float.Parse(enemyInfo.enemyAtkRange);
        TrackingBox = new Vector2(2, 2);
        enemyArmor = float.Parse(enemyInfo.enemyArmor);
        enemyMoveType = int.Parse(enemyInfo.enemyMoveType);
        enemyMoveSpeed = float.Parse(enemyInfo.enemyMoveSpeed);
        enemyAbilities = enemyInfo.enemyAbilities.ToList();
        EnemyStatus = new List<StatusV>();

        animator = GetComponent<Animator>();
        HPBar = Instantiate(Resources.Load<RectTransform>("UIs/HPBar"), new Vector3(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);
        HPBar.localScale = new Vector2(GetComponent<BoxCollider2D>().size.x * 5 * transform.localScale.x, 1);
        StatusBar = Instantiate(Resources.Load<RectTransform>("UIs/StatusBar"), new Vector3(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);

        // 타겟 추적
        target = Hero;
        InvokeRepeating("UpdateTarget", 0, 0.25f);

    }

    Vector2 moveDirection;
    GameObject Hero;
    GameObject target;
    public bool attackedByZombie = false;
    public bool movable = true;
    public bool attackable = true;

    public bool fired = false;
    void Update()
    {
        enemyAtkSpeed = float.Parse(DataManager.AllEnemyList[_enemyID].enemyAtkSpeed) * atkSpeedCVM;
        animator.SetFloat("AttackSpeed", atkSpeedCVM);
        if(enemyAtkSpeed > 0) atkCoolTime = 10 / enemyAtkSpeed;
        else atkCoolTime = 100;
        enemyMoveSpeed = float.Parse(DataManager.AllEnemyList[_enemyID].enemyMoveSpeed) * moveSpeedCVM;
        animator.SetFloat("MoveSpeed", moveSpeedCVM);
        enemyAtkDmg = (float.Parse(DataManager.AllEnemyList[_enemyID].enemyAtkDmg.Split('x')[0]) + atkDmgCV) * atkDmgCVM;
        enemyArmor = float.Parse(DataManager.AllEnemyList[_enemyID].enemyArmor) + armorCV;
        enemyAtkRange = float.Parse(DataManager.AllEnemyList[_enemyID].enemyAtkRange) + atkRangeCV;
        
        bool fear = false;
        fired = false;
        // status timer
        if(EnemyStatus.Count > 0)
        {
            for(int i=0; i<EnemyStatus.Count; i++)
            {
                if(EnemyStatus[i].statusID == "0004") fear = true;
                else if(EnemyStatus[i].statusID == "0008") fired = true;
                EnemyStatus[i].buffTime -= Time.deltaTime;
                if(EnemyStatus[i].buffTime <= 0)
                {
                    RemoveStatus(EnemyStatus[i].statusID);
                    EnemyStatus.RemoveAt(i);
                    Destroy(StatusBar.transform.GetChild(i).gameObject);
                }
            }
        }

        if(enemyNowHP <= 0)
        {
            HPBar.GetComponent<Image>().fillAmount = 0;
            StatusBar.gameObject.SetActive(false);
            gameObject.layer = 8;
            animator.SetBool("isMoving", false);
            animator.SetTrigger("Dead");
        }
        else
        {
            HPBar.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y - GetComponent<BoxCollider2D>().size.y * transform.localScale.y - 0.1f, 0));
            HPBar.GetComponent<Image>().fillAmount = enemyNowHP / enemyMaxHP;
            StatusBar.position = Camera.main.WorldToScreenPoint(new Vector3(transform.GetComponent<Collider2D>().bounds.center.x, transform.GetComponent<Collider2D>().bounds.center.y + GetComponent<BoxCollider2D>().size.y * transform.localScale.y + 0.1f, 0));
            if(target != null)
            {
                if(Vector2.Distance(transform.GetComponent<Collider2D>().bounds.center, target.GetComponent<Collider2D>().bounds.center) * Mathf.Abs(transform.localScale.x) < enemyAtkRange)
                {
                    if(curTime <= 0 && attackable)
                    {
                        if(enemyAtkType == 2) projectileCount++;
                        animator.SetTrigger("Attack");
                        curTime = atkCoolTime;
                    }
                    else
                    {
                        if(enemyMoveType == 3 && movable)
                        {
                            animator.SetBool("isMoving", true);
                            moveDirection = GetComponent<Collider2D>().bounds.center - target.GetComponent<Collider2D>().bounds.center;
                            if(target.GetComponent<Collider2D>().bounds.center.x < GetComponent<Collider2D>().bounds.center.x)
                            {
                                if(transform.localScale.x > 0)
                                {
                                    transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                                }
                            }
                            else
                            {
                                if(transform.localScale.x < 0)
                                {
                                    transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                                }
                            }
                            transform.localScale = new Vector2(-transform.localScale.x * animatorCV, transform.localScale.y);
                            moveDirection.Normalize();
                            transform.Translate(moveDirection * Time.deltaTime * enemyMoveSpeed);

                        }
                        else
                        {
                            animator.SetBool("isMoving", false);
                        }
                        curTime -= Time.deltaTime;
                    }
                }
                else
                {
                    if(movable)
                    {
                        animator.SetBool("isMoving", true);
                        if(fear || enemyMoveType == 2) moveDirection = GetComponent<Collider2D>().bounds.center - target.GetComponent<Collider2D>().bounds.center;
                        else if(enemyMoveType == 1 || enemyMoveType == 3) moveDirection = target.GetComponent<Collider2D>().bounds.center - GetComponent<Collider2D>().bounds.center;
                        if(target.GetComponent<Collider2D>().bounds.center.x < GetComponent<Collider2D>().bounds.center.x)
                        {
                            if(transform.localScale.x > 0)
                            {
                                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                            }
                        }
                        else
                        {
                            if(transform.localScale.x < 0)
                            {
                                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                            }
                        }
                        if(fear) transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                        transform.localScale = new Vector2(transform.localScale.x * animatorCV, transform.localScale.y);
                        moveDirection.Normalize();
                        transform.Translate(moveDirection * Time.deltaTime * enemyMoveSpeed);
                    }
                    else animator.SetBool("isMoving", false);
                }
            }
            else
            {
                animator.SetBool("isMoving", false);
            }
        }
    }

    private void UpdateTarget()
    {
        Collider2D[] tempCols = Physics2D.OverlapBoxAll(GetComponent<Collider2D>().bounds.center, TrackingBox, 0);
        int cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Player" || tempCols[i].tag == "Minion") cnt++;
        }
        Collider2D[] cols = new Collider2D[cnt];
        cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Player" || tempCols[i].tag == "Minion") 
            {
                cols[cnt] = tempCols[i];
                cnt++;
            }
        }
        if(cols.Length > 0)
        {
            float minDistance = Vector2.Distance(GetComponent<Collider2D>().bounds.center, cols[0].GetComponent<Collider2D>().bounds.center);
            int minDisIdx = 0;
            for(int i=0; i < cols.Length; i++)
            {
                if(Vector2.Distance(GetComponent<Collider2D>().bounds.center, cols[i].GetComponent<Collider2D>().bounds.center) < minDistance)
                {
                    minDistance = Vector2.Distance(GetComponent<Collider2D>().bounds.center, cols[i].GetComponent<Collider2D>().bounds.center);
                    minDisIdx = i;
                }
            }
            target = cols[minDisIdx].gameObject;
        }
        if(target == null) target = Hero;
    }

    private void MeleeAttack()
    {
        if(enemyAbilities.Contains("0044")) BeHealed(20);
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(GetComponent<Collider2D>().bounds.center, enemyAtkRange * 0.5f);
        foreach(Collider2D collider in collider2Ds)
        {   
            if(collider.tag == "Player" || collider.tag == "Minion")
            {
                AudioClip s = null;
                if(atkSound =="Blow") 
                {
                    s = Resources.Load<AudioClip>("Sounds/SE/Hit");
                }
                else if(atkSound == "Sword")
                {
                    s = Resources.Load<AudioClip>("Sounds/SE/Socapex - Evol Online SFX - Punches and hits/Socapex - Swordsmall");
                }
                else if(atkSound == "Axe")
                {
                    s = Resources.Load<AudioClip>("Sounds/SE/Socapex - Evol Online SFX - Punches and hits/Socapex - big punch");
                }
                else if(atkSound == "BiteSmall")
                {
                    s = Resources.Load<AudioClip>("Sounds/SE/80-CC0-creature-SFX/eat_04");
                }
                else if(atkSound == "Bite")
                {
                    s = Resources.Load<AudioClip>("Sounds/SE/80-CC0-creature-SFX/eat_01");
                }
                else if(atkSound == "Roar")
                {
                    s = Resources.Load<AudioClip>("Sounds/SE/80-CC0-creature-SFX/roar_03");
                }
                if(s != null) SoundManager.PlaySE(s);

            }
            if(collider.tag == "Player")
            {
                if(enemyAbilities.Contains("0004"))
                {
                    collider.GetComponent<HeroScript>().BeAttacked(enemyAtkDmg + collider.GetComponent<HeroScript>().armor);
                }
                else
                {
                    collider.GetComponent<HeroScript>().BeAttacked(enemyAtkDmg);
                }

                if(enemyAbilities.Contains("0005"))
                {
                    collider.gameObject.GetComponent<HeroScript>().AddStatus("0002");
                }
                if(enemyAbilities.Contains("0006"))
                {
                    collider.gameObject.GetComponent<HeroScript>().AddStatus("0003");
                }
                if(enemyAbilities.Contains("0025"))
                {
                    collider.gameObject.GetComponent<HeroScript>().AddStatus("0007");
                }
                if(enemyAbilities.Contains("0026"))
                {
                    collider.gameObject.GetComponent<HeroScript>().AddStatus("0008");
                }
            }
            else if(collider.tag == "Minion")
            {
                if(enemyAbilities.Contains("0004"))
                {
                    collider.GetComponent<MinionScript>().BeAttacked(enemyAtkDmg + collider.GetComponent<MinionScript>().minionArmor);
                }
                else
                {
                    collider.GetComponent<MinionScript>().BeAttacked(enemyAtkDmg);
                }

                if(enemyAbilities.Contains("0005"))
                {
                    collider.gameObject.GetComponent<MinionScript>().AddStatus("0002");
                }
                if(enemyAbilities.Contains("0006"))
                {
                    collider.gameObject.GetComponent<MinionScript>().AddStatus("0003");
                }
                if(enemyAbilities.Contains("0025") && !collider.gameObject.GetComponent<MinionScript>().minionAbilities.Contains("0027"))
                {
                    collider.gameObject.GetComponent<MinionScript>().AddStatus("0007");
                }
                if(enemyAbilities.Contains("0026") && !collider.gameObject.GetComponent<MinionScript>().minionAbilities.Contains("0028"))
                {
                    collider.gameObject.GetComponent<MinionScript>().AddStatus("0008");
                }

                if(enemyAbilities.Contains("0011"))
                {
                    collider.GetComponent<MinionScript>().attackedByZombie = true;
                }
            }
        }
    }
    
    private void RangedAttack()
    {
        if(projectileCount > 0)
        {
            AudioClip s = null;
            if(atkSound =="Spit") 
            {
                s = Resources.Load<AudioClip>("Sounds/SE/80-CC0-creature-SFX/spit_01");
            }
            else if(atkSound == "Bow")
            {
                s = Resources.Load<AudioClip>("Sounds/SE/swishes/swish-13");
            }
            if(s != null) SoundManager.PlaySE(s);

            GameObject p = Instantiate(Resources.Load<GameObject>($"Projectiles/ProjectileEnemy{stringID}"), GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
            p.GetComponentInChildren<ProjectileScript>().SetProjectile(gameObject, target);
            if(enemyAbilities.Contains("0044")) BeHealed(20);
            projectileCount--;
        }
    }

    public void BeAttacked(float dmg, float knockback, bool getCritical = false)
    {
        if(dmg - enemyArmor < 1) dmg = 1;
        else dmg = dmg - enemyArmor;
        enemyNowHP -= dmg;
        RectTransform DmgText = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
        DmgText.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
        if(getCritical) DmgText.gameObject.GetComponent<FloatingText>().SetText($"<size=40>{Mathf.Round(dmg).ToString()}</size>", "#FF0000");
        else DmgText.gameObject.GetComponent<FloatingText>().SetText(Mathf.Round(dmg).ToString(), "#FFAAAA");
        // print($"enemyNowHP: {enemyNowHP}");
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 0, 0, 1);
        if(Hero != null)
        {
            Vector2 moveDirection = GetComponent<Collider2D>().bounds.center - Hero.GetComponent<Collider2D>().bounds.center;
            moveDirection.Normalize();
            if(!enemyAbilities.Contains("0007")) transform.Translate(moveDirection * knockback);
        }
        Invoke("OffDamaged", 0.3f);
    }
    
    void OffDamaged()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void BeHealed(float value)
    {
        if(fired) value /= 2;
        if(value > 0 && enemyNowHP < enemyMaxHP) 
        {
            enemyNowHP += value;
            if(enemyNowHP > enemyMaxHP) enemyNowHP = enemyMaxHP;
            RectTransform text = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
            text.GetComponent<FloatingText>().SetText($"+{Mathf.Round(value)}", "#00FF00");
            text.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
        }
    }

    public Vector2 TrackingBox;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetComponent<Collider2D>().bounds.center, enemyAtkRange * 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(GetComponent<Collider2D>().bounds.center, new Vector2(enemyAtkRange, enemyAtkRange));
    }
    
    public void AddStatus(string _statusID)
    {
        Status _status = DataManager.AllStatusList.Find(x => x.statusID == _statusID);
        int isAlreadyGotIt = EnemyStatus.FindIndex(x => x.statusID == _statusID);
        if(isAlreadyGotIt != -1)
        {
            EnemyStatus[isAlreadyGotIt].buffTime = float.Parse(_status.buffTime);
        }
        else if(!(_status.statusID == "0004" && enemyAbilities.Contains("0017")) || !(_status.statusID == "0007" && enemyAbilities.Contains("0027") || !(_status.statusID == "0008" && enemyAbilities.Contains("0028"))))
        {
            StatusV tempStatus = new StatusV(); 
            tempStatus.statusID = _status.statusID;
            tempStatus.statusNameKR = _status.statusNameKR;
            tempStatus.statusExplainKR = _status.statusExplainKR;
            tempStatus.buffStat = new string[_status.buffStat.Length];
            Array.Copy(_status.buffStat, tempStatus.buffStat, _status.buffStat.Length);
            tempStatus.buffValue = Array.ConvertAll(_status.buffValue, x => float.Parse(x));

            for(int i=0; i<_status.buffStat.Length; i++)
            {
                if(_status.buffStat[i] == "atkSpeedCVM")
                {
                    atkSpeedCVM += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "moveSpeedCVM")
                {
                    moveSpeedCVM += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "atkDmgCV")
                {
                    atkDmgCV += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "atkDmgCVM")
                {
                    atkDmgCVM += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "armorCV")
                {
                    armorCV += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "atkRangeCV")
                {
                    atkRangeCV += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "attackable")
                {
                    if(float.Parse(_status.buffValue[i]) == -1)
                    {
                        attackable = false;
                    }
                    else attackable = true;
                }
                else if(_status.buffStat[i] == "movable")
                {
                    if(float.Parse(_status.buffValue[i]) == -1)
                    {
                        movable = false;
                    }
                    else movable = true;
                }
                else if(_status.buffStat[i] == "nowHPCV")
                {
                    StartCoroutine(ContinuousDmg(float.Parse(_status.buffValue[i]), float.Parse(_status.buffTime)));
                }
                else
                {
                    print($"wrong buffStat name : '{_status.buffStat[i]}'");
                }
            }
            Instantiate(Resources.Load($"UIs/Icons/Status/Status{_statusID}"), new Vector2(0, 0), Quaternion.identity, StatusBar.transform);
            tempStatus.buffTime = float.Parse(_status.buffTime);
            EnemyStatus.Add(tempStatus);
        }
    }
    
    public void RemoveStatus(string _statusID)
    {
        int idx = EnemyStatus.FindIndex(x => x.statusID == _statusID);
        for(int i=0; i<EnemyStatus[idx].buffStat.Length; i++)
        {
            if(EnemyStatus[idx].buffStat[i] == "atkSpeedCVM")
            {
                atkSpeedCVM -= EnemyStatus[idx].buffValue[i];
            }
            else if(EnemyStatus[idx].buffStat[i] == "moveSpeedCVM")
            {
                moveSpeedCVM -= EnemyStatus[idx].buffValue[i];
            }
            else if(EnemyStatus[idx].buffStat[i] == "atkDmgCV")
            {
                atkDmgCV -= EnemyStatus[idx].buffValue[i];
            }
            else if(EnemyStatus[idx].buffStat[i] == "atkDmgCVM")
            {
                atkDmgCVM -= EnemyStatus[idx].buffValue[i];
            }
            else if(EnemyStatus[idx].buffStat[i] == "armorCV")
            {
                armorCV -= EnemyStatus[idx].buffValue[i];
            }
            else if(EnemyStatus[idx].buffStat[i] == "atkRangeCV")
            {
                atkRangeCV -= EnemyStatus[idx].buffValue[i];
            }
            else if(EnemyStatus[idx].buffStat[i] == "attackable")
            {
                if(EnemyStatus[idx].buffValue[i] == -1)
                {
                    attackable = true;
                }
                else attackable = false;
            }
            else if(EnemyStatus[idx].buffStat[i] == "movable")
            {
                if(EnemyStatus[idx].buffValue[i] == -1)
                {
                    movable = true;
                }
                else movable = false;
            }
            else if(EnemyStatus[idx].buffStat[i] == "nowHPCV")
            {
                StopCoroutine("ContinuousDmg");
            }
        }
    }

    IEnumerator ContinuousDmg(float dmg, float time)
    {
        int t = (int)time;
        for(int i=0; i<t; i++)
        {
            enemyNowHP += dmg;
            RectTransform DmgText = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
            DmgText.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
            DmgText.gameObject.GetComponent<FloatingText>().SetText($"{dmg}", "#FF0000");
            yield return new WaitForSeconds(1);
        }
    }
}
