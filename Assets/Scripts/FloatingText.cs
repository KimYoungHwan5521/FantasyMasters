using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    private float moveSpeed;
    private float alphaSpeed;
    private float destroyTime;
    Text text;
    Color alpha;
    public float damage;
    public bool isCritical = false;

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 4.0f;
        alphaSpeed = 2.0f;
        destroyTime = 2.0f;

        text = GetComponent<Text>();
        text.text = Mathf.Round(damage).ToString();
        if(damage == 0)
        {
            alpha = Color.white;
        }
        else
        {
            if(isCritical) ColorUtility.TryParseHtmlString("#FF0000", out alpha);
            else ColorUtility.TryParseHtmlString("#FF9999", out alpha);
        }
        text.color = alpha;
        Invoke("DestroyObject", destroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0)); // 텍스트 위치
        
        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed); // 텍스트 알파값
        text.color = alpha;
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}