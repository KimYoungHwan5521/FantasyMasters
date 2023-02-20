using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeSelectScript : MonoBehaviour
{
    public GameObject HeroSelect;

    public void OnClickGameStart()
    {
        HeroSelect.SetActive(true);
    }

    public void OnClickBack()
    {
        HeroSelect.SetActive(false);
    }

    public void SelectType(int heroID)
    {
    }

}
