using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrapScript : MonoBehaviour
{
    Animator animator;
    DataManager DataManager;
    SoundManager SoundManager;

    public string _trapID;
    public float trapDmg;
    public float trapKnockback;
    public float trapRange;
    public List<string> trapStatus;
    public string trapSound;
    // Start is called before the first frame update
    void Start()
    {
        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        SoundManager = GameObject.Find("DataManager").GetComponent<SoundManager>();
        animator = gameObject.GetComponent<Animator>();

        Trap trapInfo = DataManager.AllTrapList.Find(x => x.trapID == _trapID);
        trapDmg = float.Parse(trapInfo.trapDmg);
        trapKnockback = float.Parse(trapInfo.trapKnockback);
        trapRange = float.Parse(trapInfo.trapRange);
        trapStatus = trapInfo.trapStatus.ToList();
    }

    
    public bool activated = false;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && !activated)
        {
            AudioClip s = null;
            if(trapSound == "BearTrap") s = Resources.Load<AudioClip>("Sounds/SE/Socapex - Evol Online SFX - Punches and hits/Socapex - new_hits");
            else if(trapSound == "PoisonousMushroom") s = Resources.Load<AudioClip>("Sounds/SE/acid_burn/acid burn small");
            else if(trapSound == "SpikeTrap") s = Resources.Load<AudioClip>("Sounds/SE/Fantasy Sound Library/Mp3/Trap_00");
            if(s != null) SoundManager.PlaySE(s);
            animator.SetTrigger("Activate");
            Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(GetComponent<Collider2D>().bounds.center, trapRange);
            foreach(Collider2D collider in collider2Ds)
            {
                if(collider.tag == "Enemy")
                {
                    collider.GetComponent<EnemyScript>().BeAttacked(trapDmg, trapKnockback);
                    if(trapStatus[0] != "")
                    {
                        for(int i=0; i<trapStatus.Count; i++)
                        {
                            collider.GetComponent<EnemyScript>().AddStatus(trapStatus[i]);
                        }
                    }
                }
            }
            activated = true;
        }
    }

    public void ActivateReset()
    {
        activated = false;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetComponent<Collider2D>().bounds.center, trapRange * 0.5f);
    }
}
