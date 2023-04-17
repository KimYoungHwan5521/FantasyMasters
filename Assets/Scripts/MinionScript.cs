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
    public float maxHPCV = 0;
    public float maxHPCVM = 1;
    public float minionNowHP;
    public int minionAtkType;
    public float minionAtkDmg;
    public float atkDmgCV = 0;
    public float atkDmgCVM = 1;
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
    public float armorCVM = 1;
    public int minionMoveType = 1;
    public float minionMoveSpeed;
    public float moveSpeedCVM = 1;
    public float sizeCVM = 1;
    public List<string> minionAbilities;
    public List<StatusV> MinionStatus;

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

    public GameObject Hero;
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
        minionAtkDmg = float.Parse(minionInfo.minionAtkDmg.Split('x')[0]);
        minionAtkSpeed = float.Parse(minionInfo.minionAtkSpeed);
        atkCoolTime = 10 / minionAtkSpeed;
        minionAtkRange = float.Parse(minionInfo.minionAtkRange);
        minionCriticalDmg = float.Parse(minionInfo.minionCriticalDmg);
        minionCriticalChance = float.Parse(minionInfo.minionCriticalChance);
        minionArmor = float.Parse(minionInfo.minionArmor);
        minionMoveType = int.Parse(minionInfo.minionMoveType);
        minionMoveSpeed = float.Parse(minionInfo.minionMoveSpeed);
        minionAbilities = minionInfo.minionAbilities.ToList();
        MinionStatus = new List<StatusV>();
        TrackingBox = new Vector2(10, 10);

        animator = GetComponent<Animator>();
        HPBar = Instantiate(Resources.Load<RectTransform>("UIs/HPBar"), new Vector3(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);
        HPBar.localScale = new Vector2(GetComponent<BoxCollider2D>().size.x * 5 * transform.localScale.x, 1);
        StatusBar = Instantiate(Resources.Load<RectTransform>("UIs/StatusBar"), new Vector3(0, 0), Quaternion.identity, GameObject.Find("Canvas").transform);

        Hero = GameObject.FindWithTag("Player");
        List<string> hAbilities = Hero.GetComponent<HeroScript>().abilities;
        if(hAbilities.Contains("0019")) AddStatus("0005");
        if(hAbilities.Contains("0020")) AddStatus("0006");
        if(hAbilities.Contains("0030") && minionAtkDmg <= 30) AddStatus("0009");
        if(hAbilities.Contains("0032"))
        {
            AddStatus("0010");
            flockAttack = true;
        }
        if(hAbilities.Contains("0033")) 
        {
            AddStatus("0011");
            floakDefense = true;
        }
        if(hAbilities.Contains("0040"))
        {
            for(int i=0; i<StageManager.mapMinionKillEnemies; i++)
            {
                int r = UnityEngine.Random.Range(0, 3);
                if(r == 0) atkDmgCV++;
                else if(r == 1) armorCV++;
                else maxHPCV += 10;
            }
        }
        if(_minionID == 14)
        {
            float val = StageManager.stageMinionDeath + StageManager.stageEnemyDeath;
            atkDmgCV += val;
            armorCV += val * 0.5f;
            maxHPCV += val * 10;
            minionNowHP += maxHPCV;
            transform.localScale = new Vector2(transform.localScale.x * (1 + 0.05f * val), transform.localScale.y * (1 + 0.05f * val));
            animatorCV = -1;
        }

        target = null;
        if(minionMoveType == 4) InvokeRepeating("UpdateTargetAlli", 0, 0.25f);
        InvokeRepeating("UpdateTarget", 0, 0.25f);
        if(minionAbilities.Contains("0051"))
        {
            StartCoroutine(Heal());
        }
    }

    Vector2 moveDirection;
    GameObject target;
    GameObject targetAlli;
    bool isCritical = false;
    public bool attackedByZombie = false;
    public bool movable = true;
    public bool attackable = true;

    public bool fired = false;
    public bool flockAttack = false;
    public bool floakDefense = false;
    void Update()
    {
        minionAtkSpeed = float.Parse(DataManager.AllMinionList[_minionID].minionAtkSpeed) * atkSpeedCVM;
        animator.SetFloat("AttackSpeed", atkSpeedCVM);
        atkCoolTime = 10 / minionAtkSpeed;
        minionMoveSpeed = float.Parse(DataManager.AllMinionList[_minionID].minionMoveSpeed) * moveSpeedCVM;
        animator.SetFloat("MoveSpeed", moveSpeedCVM);
        minionMaxHP = float.Parse(DataManager.AllMinionList[_minionID].minionMaxHP) * maxHPCVM;
        if(minionNowHP > minionMaxHP) minionNowHP = minionMaxHP;
        if(flockAttack) 
        {
            float abilityAtkDmgCV = GameObject.FindGameObjectsWithTag("Minion").Length * 5;
            minionAtkDmg = (float.Parse(DataManager.AllMinionList[_minionID].minionAtkDmg.Split('x')[0]) + atkDmgCV + abilityAtkDmgCV) * atkDmgCVM;
        }
        else minionAtkDmg = (float.Parse(DataManager.AllMinionList[_minionID].minionAtkDmg.Split('x')[0]) + atkDmgCV) * atkDmgCVM;
        if(floakDefense)
        {
            float abilityArmorCV = GameObject.FindGameObjectsWithTag("Minion").Length;
            minionArmor =minionArmor = (float.Parse(DataManager.AllMinionList[_minionID].minionArmor) + armorCV + abilityArmorCV) * armorCVM;
        }
        else minionArmor = (float.Parse(DataManager.AllMinionList[_minionID].minionArmor) + armorCV) * armorCVM;
        minionAtkRange = (float.Parse(DataManager.AllMinionList[_minionID].minionAtkRange) + atkRangeCV) * sizeCVM;
        minionCriticalDmg = float.Parse(DataManager.AllMinionList[_minionID].minionCriticalDmg) + criticalDmgCV;
        minionCriticalChance = float.Parse(DataManager.AllMinionList[_minionID].minionCriticalChance) + criticalChanceCV;
        
        bool fear = false;
        fired = false;
        // status timer
        if(MinionStatus.Count > 0)
        {
            for(int i=0; i<MinionStatus.Count; i++)
            {
                if(MinionStatus[i].statusID == "0004") fear = true;
                else if(MinionStatus[i].statusID == "0008") fired = true;
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
            gameObject.layer = 10;
            animator.SetTrigger("Dead");
            animator.SetBool("isMoving", false);
        }
        else
        {
            HPBar.position = Camera.main.WorldToScreenPoint(new Vector3(transform.GetComponent<Collider2D>().bounds.center.x, transform.GetComponent<Collider2D>().bounds.center.y - GetComponent<BoxCollider2D>().size.y * transform.localScale.y - 0.1f, 0));
            HPBar.GetComponent<Image>().fillAmount = minionNowHP / minionMaxHP;
            StatusBar.position = Camera.main.WorldToScreenPoint(new Vector3(transform.GetComponent<Collider2D>().bounds.center.x, transform.GetComponent<Collider2D>().bounds.center.y + GetComponent<BoxCollider2D>().size.y * transform.localScale.y + 0.1f, 0));
            if(target != null)
            {
                if(Vector2.Distance(transform.GetComponent<Collider2D>().bounds.center, target.GetComponent<Collider2D>().bounds.center) * Mathf.Abs(transform.localScale.x) < minionAtkRange)
                {
                    if(curTime <= 0 && attackable && minionMoveType != 4)
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
                        if(minionMoveType == 3 && movable)
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
                            transform.Translate(moveDirection * Time.deltaTime * minionMoveSpeed);
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
                        if(fear || minionMoveType == 2) moveDirection = GetComponent<Collider2D>().bounds.center - target.GetComponent<Collider2D>().bounds.center;
                        else if(minionMoveType == 1 || minionMoveType == 3) moveDirection = target.GetComponent<Collider2D>().bounds.center - GetComponent<Collider2D>().bounds.center;
                        else if(minionMoveType == 4)
                        {
                            if(Vector2.Distance(GetComponent<Collider2D>().bounds.center, targetAlli.GetComponent<Collider2D>().bounds.center) > minionAtkRange)
                            {
                                moveDirection = targetAlli.GetComponent<Collider2D>().bounds.center - GetComponent<Collider2D>().bounds.center;
                            }
                            else
                            {
                                moveDirection = (GetComponent<Collider2D>().bounds.center - target.GetComponent<Collider2D>().bounds.center) + (targetAlli.GetComponent<Collider2D>().bounds.center - GetComponent<Collider2D>().bounds.center);
                            }
                        }
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
                        if(fear || minionMoveType == 2) transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                        transform.localScale = new Vector2(transform.localScale.x * animatorCV, transform.localScale.y);
                        moveDirection.Normalize();
                        transform.Translate(moveDirection * Time.deltaTime * minionMoveSpeed);
                    }
                    else animator.SetBool("isMoving", false);
                }
            }
            else
            {
                if(minionMoveType == 4 && targetAlli != null && Vector2.Distance(GetComponent<Collider2D>().bounds.center, targetAlli.GetComponent<Collider2D>().bounds.center) > minionAtkRange)
                {
                    animator.SetBool("isMoving", true);
                    moveDirection = targetAlli.GetComponent<Collider2D>().bounds.center - GetComponent<Collider2D>().bounds.center;
                    moveDirection.Normalize();
                    transform.Translate(moveDirection * Time.deltaTime * minionMoveSpeed);
                }
                else animator.SetBool("isMoving", false);
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
            if(minionMoveType == 2 && tempCols[i].tag == "Wall") cnt++;
        }
        Collider2D[] cols = new Collider2D[cnt];
        cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Enemy" || (minionMoveType == 2 && tempCols[i].tag == "Wall")) 
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

    private void UpdateTargetAlli()
    {
        Collider2D[] tempCols = Physics2D.OverlapBoxAll(GetComponent<Collider2D>().bounds.center, TrackingBox, 0);
        int cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if((tempCols[i].tag == "Minion" && tempCols[i].gameObject.GetComponent<MinionScript>().minionNowHP < tempCols[i].gameObject.GetComponent<MinionScript>().minionMaxHP) || tempCols[i].tag == "Player") cnt++;
        }
        Collider2D[] cols = new Collider2D[cnt];
        cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if((tempCols[i].tag == "Minion" && tempCols[i].gameObject.GetComponent<MinionScript>().minionNowHP < tempCols[i].gameObject.GetComponent<MinionScript>().minionMaxHP) || tempCols[i].tag == "Player") 
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
            targetAlli = cols[minDisIdx].gameObject;
        }
        else targetAlli = Hero;
    }

    private void MeleeAttack()
    {
        if(minionAbilities.Contains("0044")) BeHealed(20);
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(GetComponent<Collider2D>().bounds.center, minionAtkRange * 0.5f);
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
                if(collider.gameObject.GetComponent<EnemyScript>().enemyNowHP <= 0) StageManager.mapMinionKillEnemies++;
                
                if(minionAbilities.Contains("0005"))
                {
                    collider.gameObject.GetComponent<EnemyScript>().AddStatus("0002");
                }
                if(minionAbilities.Contains("0006"))
                {
                    collider.gameObject.GetComponent<EnemyScript>().AddStatus("0003");
                }
                if(minionAbilities.Contains("0025"))
                {
                    collider.gameObject.GetComponent<EnemyScript>().AddStatus("0007");
                }
                if(minionAbilities.Contains("0026"))
                {
                    collider.gameObject.GetComponent<EnemyScript>().AddStatus("0008");
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
            if(minionAbilities.Contains("0044")) BeHealed(20);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetComponent<Collider2D>().bounds.center, minionAtkRange * 0.5f);
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

    public void BeHealed(float value)
    {
        if(fired) value /= 2;
        if(value > 0 && minionNowHP < minionMaxHP) 
        {
            minionNowHP += value;
            if(minionNowHP > minionMaxHP) minionNowHP = minionMaxHP;
            RectTransform text = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
            text.GetComponent<FloatingText>().SetText($"+{Mathf.Round(value)}", "#00FF00");
            text.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
        }
    }
    
    public void AddStatus(string _statusID)
    {
        int isAlreadyGotIt = MinionStatus.FindIndex(x => x.statusID == _statusID);
        Status _status = DataManager.AllStatusList.Find(x => x.statusID == _statusID);
        if(isAlreadyGotIt != -1)
        {
            MinionStatus[isAlreadyGotIt].buffTime = float.Parse(_status.buffTime);
        }
        else if(!(_status.statusID == "0004" && minionAbilities.Contains("0017")) || !(_status.statusID == "0007" && minionAbilities.Contains("0027") || !(_status.statusID == "0008" && minionAbilities.Contains("0028"))))
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
                else if(_status.buffStat[i] == "armorCVM")
                {
                    armorCVM += float.Parse(_status.buffValue[i]);
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
                else if(_status.buffStat[i] == "maxHPCVM")
                {
                    maxHPCVM += float.Parse(_status.buffValue[i]);
                    minionNowHP += float.Parse(DataManager.AllMinionList.Find(x => x.minionID == stringID).minionMaxHP) * float.Parse(_status.buffValue[i]);
                }
                else if(_status.buffStat[i] == "sizeCVM")
                {
                    sizeCVM += float.Parse(_status.buffValue[i]);
                    transform.localScale = new Vector2(transform.localScale.x * (1 + float.Parse(_status.buffValue[i])), transform.localScale.y * (1 + float.Parse(_status.buffValue[i])));
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
            else if(MinionStatus[idx].buffStat[i] == "atkDmgCVM")
            {
                atkDmgCVM -= MinionStatus[idx].buffValue[i];
            }
            else if(MinionStatus[idx].buffStat[i] == "armorCV")
            {
                armorCV -= MinionStatus[idx].buffValue[i];
            }
            else if(MinionStatus[idx].buffStat[i] == "armorCVM")
            {
                armorCVM -= MinionStatus[idx].buffValue[i];
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
            else if(MinionStatus[idx].buffStat[i] == "attackable")
            {
                if(MinionStatus[idx].buffValue[i] == -1)
                {
                    attackable = true;
                }
                else attackable = false;
            }
            else if(MinionStatus[idx].buffStat[i] == "movable")
            {
                if(MinionStatus[idx].buffValue[i] == -1)
                {
                    movable = true;
                }
                else movable = false;
            }
            else if(MinionStatus[idx].buffStat[i] == "nowHPCV")
            {
                StopCoroutine("ContinuousDmg");
            }
            else if(MinionStatus[idx].buffStat[i] == "maxHPCVM")
            {
                maxHPCVM -= MinionStatus[idx].buffValue[i];
            }
            else if(MinionStatus[idx].buffStat[i] == "sizeCVM")
            {
                sizeCVM -= MinionStatus[idx].buffValue[i];
                transform.localScale = new Vector2(transform.localScale.x / (1 + MinionStatus[idx].buffValue[i]), transform.localScale.y / (1 + MinionStatus[idx].buffValue[i]));
            }
        }
    }
    
    IEnumerator ContinuousDmg(float dmg, float time)
    {
        int t = (int)time;
        for(int i=0; i<t; i++)
        {
            minionNowHP += dmg;
            RectTransform DmgText = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
            DmgText.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
            DmgText.gameObject.GetComponent<FloatingText>().SetText($"{dmg}", "#FF0000");
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator Heal()
    {
        while(true)
        {
            if(targetAlli != null)
            {
                animator.SetTrigger("Attack");
                if(targetAlli.tag == "Player") targetAlli.GetComponent<HeroScript>().BeHealed(10);
                else targetAlli.GetComponent<MinionScript>().BeHealed(10);
            }
            yield return new WaitForSeconds(1);
        }
    }
}
