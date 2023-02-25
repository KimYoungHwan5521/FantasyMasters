using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public DataManager DataManager;
    public int _enemyID;
    public string enemyNameKR;
    float enemyMaxHP;
    float enemyNowHP;
    float enemyAtkDmg;
    float enemyAtkSpeed;
    float enemyAtkRange;
    float enemyArmor;
    float enemyMoveSpeed;

    void Start()
    {
        string stringID;
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
        for(int i=0; i < 3 - cntDigit / 10; i++)
        {
            stringID += "0";
        }
        stringID += _enemyID.ToString();

        int idx = DataManager.AllEnemyList.FindIndex(x => x.enemyID == stringID);
        Enemy enemyInfo = DataManager.AllEnemyList[idx];
        enemyNameKR = enemyInfo.enemyNameKR;
        enemyMaxHP = float.Parse(enemyInfo.enemyMaxHP);
        enemyNowHP = enemyMaxHP;
        enemyAtkDmg = float.Parse(enemyInfo.enemyAtkDmg);
        enemyAtkSpeed = float.Parse(enemyInfo.enemyAtkSpeed);
        enemyAtkRange = float.Parse(enemyInfo.enemyAtkRange);
        enemyArmor = float.Parse(enemyInfo.enemyArmor);
        // enemyMoveSpeed = float.Parse(enemyInfo.enemyMoveSpeed);
        enemyMoveSpeed = 1;
    }

    public GameObject Hero;
    void Update()
    {
        Vector2 moveDirection = Hero.transform.position - transform.position;
        if(Hero.transform.position.x < transform.position.x)
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
        transform.Translate(moveDirection * Time.deltaTime * enemyMoveSpeed);
    }

    public void BeAttacked(float dmg, float knockback)
    {
        enemyNowHP -= dmg;
        print($"enemyNowHP: {enemyNowHP}");
    }
}
