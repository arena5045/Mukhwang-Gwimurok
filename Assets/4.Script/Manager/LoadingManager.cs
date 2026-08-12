using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{

    private static LoadingManager _instance;
    public static LoadingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 자동 생성: Resources에서 프리팹 로드
                GameObject prefab = Resources.Load<GameObject>("LoadingManager");
                if (prefab == null)
                {
                    Debug.LogError("LoadingManager prefab not found in Resources folder.");
                    return null;
                }

                GameObject obj = Instantiate(prefab);
                _instance = obj.GetComponent<LoadingManager>();
            }

            return _instance;
        }
    }

    [Header("로딩 ui")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Slider progressBar;

    [Header("페이드 이미지")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;


    void Awake()
    {
        // 로딩 화면은 씬 사이를 실제로 연결해야 하므로 전체 매니저 중 유일하게 영속화한다.
        // 이후 씬에 중복 생성된 LoadingManager는 기존 로딩 코루틴과 UI를 보호하기 위해 제거한다.
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        loadingPanel.SetActive(false);
        fadeImage?.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // 애플리케이션 종료나 명시적 파괴 뒤 Instance가 파괴된 객체를 반환하지 않게 한다.
        if (_instance == this)
        {
            _instance = null;
        }
    }


    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // 1. 페이드 아웃: 화면을 천천히 어둡게 만들기
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // 검정 배경 활성화
            fadeImage.color = new Color(0, 0, 0, 0); // 투명하게 시작
            yield return fadeImage.DOFade(1f, fadeDuration).WaitForCompletion(); // 천천히 검정색으로
        }

        // 2. 로딩 UI 보여주기
        loadingPanel.SetActive(true);                // 로딩 패널 켜기
        loadingText.text = "준비중...";             // 텍스트 표시
        progressBar.value = 0f;                      // 바 초기화

        // 3. 씬 비동기 로딩 시작 (자동 전환은 잠시 막아둠)
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;             // 완료되더라도 자동으로 전환되지 않도록 막음

        float displayProgress = 0f;
        progressBar.value = 0f;

        // 4. 진행도 표시 (0.99까지만 올라감)
        while (op.progress < 0.9f)
        {
            // 실제 진행도와 슬라이더를 부드럽게 맞춤
            displayProgress = Mathf.MoveTowards(displayProgress, op.progress, Time.deltaTime * 3f);
            progressBar.value = displayProgress;
            yield return null;
        }

        // 5. 마지막 0.99 → 1.0 표시 및 완료 텍스트

        while (displayProgress < 1f)
        {
            displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.deltaTime * 3f);
            progressBar.value = displayProgress;
            yield return null;
        }

       // progressBar.value = 1f;
        loadingText.text = "준비 완료!";
        yield return new WaitForSeconds(0.3f); // 약간의 연출 대기 시간

        // 6. 이제 씬 전환 허용
        op.allowSceneActivation = true;

        // 7. 새로운 씬이 완전히 로드되면, 후처리 진행
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    // 씬 로드 완료 후 실행되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("✅ OnSceneLoaded 호출됨: " + scene.name);

        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 로딩 UI 끄기
        loadingPanel.SetActive(false);

        // 페이드 인: 화면 밝아지기
        if (fadeImage != null)
        {
            fadeImage.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                fadeImage.gameObject.SetActive(false);
            });
        }
    }

}
