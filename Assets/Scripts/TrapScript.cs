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

    
    public bool activated = false;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && !activated)
        {
            animator.SetBool("Activate", true);
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(GetComponent<Collider2D>().bounds.center, boxSize, 0);
            foreach(Collider2D collider in collider2Ds)
            {
                if(collider.tag == "Enemy")
                {
                    collider.GetComponent<EnemyScript>().BeAttacked(trapDmg, trapKnockback);
                    for(int i=0; i<trapStatus.Count; i++)
                    {
                        collider.GetComponent<EnemyScript>().AddStatus(trapStatus[i]);
                    }
                }
            }
            activated = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetComponent<Collider2D>().bounds.center, boxSize);
    }
}
