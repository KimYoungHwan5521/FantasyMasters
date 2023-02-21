using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    Camera _Camera;
    public float scrollSpeed = 2000.0f;
    // Start is called before the first frame update
    void Start()
    {
        _Camera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        _Camera.orthographicSize += scrollWheel * Time.deltaTime * scrollSpeed;
    }
}
