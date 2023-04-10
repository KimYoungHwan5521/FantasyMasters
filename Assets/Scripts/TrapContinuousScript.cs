using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrapContinuousScript : MonoBehaviour
{
    Animator animator;
    DataManager DataManager;

    public string _trapID;
    public float trapDmg;
    public float trapKnockback;
    public float trapRange;
    public Vector2 boxSize;
    public List<string> trapStatus;
    // Start is called before the first frame update
    void Start()
    {
        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        animator = gameObject.GetComponent<Animator>();

        Trap trapInfo = DataManager.AllTrapList.Find(x => x.trapID == _trapID);
        trapDmg = float.Parse(trapInfo.trapDmg);
        trapKnockback = float.Parse(trapInfo.trapKnockback);
        trapRange = float.Parse(trapInfo.trapRange);
        boxSize = new Vector2(trapRange, trapRange);
        trapStatus = trapInfo.trapStatus.ToList();
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            if(_trapID == "0002")
            {
                collision.gameObject.GetComponent<EnemyScript>().moveSpeedCVM *= 0.5f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            if(_trapID == "0002")
            {
                collision.gameObject.GetComponent<EnemyScript>().moveSpeedCVM *= 2f;
            }
        }
    }
}
