using UnityEngine;

public class FirstFade : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 0.5초뒤에 자신오브젝트 숨김
        Invoke("HideSelf", 0.5f);
    }
    void HideSelf()
    {
        gameObject.SetActive(false);
    }

}
