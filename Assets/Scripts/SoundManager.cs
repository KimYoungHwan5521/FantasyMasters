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
        }
        
    }

    AudioSource BGM;
    AudioSource SE;
    void Start()
    {
        BGM = GameObject.Find("BGM").GetComponent<AudioSource>();
        SE = GameObject.Find("SE").GetComponent<AudioSource>();
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
        SE.Stop();
        SE.clip = clip;
        SE.loop = loop;
        SE.time = 0;
        SE.PlayOneShot(clip);
    }

    public void SEStop()
    {
        SE.Stop();
    }
}
