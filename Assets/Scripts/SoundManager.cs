using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private void Awake()
    {
        GameObject root = GameObject.Find("@Sound");
        if(root == null)
        {
            root = new GameObject {name = "@Sound"};
            DontDestroyOnLoad(root);

            GameObject go = new GameObject { name = "BGM" }; 
            go.AddComponent<AudioSource>();
            go.transform.parent = root.transform;

            GameObject go2 = new GameObject { name = "SE" }; 
            go2.AddComponent<AudioSource>();
            go2.transform.parent = root.transform;

            GameObject go3 = new GameObject { name = "SE2" }; 
            go3.AddComponent<AudioSource>();
            go3.transform.parent = root.transform;
        }
        
    }

    AudioSource BGM;
    AudioSource SE;
    AudioSource SE2;
    void Start()
    {
        BGM = GameObject.Find("BGM").GetComponent<AudioSource>();
        SE = GameObject.Find("SE").GetComponent<AudioSource>();
        SE2 = GameObject.Find("SE2").GetComponent<AudioSource>();
    }

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        BGM.Stop();
        BGM.clip = clip;
        BGM.loop = loop;
        BGM.time = 0;
        BGM.Play();
    }

    public void BGMStop()
    {
        BGM.Stop();
    }

    public void PlaySE(AudioClip clip, bool loop = false)
    {
        SE.clip = clip;
        SE.loop = loop;
        SE.time = 0;
        SE.PlayOneShot(clip);
    }

    public void PlaySE2(AudioClip clip, bool loop = false)
    {
        SE2.Stop();
        SE2.clip = clip;
        SE2.loop = loop;
        SE2.time = 0;
        SE2.Play();
    }

    public void SEStop()
    {
        SE.Stop();
        SE2.Stop();
    }
}
