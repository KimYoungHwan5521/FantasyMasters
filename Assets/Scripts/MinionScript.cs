using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MinionScript : MonoBehaviour
{
    DataManager DataManager;
    public int _minionID;
    public string stringID;
    public string minionNameKR;
    public string[] minionAttributes;
    public float minionMaxHP;
    public float minionNowHP;
    public int minionAtkType;
    public float minionAtkDmg;
    public float atkDmgCV = 0;
    public float minionAtkSpeed;
    public float atkSpeedCVM = 1;
    public float minionAtkRange;
    public float atkRangeCV = 0;
    public float minionCriticalDmg;
    public float criticalDmgCV = 0;
    public float minionCriticalChance;
    public float criticalChanceCV = 0;
    public float minionArmor;
    public float armorCV = 0;
    public float minionMoveSpeed;
    public float moveSpeedCVM = 1;
    public List<string> minionAbilities;
    public List<StatusV> MinionStatus;

    Animator animator;
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
        int cntDgit = 0;
        int copy_minionID = _minionID;
        for(int i=0; i<4; i++)
        {
            if(copy_minionID / 10 > 0)
            {
                cntDgit++;
                copy_minionID /= 10;
            }
            else break;
        }
        stringID = "";
        for(int i=0;i<3 - cntDgit; i++)
        {
            stringID += "0";
        }
        stringID += _minionID.ToString();
        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        int idx = DataManager.AllMinionList.FindIndex(x => x.minionID == stringID);
        Minion minionInfo = DataManager.AllMinionList[idx];
        minionNameKR = minionInfo.minionNameKR;
        minionAttributes = minionInfo.minionAttributes;
        minionMaxHP = float.Parse(minionInfo.minionMaxHP);
        minionNowHP = minionMaxHP;
        minionAtkType = int.Parse(minionInfo.minionAtkType);
        minionAtkDmg = float.Parse(minionInfo.minionAtkDmg);
        minionAtkSpeed = float.Parse(minionInfo.minionAtkSpeed);
        atkCoolTime = 10 / minionAtkSpeed;
        minionAtkRange = float.Parse(minionInfo.minionAtkRange);
        boxSize = new Vector2(minionAtkRange, minionAtkRange);
        minionCriticalDmg = float.Parse(minionInfo.minionCriticalDmg);
        minionCriticalChance = float.Parse(minionInfo.minionCriticalChance);
        minionArmor = float.Parse(minionInfo.minionArmor);
        minionMoveSpeed = float.Parse(minionInfo.minionMoveSpeed);
        minionAbilities = minionInfo.minionAbilities.ToList();
        MinionStatus = new List<StatusV>();
        TrackingBox = new Vector2(10, 10);

        animator = GetComponent<Animator>();
        HPBar = Instantiate(Resources.Load<RectTransform>("UIs/HPBar"), new Vector3(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);
        HPBar.localScale = new Vector2(GetComponent<BoxCollider2D>().size.x * 10, 1);
        StatusBar = Instantiate(Resources.Load<RectTransform>("UIs/StatusBar"), new Vector3(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);

        target = null;
        InvokeRepeating("UpdateTarget", 0, 0.25f);

    }

    Vector2 moveDirection;
    GameObject target;
    bool isCritical = false;
    public bool attackedByZombie = false;
    void Update()
    {
        minionAtkSpeed = float.Parse(DataManager.AllMinionList[_minionID].minionAtkSpeed) * atkSpeedCVM;
        animator.SetFloat("AttackSpeed", atkSpeedCVM);
        atkCoolTime = 10 / minionAtkSpeed;
        minionMoveSpeed = float.Parse(DataManager.AllMinionList[_minionID].minionMoveSpeed) * moveSpeedCVM;
        animator.SetFloat("MoveSpeed", moveSpeedCVM);
        minionAtkDmg = float.Parse(DataManager.AllMinionList[_minionID].minionAtkDmg.Split('x')[0]) + atkDmgCV;
        minionArmor = float.Parse(DataManager.AllMinionList[_minionID].minionArmor) + armorCV;
        minionAtkRange = float.Parse(DataManager.AllMinionList[_minionID].minionAtkRange) + atkRangeCV;
        boxSize = new Vector2(minionAtkRange, minionAtkRange);
        minionCriticalDmg = float.Parse(DataManager.AllMinionList[_minionID].minionCriticalDmg) + criticalDmgCV;
        minionCriticalChance = float.Parse(DataManager.AllMinionList[_minionID].minionCriticalChance) + criticalChanceCV;
        // status timer
        if(MinionStatus.Count > 0)
        {
            for(int i=0; i<MinionStatus.Count; i++)
            {
                MinionStatus[i].buffTime -= Time.deltaTime;
                if(MinionStatus[i].buffTime <= 0)
                {
                    RemoveStatus(MinionStatus[i].statusID);
                    MinionStatus.RemoveAt(i);
                    Destroy(StatusBar.transform.GetChild(i).gameObject);
                }
            }
        }

        if(minionNowHP <= 0)
        {
            HPBar.GetComponent<Image>().fillAmount = 0;
            StatusBar.gameObject.SetActive(false);
            animator.SetTrigger("Dead");
            animator.SetBool("isMoving", false);
        }
        else
        {
            HPBar.position = Camera.main.WorldToScreenPoint(new Vector3(transform.GetComponent<Collider2D>().bounds.center.x, transform.GetComponent<Collider2D>().bounds.center.y - GetComponent<BoxCollider2D>().size.y * transform.localScale.y, 0));
            HPBar.GetComponent<Image>().fillAmount = minionNowHP / minionMaxHP;
            StatusBar.position = Camera.main.WorldToScreenPoint(new Vector3(transform.GetComponent<Collider2D>().bounds.center.x, transform.GetComponent<Collider2D>().bounds.center.y + GetComponent<BoxCollider2D>().size.y * transform.localScale.y, 0));
            if(target != null)
            {
                if(Vector2.Distance(transform.GetComponent<Collider2D>().bounds.center, target.GetComponent<Collider2D>().bounds.center) * Mathf.Abs(transform.localScale.x) < minionAtkRange)
                {
                    if(curTime <= 0)
                    {
                        if(UnityEngine.Random.Range(0, 100) < minionCriticalChance) isCritical = true;
                        else isCritical = false;
                        if(minionAtkType == 2) projectileCount++;
                        animator.SetTrigger("Attack");
                        if(isCritical) animator.SetBool("isCritical", true);
                        else animator.SetBool("isCritical", false);
                        curTime = atkCoolTime;
                    }
                    else
                    {
                        animator.SetBool("isMoving", false);
                        curTime -= Time.deltaTime;
                    }
                    
                }
                else
                {
                    animator.SetBool("isMoving", true);
                    moveDirection = target.GetComponent<Collider2D>().bounds.center - GetComponent<Collider2D>().bounds.center;
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
                    moveDirection.Normalize();
                    transform.Translate(moveDirection * Time.deltaTime * minionMoveSpeed);
                }
            }
            else
            {
                animator.SetBool("isMoving", false);
            }
        }
    }

    public Vector2 TrackingBox;
    private void UpdateTarget()
    {
        Collider2D[] tempCols = Physics2D.OverlapBoxAll(GetComponent<Collider2D>().bounds.center, TrackingBox, 0);
        int cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Enemy") cnt++;
        }
        Collider2D[] cols = new Collider2D[cnt];
        cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Enemy") 
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
    }

    private void MeleeAttack()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(transform.GetComponent<Collider2D>().bounds.center, boxSize, 0);
        foreach(Collider2D collider in collider2Ds)
        {
            if(collider.tag == "Enemy")
            {
                if(isCritical)
                {
                    if(collider.gameObject.GetComponent<EnemyScript>().enemyAbilities.Contains("0004"))
                    {
                        collider.gameObject.GetComponent<EnemyScript>().BeAttacked(minionAtkDmg * minionCriticalDmg + collider.GetComponent<EnemyScript>().enemyArmor, 0.6f, isCritical);
                    }
                    else
                    {
                        collider.gameObject.GetComponent<EnemyScript>().BeAttacked(minionAtkDmg * minionCriticalDmg, 0.6f, isCritical);
                    }
                }
                else
                {
                    if(collider.gameObject.GetComponent<EnemyScript>().enemyAbilities.Contains("0004"))
                    {
                        collider.gameObject.GetComponent<EnemyScript>().BeAttacked(minionAtkDmg + collider.GetComponent<EnemyScript>().enemyArmor, 0.3f, isCritical);
                    }
                    else
                    {
                        collider.gameObject.GetComponent<EnemyScript>().BeAttacked(minionAtkDmg, 0.3f, isCritical);
                    }
                }

                if(minionAbilities.Contains("0011"))
                {
                    collider.gameObject.GetComponent<EnemyScript>().attackedByZombie = true;
                }
            }
        }
    }

    private void RangedAttack()
    {
        if(projectileCount > 0)
        {
            GameObject p = Instantiate(Resources.Load<GameObject>($"Projectiles/ProjectileMinion{stringID}"), GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
            p.GetComponentInChildren<ProjectileScript>().SetProjectile(gameObject, target, isCritical);
            projectileCount--;
        }
    }

    public void BeAttacked(float dmg)
    {
        RectTransform DmgText = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
        DmgText.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
        if(dmg - minionArmor <= 0)
        {
            DmgText.gameObject.GetComponent<FloatingText>().SetText("0", "#FFFFFF");
        }
        else
        {
            minionNowHP -= dmg - minionArmor;
            DmgText.gameObject.GetComponent<FloatingText>().SetText(Mathf.Round(dmg).ToString(), "#FFAAAA");
            OnDamaged();
        }

    }

    public Vector2 boxSize;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetComponent<Collider2D>().bounds.center, boxSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(GetComponent<Collider2D>().bounds.center, TrackingBox);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            float dmg = 0;
            EnemyScript colES = collision.gameObject.GetComponent<EnemyScript>();
            if(colES.enemyCollisionDmg - minionArmor > 0)
            {
                dmg = colES.enemyCollisionDmg - minionArmor;
                minionNowHP -= dmg;
                // print($"minionNowHP: {minionNowHP}");
                OnDamaged();
            }
            RectTransform DmgText = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
            DmgText.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
            DmgText.gameObject.GetComponent<FloatingText>().SetText(Mathf.Round(dmg).ToString(), "#FFAAAA");
        }
    }

    void OnDamaged()
    {
        gameObject.layer = 10;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        Invoke("OffDamaged", 1);
    }

    void OffDamaged()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.layer = 9;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }
    
    public void AddStatus(string _statusID)
    {
        int isAlreadyGotIt = MinionStatus.FindIndex(x => x.statusID == _statusID);
        if(isAlreadyGotIt != -1)
        {
            MinionStatus[isAlreadyGotIt].buffTime = float.Parse(DataManager.AllStatusList.Find(x => x.statusID == _statusID).buffTime);
        }
        else
        {
            Status _status = DataManager.AllStatusList.Find(x => x.statusID == _statusID);
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
                else if(_status.buffStat[i] == "armorCV")
                {
                    armorCV += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "atkRangeCV")
                {
                    atkRangeCV += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "criticalDmgCV")
                {
                    criticalDmgCV += float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "criticalChanceCV")
                {
                    criticalChanceCV += float.Parse(_status.buffValue[i]);
                }
                else
                {
                    print($"wrong buffStat name : '{_status.buffStat[i]}'");
                }
            }
            Instantiate(Resources.Load($"UIs/Icons/Status/Status{_statusID}"), new Vector2(0, 0), Quaternion.identity, StatusBar.transform);
            tempStatus.buffTime = float.Parse(_status.buffTime);
            MinionStatus.Add(tempStatus);
        }
    }

    public void RemoveStatus(string _statusID)
    {
        int idx = MinionStatus.FindIndex(x => x.statusID == _statusID);
        for(int i=0; i<MinionStatus[idx].buffStat.Length; i++)
        {
            if(MinionStatus[idx].buffStat[i] == "atkSpeedCVM")
            {
                atkSpeedCVM -= MinionStatus[idx].buffValue[i];
            }
            else if(MinionStatus[idx].buffStat[i] == "moveSpeedCVM")
            {
                moveSpeedCVM -= MinionStatus[idx].buffValue[i];
            }
            else if(MinionStatus[idx].buffStat[i] == "atkDmgCV")
            {
                atkDmgCV -= MinionStatus[idx].buffValue[i];
            }
            else if(MinionStatus[idx].buffStat[i] == "armorCV")
            {
                armorCV -= MinionStatus[idx].buffValue[i];
            }
            else if(MinionStatus[idx].buffStat[i] == "atkRangeCV")
            {
                atkRangeCV -= MinionStatus[idx].buffValue[i];
            }
            else if(MinionStatus[idx].buffStat[i] == "criticalDmgCV")
            {
                criticalDmgCV -= MinionStatus[idx].buffValue[i];
            }
            else if(MinionStatus[idx].buffStat[i] == "criticalChanceCV")
            {
                criticalChanceCV -= MinionStatus[idx].buffValue[i];
            }
        }
    }
}
