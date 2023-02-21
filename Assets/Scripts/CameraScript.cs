using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject Hero;
    Camera _Camera;
    public float scrollSpeed = 2000.0f;
    public float speed = 100.0f;
    public float z;

    void Start()
    {
        _Camera = GameObject.Find("CameraUI").GetComponent<Camera>();
        z = transform.position.z;
    }

    void LateUpdate()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        _Camera.orthographicSize += scrollWheel * Time.deltaTime * scrollSpeed;

        transform.position = Vector3.Lerp(Hero.transform.position, Hero.transform.position, Time.deltaTime * speed);
        transform.position = new Vector3(Hero.transform.position.x, Hero.transform.position.y, z);
        // print($"Hero.transform: {Hero.transform.position.x}");
    }
}
