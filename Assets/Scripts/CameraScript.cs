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
    }

    private float temp_value;
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * speed;

        // scroll < 0 : scroll down하면 줌인
        if (gameObject.GetComponent<Camera>().orthographicSize - scroll * 0.01f < 2f && scroll > 0)
        {
            temp_value = gameObject.GetComponent<Camera>().orthographicSize;
            gameObject.GetComponent<Camera>().orthographicSize = temp_value; // maximize zoom in

           // 최대로 Zoom in 했을 때, 특정 값을 지정했을 때 최대 줌 인 범위를 벗어날 때 값에 맞추려고 한번 줌 아웃 되는 현상을 방지
        }

        // scroll > 0 : scroll up하면 줌아웃
        else if (gameObject.GetComponent<Camera>().orthographicSize - scroll * 0.01f > 5f && scroll < 0)
        {
            temp_value = gameObject.GetComponent<Camera>().orthographicSize;
            gameObject.GetComponent<Camera>().orthographicSize = temp_value; // maximize zoom out
        }
        else
            gameObject.GetComponent<Camera>().orthographicSize -= scroll * 0.01f;
    }

    void LateUpdate()
    {
        Hero = GameObject.FindWithTag("Player");
        transform.position = Vector3.Lerp(Hero.GetComponent<BoxCollider2D>().bounds.center, Hero.GetComponent<BoxCollider2D>().bounds.center, Time.deltaTime * speed);

        height = Camera.main.orthographicSize;
        width = height * Screen.width / Screen.height;
        // Mathf.Clamp() : 변수가 일정한 값을 벗어나지 못하도록 범위를 제한하는 함수
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
