using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Hero
{
    public Hero(string _ID, string _NameKR, string _Attributes, string _HP, string _HPRegeneration, string _AtkType, string _AtkDmg, string _AtkSpeed, string _AtkRange,
    string _CriticalDmg, string _CriticalChance, string _Armor, string _MoveSpeed, string _AbilityID)
    {
        heroID = _ID;
        heroNameKR = _NameKR;
        heroAttributes = _Attributes.Split(' ');
        heroMaxHP = _HP;
        heroHPRegeneration = _HPRegeneration;
        heroAtkType = _AtkType;
        heroAtkDmg = _AtkDmg;
        heroAtkSpeed = _AtkSpeed;
        heroAtkRange = _AtkRange;
        heroCriticalDmg = _CriticalDmg;
        heroCriticalChance = _CriticalChance;
        heroArmor = _Armor;
        heroMoveSpeed = _MoveSpeed;
        heroAbilities = _AbilityID.Split(',').ToList();
    }
    public string heroID;
    public string heroNameKR;
    public string[] heroAttributes;
    public string heroMaxHP;
    public string heroHPRegeneration;
    public string heroAtkType;
    public string heroAtkDmg;
    public string heroAtkSpeed;
    public string heroAtkRange;
    public string heroCriticalDmg;
    public string heroCriticalChance;
    public string heroArmor;
    public string heroMoveSpeed;
    public List<string> heroAbilities;
}

[System.Serializable]
public class Enemy
{
    public Enemy(string _ID, string _NameKR, string _HP, string _AtkType, string _CollisionDmg, string _AtkDmg, string _AtkSpeed, string _AtkRange, string _Armor, string _MoveType, string _MoveSpeed, string _AbilityID)
    {
        enemyID = _ID;
        enemyNameKR = _NameKR;
        enemyMaxHP = _HP;
        enemyAtkType = _AtkType;
        enemyCollisionDmg = _CollisionDmg;
        enemyAtkDmg = _AtkDmg;
        enemyAtkSpeed = _AtkSpeed;
        enemyAtkRange = _AtkRange;
        enemyArmor = _Armor;
        enemyMoveType = _MoveType;
        enemyMoveSpeed = _MoveSpeed;
        enemyAbilities = _AbilityID.Split(',').ToList();
    }
    public string enemyID;
    public string enemyNameKR;
    public string enemyMaxHP;
    public string enemyAtkType;
    public string enemyCollisionDmg;
    public string enemyAtkDmg;
    public string enemyAtkSpeed;
    public string enemyAtkRange;
    public string enemyArmor;
    public string enemyMoveType;
    public string enemyMoveSpeed;
    public List<string> enemyAbilities;
}

[System.Serializable]
public class Minion
{
    public Minion(string _ID, string _NameKR, string _Attributes, string _HP, string _AtkType, string _AtkDmg, string _AtkSpeed, string _AtkRange,
    string _CriticalDmg, string _CriticalChance, string _Armor, string _MoveType, string _MoveSpeed, string _AbilityID)
    {
        minionID = _ID;
        minionNameKR = _NameKR;
        minionAttributes = _Attributes.Split(' ');
        minionMaxHP = _HP;
        minionAtkType = _AtkType;
        minionAtkDmg = _AtkDmg;
        minionAtkSpeed = _AtkSpeed;
        minionAtkRange = _AtkRange;
        minionCriticalDmg = _CriticalDmg;
        minionCriticalChance = _CriticalChance;
        minionArmor = _Armor;
        minionMoveType = _MoveType;
        minionMoveSpeed = _MoveSpeed;
        minionAbilities = _AbilityID.Split(',').ToList();
    }
    public string minionID;
    public string minionNameKR;
    public string[] minionAttributes;
    public string minionMaxHP;
    public string minionAtkType;
    public string minionAtkDmg;
    public string minionAtkSpeed;
    public string minionAtkRange;
    public string minionCriticalDmg;
    public string minionCriticalChance;
    public string minionArmor;
    public string minionMoveType;
    public string minionMoveSpeed;
    public List<string> minionAbilities;
}

[System.Serializable]
public class Ability
{
    public Ability(string _ID, string _NameKR, string _Attributes, string _RareDegree, string _CoolTime, string _ExplainKR, string _RelatedMinion, string _RelatedTrap, string _RelatedStatus)
    {
        abilityID = _ID;
        abilityNameKR = _NameKR;
        abilityAttributes = _Attributes.Split(' ');
        abilityRareDegree = _RareDegree;
        abilityCoolTime = _CoolTime;
        abilityExplainKR = _ExplainKR;
        relatedMinion = _RelatedMinion.Split(',').ToList();
        relatedTrap = _RelatedTrap.Split(',').ToList();
        relatedStatus = _RelatedStatus.Split(',').ToList();
    }
    public string abilityID;
    public string abilityNameKR;
    public string[] abilityAttributes;
    public string abilityRareDegree;
    public string abilityCoolTime;
    public string abilityExplainKR;
    public List<string> relatedMinion;
    public List<string> relatedTrap;
    public List<string> relatedStatus;
}

[System.Serializable]
public class Status
{
    public Status(string _ID, string _NameKR, string _ExplainKR, string _buffStat, string _buffValue, string _buffTime)
    {
        statusID = _ID;
        statusNameKR = _NameKR;
        statusExplainKR = _ExplainKR;
        buffStat = _buffStat.Split('|');
        buffValue = _buffValue.Split('|');
        buffTime = _buffTime;
    }
    public string statusID;
    public string statusNameKR;
    public string statusExplainKR;
    public string[] buffStat;
    public string[] buffValue;
    public string buffTime;
}

[System.Serializable]
public class Stage
{
    public Stage(string _ID, string _stageNumber, string _stageInfo)
    {
        mapID = _ID;
        stageNumber = _stageNumber;
        stageInfo = _stageInfo;
    }
    public string mapID;
    public string stageNumber;
    public string stageInfo;
}

[System.Serializable]
public class Item
{
    public Item(string _ID, string _NameKR, string _Attributes, string _RareDegree, string _ExplainKR, string _buffStat, string _buffValue, string _AbilityID)
    {
        itemID = _ID;
        itemNameKR = _NameKR;
        itemAttributes = _Attributes.Split(' ');
        itemRareDegree = _RareDegree;
        itemExplainKR = _ExplainKR;
        itemBuffStat = _buffStat.Split('|');
        itemBuffValue = _buffValue.Split('|');
        itemAbilities = _AbilityID.Split(',').ToList();
    }
    public string itemID;
    public string itemNameKR;
    public string[] itemAttributes;
    public string itemRareDegree;
    public string itemExplainKR;
    public string[] itemBuffStat;
    public string[] itemBuffValue;
    public List<string> itemAbilities;
}

public class Trap
{
    public Trap(string _ID, string _NameKR, string _Dmg, string _Knockback, string _Range, string _status)
    {
        trapID = _ID;
        trapNameKR = _NameKR;
        trapDmg = _Dmg;
        trapKnockback = _Knockback;
        trapRange = _Range;
        trapStatus = _status.Split(',').ToList();
    }
    public string trapID;
    public string trapNameKR;
    public string trapDmg;
    public string trapKnockback;
    public string trapRange;
    public List<string> trapStatus;
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
    public TextAsset MinionDB;
    public TextAsset AbilityDB;
    public TextAsset StatusDB;
    public TextAsset StageDB;
    public TextAsset ItemDB;
    public TextAsset TrapDB;
    public static List<Hero> AllHeroList;
    public static List<Enemy> AllEnemyList;
    public static List<Minion> AllMinionList;
    public static List<Ability> AllAbilityList;
    public static List<Status> AllStatusList;
    public static List<Stage> AllStageList;
    public static List<Item> AllItemList;
    public static List<Trap> AllTrapList;

    public static int selectedHeroID = 0;

    void Start()
    {
        AllHeroList = new List<Hero>();
        string[] line = HeroDB.text.Substring(0, HeroDB.text.Length).Split('\r');
        for(int i=1;i<line.Length;i++)
        {
            string[] row = line[i].Split('\t');
            if(i == 0)
            {
                AllHeroList.Add(new Hero(row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10], row[11], row[12], row[13]));
            }
            else
            {
                AllHeroList.Add(new Hero(row[0].Substring(1), row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10], row[11], row[12], row[13]));
            }
        }

        AllEnemyList = new List<Enemy>();
        line = EnemyDB.text.Substring(0, EnemyDB.text.Length).Split('\r');
        for(int i=1;i<line.Length;i++)
        {
            string[] row = line[i].Split('\t');
            if(i == 0)
            {
                AllEnemyList.Add(new Enemy(row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10], row[11]));
            }
            else
            {
                AllEnemyList.Add(new Enemy(row[0].Substring(1), row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10], row[11]));
            }
        }

        AllMinionList = new List<Minion>();
        line = MinionDB.text.Substring(0, MinionDB.text.Length).Split('\r');
        for(int i=1;i<line.Length;i++)
        {
            string[] row = line[i].Split('\t');
            if(i == 0)
            {
                AllMinionList.Add(new Minion(row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10], row[11], row[12], row[13]));
            }
            else
            {
                AllMinionList.Add(new Minion(row[0].Substring(1), row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10], row[11], row[12], row[13]));
            }
        }

        AllAbilityList = new List<Ability>();
        line = AbilityDB.text.Substring(0, AbilityDB.text.Length).Split('\r');
        for(int i=1;i<line.Length;i++)
        {
            string[] row = line[i].Split('\t');
            if(i == 0)
            {
                AllAbilityList.Add(new Ability(row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8]));
            }
            else
            {
                AllAbilityList.Add(new Ability(row[0].Substring(1), row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8]));
            }
        }
        
        AllStatusList = new List<Status>();
        line = StatusDB.text.Substring(0, StatusDB.text.Length).Split('\r');
        for(int i=1; i<line.Length; i++)
        {
            string[] row = line[i].Split('\t');
            if(i == 0)
            {
                AllStatusList.Add(new Status(row[0], row[1], row[2], row[3], row[4], row[5]));
            }
            else
            {
                AllStatusList.Add(new Status(row[0].Substring(1), row[1], row[2], row[3], row[4], row[5]));
            }
        }

        AllStageList = new List<Stage>();
        line = StageDB.text.Substring(0, StageDB.text.Length).Split('\r');
        for(int i=1; i<line.Length; i++)
        {
            string[] row = line[i].Split('\t');
            if(i == 0)
            {
                AllStageList.Add(new Stage(row[0], row[1], row[2]));
            }
            else
            {
                AllStageList.Add(new Stage(row[0].Substring(1), row[1], row[2]));
            }
        }

        AllItemList = new List<Item>();
        line = ItemDB.text.Substring(0, ItemDB.text.Length).Split('\r');
        for(int i=1;i<line.Length;i++)
        {
            string[] row = line[i].Split('\t');
            if(i == 0)
            {
                AllItemList.Add(new Item(row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7]));
            }
            else
            {
                AllItemList.Add(new Item(row[0].Substring(1), row[1], row[2], row[3], row[4], row[5], row[6], row[7]));
            }
        }

        AllTrapList = new List<Trap>();
        line = TrapDB.text.Substring(0, TrapDB.text.Length).Split('\r');
        for(int i=1;i<line.Length;i++)
        {
            string[] row = line[i].Split('\t');
            if(i == 0)
            {
                AllTrapList.Add(new Trap(row[0], row[1], row[2], row[3], row[4], row[5]));
            }
            else
            {
                AllTrapList.Add(new Trap(row[0].Substring(1), row[1], row[2], row[3], row[4], row[5]));
            }
        }
    }

}
