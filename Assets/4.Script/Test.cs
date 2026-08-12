using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        // 개발 편의를 위한 임시 씬 이동이다. Player 빌드에는 SampleScene이 포함되지 않을 수 있으므로
        // 에디터 전용 조건부 컴파일로 감싸 모바일에서 실행 경로 자체가 생성되지 않게 한다.
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            LoadingManager.Instance.LoadScene("SampleScene");
        }
#endif
    }
}
