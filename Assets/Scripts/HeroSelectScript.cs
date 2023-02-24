using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

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

public class HeroSelectScript : MonoBehaviour
{
    public TextAsset HeroDB;
    public List<Hero> AllHeroList, CurHeroList;

    public GameObject HeroSelect;
    public int selectedHeroID;
    public Outline[] HeroOutlines;
    NoticeUI _notice;

    void Awake()
    {
        _notice = FindObjectOfType<NoticeUI>();
    }

    void Start()
    {
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
        CurHeroList = AllHeroList.ToList();
    }

    public GameObject[] HerosSimple;
    void Update()
    {
        for(int i=0; i < HerosSimple.Length; i++)
        {
            HerosSimple[i].SetActive(i < CurHeroList.Count);
            Text[] heroInfoSimple = HerosSimple[i].GetComponentsInChildren<Text>();
            heroInfoSimple[0].text = i < CurHeroList.Count ? CurHeroList[i].heroNameKR : "";
            heroInfoSimple[1].text = "";
            if(i < CurHeroList.Count)
            {
                for(int j=0; j < CurHeroList[i].heroAttributes.Length; j++)
                {
                    if(j > 0)
                    {
                        heroInfoSimple[1].text += " ";
                    }
                    if(CurHeroList[i].heroAttributes[j] == "암흑")
                    {
                        heroInfoSimple[1].text += "<color=purple>암흑</color>";   
                    }
                    else if(CurHeroList[i].heroAttributes[j] == "불")
                    {
                        heroInfoSimple[1].text += "<color=red>불</color>";   
                    }
                    else if(CurHeroList[i].heroAttributes[j] == "물")
                    {
                        heroInfoSimple[1].text += "<color=blue>물</color>";   
                    }
                    else if(CurHeroList[i].heroAttributes[j] == "숲")
                    {
                        heroInfoSimple[1].text += "<color=green>숲</color>";   
                    }
                    else if(CurHeroList[i].heroAttributes[j] == "대지")
                    {
                        heroInfoSimple[1].text += "<olive=red>대지</color>";   
                    }
                    else if(CurHeroList[i].heroAttributes[j] == "금속")
                    {
                        heroInfoSimple[1].text += "<color=silver>금속</color>";   
                    }
                    else if(CurHeroList[i].heroAttributes[j] == "빛")
                    {
                        heroInfoSimple[1].text += "<color=yellow>빛</color>";   
                    }
                    else
                    {
                        heroInfoSimple[1].text += CurHeroList[i].heroAttributes[j];   
                    }
                }

            }
        }
    }

    public void OnClickGameStart()
    {
        HeroSelect.SetActive(true);
    }

    public void OnClickBack()
    {
        HeroSelect.SetActive(false);
    }

    public GameObject HeroDetails;
    public Text[] HeroDetailValues;
    public void MouseEnterToHero(int _heroID)
    {
        HeroDetailValues[0].text = CurHeroList[_heroID].heroNameKR;
        HeroDetailValues[1].text = "";
        for(int j=0; j < CurHeroList[_heroID].heroAttributes.Length; j++)
        {
            if(j > 0)
            {
                HeroDetailValues[1].text += " ";
            }
            if(CurHeroList[_heroID].heroAttributes[j] == "암흑")
            {
                HeroDetailValues[1].text += "<color=purple>암흑</color>";   
            }
            else if(CurHeroList[_heroID].heroAttributes[j] == "불")
            {
                HeroDetailValues[1].text += "<color=red>불</color>";   
            }
            else if(CurHeroList[_heroID].heroAttributes[j] == "물")
            {
                HeroDetailValues[1].text += "<color=blue>물</color>";   
            }
            else if(CurHeroList[_heroID].heroAttributes[j] == "숲")
            {
                HeroDetailValues[1].text += "<color=green>숲</color>";   
            }
            else if(CurHeroList[_heroID].heroAttributes[j] == "대지")
            {
                HeroDetailValues[1].text += "<olive=red>대지</color>";   
            }
            else if(CurHeroList[_heroID].heroAttributes[j] == "금속")
            {
                HeroDetailValues[1].text += "<color=silver>금속</color>";   
            }
            else if(CurHeroList[_heroID].heroAttributes[j] == "빛")
            {
                HeroDetailValues[1].text += "<color=yellow>빛</color>";   
            }
            else
            {
                HeroDetailValues[1].text += CurHeroList[_heroID].heroAttributes[j];   
            }
        }
        HeroDetailValues[2].text = CurHeroList[_heroID].heroMaxHP;
        HeroDetailValues[3].text = CurHeroList[_heroID].heroHPRegeneration;
        HeroDetailValues[4].text = CurHeroList[_heroID].heroAtkDmg;
        HeroDetailValues[5].text = CurHeroList[_heroID].heroAtkSpeed;
        HeroDetailValues[6].text = CurHeroList[_heroID].heroAtkRange;
        HeroDetailValues[7].text = "x" +CurHeroList[_heroID].heroCriticalDmg;
        HeroDetailValues[8].text = CurHeroList[_heroID].heroCriticalChance + "%";
        HeroDetailValues[9].text = CurHeroList[_heroID].heroArmor;
        HeroDetailValues[10].text = CurHeroList[_heroID].heroMoveSpeed;
        HeroDetailValues[11].text = CurHeroList[_heroID].heroAbilityKR;

        RectTransform Parent;
        Parent = HerosSimple[_heroID].transform.parent.GetComponent<RectTransform>();
        RectTransform HSRT;
        HSRT = HerosSimple[_heroID].GetComponent<RectTransform>();
        RectTransform HDRT;
        HDRT = HeroDetails.GetComponent<RectTransform>();
        if(HSRT.anchoredPosition.x < -Parent.anchoredPosition.x)
        {
            HDRT.anchoredPosition = new Vector2((Parent.anchoredPosition.x + HDRT.rect.width) * 0.5f + 160, HDRT.anchoredPosition.y);
        }
        else
        {
            HDRT.anchoredPosition = new Vector2((Parent.anchoredPosition.x - HDRT.rect.width) * 0.5f + 160, HDRT.anchoredPosition.y);
        }
        HeroDetails.SetActive(true);
    }

    public void MouseExitToHero()
    {
        HeroDetails.SetActive(false);
    }

    public void SelectHero(int _heroID)
    {
        selectedHeroID = _heroID;
        for(int i=0; i < HeroOutlines.Length; i++)
        {
            if(i == _heroID)
            {
                HeroOutlines[i].effectColor = new Color(0, 1, 0, 1);
                HeroOutlines[i].effectDistance = new Vector2(3, -3);
            }
            else
            {
                HeroOutlines[i].effectColor = new Color(0, 0, 0, 1);
                HeroOutlines[i].effectDistance = new Vector2(1, -1);
            }
        }
    }

    public void OnClickSelect()
    {
        if(selectedHeroID == -1)
        {
            _notice.SUB("캐릭터를 선택해주세요");
        }
        else
        {
            SceneManager.LoadScene("MainGameScene");
        }
    }

}
