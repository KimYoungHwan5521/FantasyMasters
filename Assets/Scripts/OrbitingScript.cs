using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingScript : MonoBehaviour
{
    SoundManager SoundManager;
    public GameObject target;
    public float orbitalDistance = 1;
    public float orbitingSpeed = 200;
    public float deg = 0;
    public float circleR = 1;
    public float dmg;
    public bool ignoreArmor;
    public string orbitingSound;
    
    public float limit = 60;
    // Start is called before the first frame update
    void Start()
    {
        SoundManager = GameObject.Find("DataManager").GetComponent<SoundManager>();
    }

    public void SetOrbiting(GameObject _target, float _orbitalDistance, float _summonDeg, float _dmg, bool _ignoreArmor = false)
    {
        target = _target;
        orbitalDistance = _orbitalDistance;
        deg = _summonDeg;
        dmg = _dmg;
        ignoreArmor = _ignoreArmor;
        StartCoroutine(Orbit());
    }

    // Update is called once per frame
    IEnumerator Orbit()
    {
        while(target || limit > 0)
        {
            if(deg < 360)
            {
                deg += Time.deltaTime * orbitingSpeed;
                var rad = Mathf.Deg2Rad * (deg);
                var x = orbitalDistance * Mathf.Sin(rad);
                var y = orbitalDistance * Mathf.Cos(rad);
                transform.parent.transform.position = target.GetComponent<Collider2D>().bounds.center + new Vector3(x, y);
                transform.rotation = Quaternion.Euler(0, 0, deg * -1);
                transform.position = transform.parent.transform.position;
            }
            else deg = 0;
            yield return null;
        }
    }

    void Update()
    {
        limit -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == "Enemy")
        {
            AudioClip s = null;
            if(orbitingSound == "Blow") s = Resources.Load<AudioClip>("Sounds/SE/hits/18");
            else if(orbitingSound == "Blade") s = Resources.Load<AudioClip>("Sounds/SE/RPG_Essentials_Free/10_Battle_SFX/77_flesh_02");
            if(s != null) SoundManager.PlaySE(s);
            if(ignoreArmor) col.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg + col.gameObject.GetComponent<EnemyScript>().enemyArmor, 0);
            else col.gameObject.GetComponent<EnemyScript>().BeAttacked(dmg, 0);
        }
    }
}
