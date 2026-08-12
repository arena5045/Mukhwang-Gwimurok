using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    void Awake()
    {
        // ScreenFader는 현재 씬의 fadeImage를 직접 사용한다.
        // 씬을 넘어 유지하지 않으며, 중복 배치된 객체만 제거한다.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        // 다음 씬에서 새 ScreenFader가 자신을 등록할 수 있도록 참조를 비운다.
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        // 시작 시 페이드 인
        FadeIn();
    }

    public void FadeIn(System.Action onComplete = null)
    {
        fadeImage.DOFade(0, fadeDuration).OnComplete(() => onComplete?.Invoke());
    }

    public void FadeOut(System.Action onComplete = null)
    {
        fadeImage.DOFade(1, fadeDuration).OnComplete(() => onComplete?.Invoke());
    }

    public void FadeAtoB(GameObject offOb, GameObject onOb)
    {
        if (!fadeImage.gameObject.activeSelf)
            fadeImage.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();

        seq.Append(fadeImage.DOFade(1, fadeDuration));
        seq.AppendCallback(() =>
        {
            offOb?.SetActive(false);
            onOb?.SetActive(true);
        });
        seq.Append(fadeImage.DOFade(0, fadeDuration));
        seq.OnComplete(() => fadeImage.gameObject.SetActive(false));

    }

    public void FadeAtoB(GameObject offOb, GameObject onOb, string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // 투명도 초기화

        Sequence seq = DOTween.Sequence();
        // 이미지가 파괴되면 시퀀스도 즉시 멈춰서 오류를 방지합니다.
        seq.SetLink(fadeImage.gameObject, LinkBehaviour.KillOnDestroy);

        // 1. 공통: 화면 어둡게 만들기
        seq.Append(fadeImage.DOFade(1, fadeDuration));

        // 2. 화면이 가려진 시점에 할 일들
        seq.AppendCallback(() => {
            offOb?.SetActive(false);
            onOb?.SetActive(true);

            // 씬 이동이 필요한 경우 여기서 씬 로드 시작
            if (!string.IsNullOrEmpty(sceneName))
            {
                // 핵심: 다음 씬으로 넘어가기 직전에 이 이미지와 관련된 모든 트윈을 완전히 파괴합니다.
                fadeImage.DOKill();
                seq.Kill();

                FadeOutAndLoadScene(sceneName);
                // 씬이 이동할 때는 여기서 시퀀스를 멈추거나 
                // 새 씬에서 별도로 FadeOut을 해주는게 일반적입니다.
            }
        });

        // 3. 씬 이동이 아닐 때만 다시 화면 밝게 만들기
        if (string.IsNullOrEmpty(sceneName))
        {
            seq.Append(fadeImage.DOFade(0, fadeDuration));
            seq.OnComplete(() => fadeImage.gameObject.SetActive(false));
        }
    }

    public void FadeOutAndLoadScene(string sceneName)
    {
        FadeOut(() =>
        {
            // 1. 이벤트를 먼저 구독 (로드 후에 실행될 약속을 먼저 잡음)
            //SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("실행");
            // 2. 그 다음 씬 로드 실행
            SceneManager.LoadScene(sceneName);
        });
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드가 완료되었으니 페이드 인 시작
        FadeIn();

        // 중요: 한 번 실행했으니 반드시 구독 해제 (안 하면 다음 씬 로드 때 또 실행됨)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
