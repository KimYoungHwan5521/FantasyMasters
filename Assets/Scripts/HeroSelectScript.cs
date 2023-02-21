using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HeroSelectScript : MonoBehaviour
{
    public GameObject HeroSelect;
    public int heroID;
    public Outline[] HeroOutlines;
    NoticeUI _notice;

    void Awake()
    {
        _notice = FindObjectOfType<NoticeUI>();
    }

    void Update()
    {
        // print(heroID);
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
        heroID = _heroID;
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
        if(heroID == -1)
        {
            _notice.SUB("캐릭터를 선택해주세요");
        }
        else
        {
            SceneManager.LoadScene("MainGameScene");
        }
    }

}
