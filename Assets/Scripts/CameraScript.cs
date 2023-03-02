using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    GameObject Hero;
    // Camera _Camera;
    public GameObject Map;
    public float scrollSpeed = 2000.0f;
    public float speed = 100.0f;
    public float z;
    float height;
    float width;
    Vector2 center;

    void Start()
    {
        // _Camera = GameObject.Find("MainCamera").GetComponent<Camera>();
        z = transform.position.z;
        height = Camera.main.orthographicSize;
        width = height * Screen.width / Screen.height;
    }

    void LateUpdate()
    {
        // float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        // _Camera.orthographicSize += scrollWheel * Time.deltaTime * scrollSpeed;

        Hero = GameObject.FindWithTag("Player");
        transform.position = Vector3.Lerp(Hero.transform.position, Hero.transform.position, Time.deltaTime * speed);

        // Mathf.Clamp() : 유니티에선 변수가 일정한 값을 벗어나지 못하도록 범위를 제한하는 함수
        float lx = Map.transform.localScale.x * 0.5f - width;
        float clampX = Mathf.Clamp(transform.position.x, -lx + center.x, lx + center.x);

        float ly = Map.transform.localScale.y * 0.5f - height;
        float clampY = Mathf.Clamp(transform.position.y, -ly + center.y, ly + center.y);
        transform.position = new Vector3(clampX, clampY, -10f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, Map.transform.localScale);
    }
}
