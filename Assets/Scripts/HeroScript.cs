using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HeroScript : MonoBehaviour
{
    Animator animator;
    GameObject Map;
    DataManager DataManager;

    Hero heroInfo;
    public int _heroID;
    public string stringID;
    public string heroNameKR;
    public string[] attributes;
    public float maxHP;
    public float maxHPCV = 0;
    public float nowHP;
    public float HPRegeneration;
    public float HPRegenerationCV = 0;
    public int atkType;
    public float atkDmg;
    public float atkDmgCV = 0;
    public float atkDmgCVM = 1;
    public float atkSpeed;
    public float atkSpeedCVM = 1;
    public float atkRange;
    public float atkRangeCV = 0;
    public float criticalDmg;
    public float criticalDmgCV = 0;
    public float criticalChance;
    public float criticalChanceCV = 0;
    public float armor;
    public float armorCV = 0;
    public float moveSpeed;
    public float moveSpeedCVM = 1;
    public List<string> abilities;
    public List<StatusV> HeroStatus;
    public List<Item> HeroItems;
    public GameObject StatusSprites;

    private float curTime;
    public float atkCoolTime;
    public Vector2 boxSize;
    private float predateCurTime;
    public float predateCoolTime;

    public Image HPbar;
    public Text TextMaxHP;
    public Text TextNowHP;

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
        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        _heroID = DataManager.selectedHeroID;
        int cntDgit = 0;
        int copy_heroID = _heroID;
        for(int i=0; i<4; i++)
        {
            if(copy_heroID / 10 > 0)
            {
                cntDgit++;
                copy_heroID /= 10;
            }
            else break;
        }
        stringID = "";
        for(int i=0;i<3 - cntDgit; i++)
        {
            stringID += "0";
        }
        stringID += _heroID.ToString();
        int idx = DataManager.AllHeroList.FindIndex(x => x.heroID == stringID);
        heroInfo = DataManager.AllHeroList[idx];
        heroNameKR = heroInfo.heroNameKR;
        attributes = heroInfo.heroAttributes;
        maxHP = float.Parse(heroInfo.heroMaxHP);
        nowHP = maxHP;
        HPRegeneration = float.Parse(heroInfo.heroHPRegeneration);
        atkType = int.Parse(heroInfo.heroAtkType);
        atkDmg = float.Parse(heroInfo.heroAtkDmg.Split('x')[0]);
        atkSpeed = float.Parse(heroInfo.heroAtkSpeed);
        atkCoolTime = 10 / atkSpeed;
        atkRange = float.Parse(heroInfo.heroAtkRange);
        boxSize = new Vector2(atkRange, atkRange);
        criticalDmg = float.Parse(heroInfo.heroCriticalDmg);
        criticalChance = float.Parse(heroInfo.heroCriticalChance);
        armor = float.Parse(heroInfo.heroArmor);
        moveSpeed = float.Parse(heroInfo.heroMoveSpeed);
        abilities = heroInfo.heroAbilities.ToList();
        HeroStatus = new List<StatusV>();
        StatusSprites = GameObject.Find("HeroStatus");
        HeroItems = new List<Item>();
        resurrection = 0;
        animator = GetComponentInChildren<Animator>();
        transform.position = new Vector2(0, 0);

        Map = GameObject.Find("Map");
        HPbar = GameObject.Find("HeroHPBar").GetComponent<Image>();
        TextMaxHP = GameObject.Find("HeroMaxHPText").GetComponent<Text>();
        TextNowHP = GameObject.Find("HeroNowHPText").GetComponent<Text>();
        InvokeRepeating("UpdateTarget", 0, 0.25f);
        InvokeRepeating("HPRegenerationMethod", 0, 1);
    }

    public GameObject target = null;
    public GameObject target2 = null;
    public bool isCritical = false;
    private int projectileCount = 0;
    public bool controllable = true;
    public bool attackable = true;
    public int resurrection;

    public bool fired = false;
    void Update()
    {
        TextMaxHP.text = Mathf.Ceil(maxHP).ToString();
        TextNowHP.text = Mathf.Ceil(nowHP).ToString();
        HPbar.fillAmount = nowHP / maxHP;
        if(nowHP / maxHP < 0.3f) HPbar.color = Color.red;
        else HPbar.color = Color.green;
        atkSpeed = float.Parse(DataManager.AllHeroList[_heroID].heroAtkSpeed) * atkSpeedCVM;
        animator.SetFloat("AttackSpeed", atkSpeedCVM);
        atkCoolTime = 10 / atkSpeed;
        moveSpeed = float.Parse(DataManager.AllHeroList[_heroID].heroMoveSpeed) * moveSpeedCVM;
        animator.SetFloat("MoveSpeed", moveSpeedCVM);
        atkDmg = (float.Parse(DataManager.AllHeroList[_heroID].heroAtkDmg.Split('x')[0]) + atkDmgCV) * atkDmgCVM;
        armor = float.Parse(DataManager.AllHeroList[_heroID].heroArmor) + armorCV;
        atkRange = float.Parse(DataManager.AllHeroList[_heroID].heroAtkRange) + atkRangeCV;
        boxSize = new Vector2(atkRange, atkRange);
        criticalDmg = float.Parse(DataManager.AllHeroList[_heroID].heroCriticalDmg) + criticalDmgCV;
        criticalChance = float.Parse(DataManager.AllHeroList[_heroID].heroCriticalChance) + criticalChanceCV;
        predateCoolTime = float.Parse(DataManager.AllAbilityList.Find(x => x.abilityID == "0015").abilityCoolTime);

        bool fear = false;
        fired = false;
        // status timer
        if(HeroStatus.Count > 0)
        {
            for(int i=0; i<HeroStatus.Count; i++)
            {
                if(HeroStatus[i].statusID == "0004") fear = true;
                else if(HeroStatus[i].statusID == "0008") fired = true;
                HeroStatus[i].buffTime -= Time.deltaTime;
                if(HeroStatus[i].buffTime <= 0)
                {
                    RemoveStatus(HeroStatus[i].statusID);
                    HeroStatus.RemoveAt(i);
                    Destroy(StatusSprites.transform.GetChild(i).gameObject);
                }
            }
        }

        // 이동
        Vector2 moveDirection = Vector2.zero;
        if(controllable)
        {
            if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                moveDirection += Vector2.right;
                if(transform.localScale.x < 0)
                {
                    transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                }
                else
                {
                    transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
                }
            }
            if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                moveDirection += Vector2.left;
                if(transform.localScale.x > 0)
                {
                    transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                }
                else
                {
                    transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
                }
            }
            if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                moveDirection += Vector2.up;
                
            }
            if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                moveDirection += Vector2.down;

            }
            moveDirection.Normalize();
            transform.Translate(moveDirection * Time.deltaTime * moveSpeed);

        }
        if(fear)
        {
            if(target != null)
            {
                moveDirection = GetComponent<Collider2D>().bounds.center - target.GetComponent<Collider2D>().bounds.center;
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
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }
        animator.SetBool("isMoving", moveDirection.magnitude > 0);
        
        // 공격
        if(curTime <= 0 && attackable)
        {
            if(target != null)
            {
                Vector2 targetPos = target.GetComponent<Collider2D>().bounds.center - GetComponent<Collider2D>().bounds.center;

                if(targetPos.x * transform.localScale.x < 0) transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                if(UnityEngine.Random.Range(0, 100) < criticalChance) isCritical = true;
                else isCritical = false;
                if(isCritical) animator.SetBool("isCritical", true);
                else animator.SetBool("isCritical", false);
                if(atkType == 2) projectileCount++;
                animator.SetTrigger("Attack");
                curTime = atkCoolTime;
            }
            else
            {
                animator.ResetTrigger("Attack");
            }
        }
        else
        {
            curTime -= Time.deltaTime;
        }
        predateCurTime -= Time.deltaTime;
    }
    
    private void UpdateTarget()
    {
        Vector2 box = new Vector2(atkRange, atkRange);
        Collider2D[] tempCols = Physics2D.OverlapBoxAll(GetComponent<Collider2D>().bounds.center, box, 0);
        int cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Enemy" && tempCols[i].gameObject.GetComponent<EnemyScript>().enemyNowHP > 0) cnt++;
        }
        Collider2D[] cols = new Collider2D[cnt];
        cnt = 0;
        for(int i=0; i<tempCols.Length; i++)
        {
            if(tempCols[i].tag == "Enemy" && tempCols[i].gameObject.GetComponent<EnemyScript>().enemyNowHP > 0) 
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
            target2 = cols[UnityEngine.Random.Range(0,cols.Length)].gameObject;
        }
    }

    private void HPRegenerationMethod()
    {
        float totalre = HPRegeneration + HPRegenerationCV;
        if(fired) totalre /= 2;
        if(totalre > 0) nowHP += totalre;
        if(nowHP > maxHP) nowHP = maxHP;
    }

    private void MeleeAttack()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(GetComponent<Collider2D>().bounds.center, boxSize, 0);
        foreach(Collider2D collider in collider2Ds)
        {
            if(collider.tag == "Enemy")
            {
                if(collider.GetComponent<EnemyScript>().enemyNowHP <= collider.GetComponent<EnemyScript>().enemyMaxHP * 0.3 && predateCurTime <= 0)
                {
                    Instantiate(Resources.Load<GameObject>("Effects/Predate"), collider.GetComponent<Collider2D>().bounds.center, Quaternion.identity);
                    float rec = 50;
                    if(fired) rec /= 2;
                    nowHP += rec;
                    RectTransform text = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
                    text.GetComponent<FloatingText>().SetText($"+{rec}", "#00FF00");
                    text.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
                    collider.GetComponent<EnemyScript>().enemyNowHP = 0;
                    predateCurTime = predateCoolTime;
                }
                else
                {
                    float isCf = 0;
                    if(isCritical) isCf = 1;
                    float isIA = 0;
                    if(abilities.Contains("0004")) isIA = 1;
                    float dmg = atkDmg;
                    float eA = collider.GetComponent<EnemyScript>().enemyArmor;
                    if(isCritical) dmg = atkDmg * criticalDmg + eA * isIA;
                    else dmg = atkDmg + eA * isIA;
                    collider.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg, 0.3f + 0.3f * isCf, isCritical);

                    if(abilities.Contains("0016")) 
                    {
                        float rec = (dmg - eA) / 10;
                        if(fired) rec /= 2;
                        nowHP += rec;
                        if(rec >= 1)
                        {
                            RectTransform text = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
                            text.GetComponent<FloatingText>().SetText($"+{Mathf.Round(rec)}", "#00FF00");
                            text.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
                        }

                    }
                    if(abilities.Contains("0005"))
                    {
                        collider.gameObject.GetComponent<EnemyScript>().AddStatus("0002");
                    }
                    if(abilities.Contains("0006"))
                    {
                        collider.gameObject.GetComponent<EnemyScript>().AddStatus("0003");
                    }
                    if(abilities.Contains("0025") && !collider.gameObject.GetComponent<EnemyScript>().enemyAbilities.Contains("0027"))
                    {
                        collider.gameObject.GetComponent<EnemyScript>().AddStatus("0007");
                    }
                    if(abilities.Contains("0026") && !collider.gameObject.GetComponent<EnemyScript>().enemyAbilities.Contains("0028"))
                    {
                        collider.gameObject.GetComponent<EnemyScript>().AddStatus("0008");
                    }
                    
                }

                if(abilities.Contains("0011"))
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
            GameObject p = Instantiate(Resources.Load<GameObject>($"Projectiles/ProjectileHero{stringID}"), GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
            p.GetComponentInChildren<ProjectileScript>().SetProjectile(gameObject, target, isCritical);
            if(abilities.Contains("0024"))
            {
                Invoke(null, 0.3f);
                GameObject p2 = Instantiate(Resources.Load<GameObject>($"Projectiles/ProjectileHero{stringID}"), GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
                p2.GetComponentInChildren<ProjectileScript>().SetProjectile(gameObject, target2, isCritical);
            }
            projectileCount--;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetComponent<Collider2D>().bounds.center, boxSize);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            EnemyScript colES = collision.gameObject.GetComponent<EnemyScript>();
            float dmg = 0;
            RectTransform DmgText = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
            DmgText.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
            if(colES.enemyCollisionDmg - armor > 0) 
            {
                dmg = colES.enemyCollisionDmg - armor;
                nowHP -= dmg;
                DmgText.gameObject.GetComponent<FloatingText>().SetText(Mathf.Round(dmg).ToString(), "#FFAAAA");
                OnDamaged();
            }
            else DmgText.gameObject.GetComponent<FloatingText>().SetText(Mathf.Round(dmg).ToString());
        }
    }

    public void BeAttacked(float dmg)
    {
        dmg -= armor;
        RectTransform DmgText = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
        DmgText.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
        if(dmg > 0)
        {
            nowHP -= dmg;
            DmgText.gameObject.GetComponent<FloatingText>().SetText(Mathf.Round(dmg).ToString(), "#FFAAAA");
            OnDamaged();
        }
        else DmgText.gameObject.GetComponent<FloatingText>().SetText("0", "#FFAAAA");
    }

    void OnDamaged()
    {
        //충돌시 플레이어의 레이어가 PlayerInv 레이어로 변함 
        gameObject.layer = 12;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);//무적시간일 때 플레이어가 투명하게
        Invoke("OffDamaged", 1);
    }

    void OffDamaged()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.layer = 11;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void AddStatus(string _statusID)
    {
        int isAlreadyGotIt = HeroStatus.FindIndex(x => x.statusID == _statusID);
        Status _status = DataManager.AllStatusList.Find(x => x.statusID == _statusID);
        if(isAlreadyGotIt != -1)
        {
            HeroStatus[isAlreadyGotIt].buffTime = float.Parse(_status.buffTime);
        }
        else if(!(_status.statusID == "0004" && abilities.Contains("0017")))
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
                else if(_status.buffStat[i] == "HPRegenerationCV")
                {
                    HPRegenerationCV += float.Parse(_status.buffValue[i]);
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
                        controllable = false;
                    }
                    else controllable = true;
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
            Instantiate(Resources.Load($"UIs/Icons/Status/Status{_statusID}"), new Vector2(0, 0), Quaternion.identity, GameObject.Find("HeroStatus").transform);
            tempStatus.buffTime = float.Parse(_status.buffTime);
            HeroStatus.Add(tempStatus);
        }
    }

    public void RemoveStatus(string _statusID)
    {
        int idx = HeroStatus.FindIndex(x => x.statusID == _statusID);
        for(int i=0; i<HeroStatus[idx].buffStat.Length; i++)
        {
            if(HeroStatus[idx].buffStat[i] == "atkSpeedCVM")
            {
                atkSpeedCVM -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "moveSpeedCVM")
            {
                moveSpeedCVM -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "atkDmgCV")
            {
                atkDmgCV -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "atkDmgCVM")
            {
                atkDmgCVM -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "armorCV")
            {
                armorCV -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "atkRangeCV")
            {
                atkRangeCV -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "criticalDmgCV")
            {
                criticalDmgCV -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "criticalChanceCV")
            {
                criticalChanceCV -= HeroStatus[idx].buffValue[i];
            }
            else if(HeroStatus[idx].buffStat[i] == "attackable")
            {
                if(HeroStatus[idx].buffValue[i] == -1)
                {
                    attackable = true;
                }
                else attackable = false;
            }
            else if(HeroStatus[idx].buffStat[i] == "movable")
            {
                if(HeroStatus[idx].buffValue[i] == -1)
                {
                    controllable = true;
                }
                else controllable = false;
            }
            else if(HeroStatus[idx].buffStat[i] == "nowHPCV")
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
            nowHP -= dmg;
            RectTransform DmgText = Instantiate(Resources.Load<RectTransform>("Effects/FloatingText"), GetComponent<Collider2D>().bounds.center, Quaternion.identity, GameObject.Find("Canvas").transform);
            DmgText.position = Camera.main.WorldToScreenPoint(new Vector3(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y, 0));
            DmgText.gameObject.GetComponent<FloatingText>().SetText($"{dmg}", "#FF0000");
            yield return new WaitForSeconds(1);
        }
    }

    public void AddItem(string _itemID)
    {
        Item _item = DataManager.AllItemList.Find(x => x.itemID == _itemID);
        
        for(int i=0; i<_item.itemBuffStat.Length; i++)
        {
            if(_item.itemBuffStat[i] == "atkSpeedCVM")
            {
                atkSpeedCVM += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "moveSpeedCVM")
            {
                moveSpeedCVM += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "atkDmgCV")
            {
                atkDmgCV += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "atkDmgCVM")
            {
                atkDmgCVM += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "armorCV")
            {
                armorCV += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "maxHPCV")
            {
                maxHPCV += float.Parse(_item.itemBuffValue[i]);
                maxHP = float.Parse(DataManager.AllHeroList[_heroID].heroMaxHP) + maxHPCV;
                if(maxHP < 1) maxHP = 1;
            }
            else if(_item.itemBuffStat[i] == "HPRegenerationCV")
            {
                HPRegenerationCV += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "atkRangeCV")
            {
                atkRangeCV += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "criticalDmgCV")
            {
                criticalDmgCV += float.Parse(_item.itemBuffValue[i]);
            }
            else if(_item.itemBuffStat[i] == "criticalChanceCV")
            {
                criticalChanceCV += float.Parse(_item.itemBuffValue[i]);
            }
            else
            {
                print($"wrong itemBuffStat name : '{_item.itemBuffStat[i]}'");
            }
        }
        for(int i=0; i<_item.itemAbilities.Count; i++)
        {
            abilities.Add(_item.itemAbilities[i]);
        }
        HeroItems.Add(_item);
    }

}
