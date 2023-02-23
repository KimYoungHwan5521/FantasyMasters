using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public class Hero
{
    public Hero(string _ID, string _NameKR, string _Attributes, string _HP, string _AtkDmg, string _AtkSpeed, string _Armor, string _MoveSpeed, string _AbilityKR)
    {
        heroID = _ID;
        heroNameKR = _NameKR;
        heroAttributes = _Attributes.Split(' ');
        heroMaxHP = _HP;
        heroAtkDmg = _AtkDmg;
        heroAtkSpeed = _AtkSpeed;
        heroArmor = _Armor;
        heroMoveSpeed = _MoveSpeed;
        heroAbilityKR = _AbilityKR;
    }
    public string heroID;
    public string heroNameKR;
    public string[] heroAttributes;
    public string heroMaxHP;
    public string heroAtkDmg;
    public string heroAtkSpeed;
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
                AllHeroList.Add(new Hero(row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8]));
            }
            else
            {
                AllHeroList.Add(new Hero(row[0].Substring(1), row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8]));
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
                        heroInfoSimple[1].text += "<color=red>숲</color>";   
                    }
                    else if(CurHeroList[i].heroAttributes[j] == "대지")
                    {
                        heroInfoSimple[1].text += "<color=red>대지</color>";   
                    }
                    else if(CurHeroList[i].heroAttributes[j] == "금속")
                    {
                        heroInfoSimple[1].text += "<color=red>금속</color>";   
                    }
                    else if(CurHeroList[i].heroAttributes[j] == "빛")
                    {
                        heroInfoSimple[1].text += "<color=red>빛</color>";   
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
