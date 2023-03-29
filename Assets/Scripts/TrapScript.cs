using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrapScript : MonoBehaviour
{
    Animator animator;
    DataManager DataManager;

    public string _trapID;
    public float trapDmg;
    public List<string> trapStatus;
    // Start is called before the first frame update
    void Start()
    {
        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        animator = gameObject.GetComponent<Animator>();

        Trap trapInfo = DataManager.AllTrapList.Find(x => x.trapID == _trapID);
        trapDmg = float.Parse(trapInfo.trapDmg);
        trapStatus = trapInfo.trapStatus.ToList();
    }


    bool activated = false;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && !activated)
        {
            collision.gameObject.GetComponent<EnemyScript>().BeAttacked(trapDmg, 0);
            for(int i=0; i<trapStatus.Count; i++)
            {
                collision.gameObject.GetComponent<EnemyScript>().AddStatus(trapStatus[i]);
            }
            animator.SetBool("Activate", true);
            activated = false;
        }
    }
}
