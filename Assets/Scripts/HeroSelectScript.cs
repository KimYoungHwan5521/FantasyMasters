using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class HeroSelectScript : MonoBehaviour
{
    public List<Hero> CurHeroList;

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
        for(int i=0; i < searchAttributeIsChecked.Length; i++)
        {
            searchAttributeIsChecked[i] = false;
        }
    }

    public GameObject[] HerosSimple;
    public Button[] SearchAttributeButton;
    public GameObject[] SearchAttributeIsChecked;
    void Update()
    {
        // SearchOptions Attribute Check
        CurHeroList = new List<Hero>();
        int cntChks = 0;
        for(int i=0; i < searchAttributeIsChecked.Length; i++)
        {
            if(searchAttributeIsChecked[i])
            {
                SearchAttributeIsChecked[i].SetActive(true);
                if(i != 8) cntChks++;
            }
            else
            {
                SearchAttributeIsChecked[i].SetActive(false);
            }
        }
        if(searchAttributeIsChecked[8])
        {
            for(int i=0; i < searchAttributeIsChecked.Length; i++)
            {
                if(!searchAttributeIsChecked[i]) SearchAttributeButton[i].interactable = false;
            }
        }
        else
        {
            for(int i=0; i< searchAttributeIsChecked.Length; i++)
            {
                SearchAttributeButton[i].interactable = true;
            }
        }
        if(cntChks != 1) 
        {
            searchAttributeIsChecked[8] = false;
            SearchAttributeButton[8].interactable = false;
        }
        else SearchAttributeButton[8].interactable = true;

        // make CurHeroList as SearchOptions
        if(cntChks != 0)
        {
            List<string> stringAttribute = new List<string>();
            string[] temp = new string[] {"암흑", "불", "물", "숲", "금속", "대지", "빛", "무속성"};
            if(!searchAttributeIsChecked[8])
            {
                for(int i=0; i < searchAttributeIsChecked.Length - 1; i++)
                {   
                    if(searchAttributeIsChecked[i]) 
                    {
                        stringAttribute.Add(temp[i]);
                    }
                }
                for(int i=0; i < DataManager.AllHeroList.Count; i++)
                {
                    for(int j=0; j < stringAttribute.Count; j++)
                    {
                        if(Array.Exists(DataManager.AllHeroList[i].heroAttributes, x => x == stringAttribute[j]))
                        {
                            CurHeroList.Add(DataManager.AllHeroList[i]);
                            break;
                        }
                    }
                }
            }
            else
            {
                for(int i=0; i < searchAttributeIsChecked.Length - 1; i++)
                {
                    if(searchAttributeIsChecked[i]) 
                    {
                        stringAttribute.Add(temp[i]);
                        break;
                    }
                }
                for(int i=0; i < DataManager.AllHeroList.Count; i++)
                {
                    for(int j=0; j < stringAttribute.Count; j++)
                    {
                        if(DataManager.AllHeroList[i].heroAttributes.Length == 1 && Array.Exists(DataManager.AllHeroList[i].heroAttributes, x => x == stringAttribute[j])) CurHeroList.Add(DataManager.AllHeroList[i]);
                    }
                }
            }
        }
        else
        {
            CurHeroList = DataManager.AllHeroList.ToList();
        }
        

        // show result of CurHeroList
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
        HeroDetailValues[11].text = "";
        HeroDetailValues[12].text = "";
        for(int i=0; i<CurHeroList[_heroID].heroAbilities.Count; i++)
        {
            if(i>0) HeroDetailValues[11].text += "/";
            HeroDetailValues[11].text += DataManager.AllAbilityList.Find(x => x.abilityID == CurHeroList[_heroID].heroAbilities[i]).abilityNameKR;
        }
        for(int i=0; i<CurHeroList[_heroID].heroAbilities.Count; i++)
        {
            if(i>0) HeroDetailValues[12].text += "/\n";
            HeroDetailValues[12].text += DataManager.AllAbilityList.Find(x => x.abilityID == CurHeroList[_heroID].heroAbilities[i]).abilityExplainKR;
        }

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

    bool[] searchAttributeIsChecked = new bool[9];
    public void CheckSearchOptionAttributes(int _AttributeID)
    {
        if(searchAttributeIsChecked[_AttributeID])
        {
            searchAttributeIsChecked[_AttributeID] = false;
        }
        else
        {
            searchAttributeIsChecked[_AttributeID] = true;
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
            DataManager.selectedHeroID = selectedHeroID;
            SceneManager.LoadScene("MainGameScene");
        }
    }

}
