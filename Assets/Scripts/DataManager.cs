using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Hero
{
    public Hero(string _ID, string _NameKR, string _Attributes, string _HP, string _HPRegeneration, string _AtkDmg, string _AtkSpeed, string _AtkRange,
    string _CriticalDmg, string _CriticalChance, string _Armor, string _MoveSpeed, string _AbilityKR)
    {
        heroID = _ID;
        heroNameKR = _NameKR;
        heroAttributes = _Attributes.Split(' ');
        heroMaxHP = _HP;
        heroHPRegeneration = _HPRegeneration;
        heroAtkDmg = _AtkDmg;
        heroAtkSpeed = _AtkSpeed;
        heroAtkRange = _AtkRange;
        heroCriticalDmg = _CriticalDmg;
        heroCriticalChance = _CriticalChance;
        heroArmor = _Armor;
        heroMoveSpeed = _MoveSpeed;
        heroAbilityKR = _AbilityKR;
    }
    public string heroID;
    public string heroNameKR;
    public string[] heroAttributes;
    public string heroMaxHP;
    public string heroHPRegeneration;
    public string heroAtkDmg;
    public string heroAtkSpeed;
    public string heroAtkRange;
    public string heroCriticalDmg;
    public string heroCriticalChance;
    public string heroArmor;
    public string heroMoveSpeed;
    public string heroAbilityKR;
}

[System.Serializable]
public class Enemy
{
    public Enemy(string _ID, string _NameKR, string _HP, string _CollisionDmg, string _AtkDmg, string _AtkSpeed, string _AtkRange, string _Armor, string _MoveSpeed)
    {
        enemyID = _ID;
        enemyNameKR = _NameKR;
        enemyMaxHP = _HP;
        enemyCollisionDmg = _CollisionDmg;
        enemyAtkDmg = _AtkDmg;
        enemyAtkSpeed = _AtkSpeed;
        enemyAtkRange = _AtkRange;
        enemyArmor = _Armor;
        enemyMoveSpeed = _MoveSpeed;
    }
    public string enemyID;
    public string enemyNameKR;
    public string enemyMaxHP;
    public string enemyCollisionDmg;
    public string enemyAtkDmg;
    public string enemyAtkSpeed;
    public string enemyAtkRange;
    public string enemyArmor;
    public string enemyMoveSpeed;
}

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(instance.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);

        // initialData = new SavedData();


        // string data = JsonUtility.ToJson(initialData);
        // // persistentDataPath -> C:\Users\[user name]\AppData\LocalLow\[company name]\[product name]
        // File.WriteAllText(Application.persistentDataPath + "/initialData", data);
        // path = Application.persistentDataPath + "/SAVE";
    }

    public TextAsset HeroDB;
    public TextAsset EnemyDB;
    public static List<Hero> AllHeroList;
    public static List<Enemy> AllEnemyList;

    void Start()
    {
        AllHeroList = new List<Hero>();
        string[] line = HeroDB.text.Substring(0, HeroDB.text.Length).Split('\r');
        for(int i=1;i<line.Length;i++)
        {
            string[] row = line[i].Split('\t');
            if(i == 0)
            {
                AllHeroList.Add(new Hero(row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10], row[11], row[12]));
            }
            else
            {
                AllHeroList.Add(new Hero(row[0].Substring(1), row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10], row[11], row[12]));
            }
        }

        AllEnemyList = new List<Enemy>();
        line = EnemyDB.text.Substring(0, EnemyDB.text.Length).Split('\r');
        for(int i=1;i<line.Length;i++)
        {
            string[] row = line[i].Split('\t');
            if(i == 0)
            {
                AllEnemyList.Add(new Enemy(row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8]));
            }
            else
            {
                AllEnemyList.Add(new Enemy(row[0].Substring(1), row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8]));
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
