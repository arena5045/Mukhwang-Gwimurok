using UnityEngine;

/// <summary>
/// 인게임의 '상단 ui' 묶음을 Android 상단 Safe Area 아래로 이동시킨다.
///
/// 이 컴포넌트는 자신이 붙은 RectTransform 하나만 이동한다. 따라서 그 아래에 들어 있는
/// 상단 바 배경, 능력치 HUD, 지역 제목, 맵 버튼과 우측 아이콘의 상대 위치는 바뀌지 않으며,
/// 캐릭터·대화창·전투·상점 등 다른 UI에는 Safe Area가 전파되지 않는다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class TopHudSafeArea : MonoBehaviour
{
    private RectTransform targetRect;
    private Vector2 authoredAnchoredPosition;

    // Device Simulator에서는 Play Mode 중에도 화면 크기와 Safe Area가 바뀔 수 있다.
    // 마지막 화면 조건을 캐싱하여 값이 달라진 프레임에만 RectTransform을 다시 쓴다.
    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;
    private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);

    private void Awake()
    {
        targetRect = GetComponent<RectTransform>();

        // Safe Area 보정값이 계속 누적되지 않도록 씬에서 작성한 위치를 한 번만 보관한다.
        // 이후 기기를 바꿀 때마다 항상 이 기준 위치에서 새 보정량을 계산한다.
        authoredAnchoredPosition = targetRect.anchoredPosition;

        InvalidateScreenCache();
        ApplySafeAreaIfChanged();
    }

    private void OnEnable()
    {
        // 비활성화되었다가 다시 켜질 때 현재 기기의 화면 조건을 다시 반영한다.
        InvalidateScreenCache();
    }

    private void Update()
    {
        ApplySafeAreaIfChanged();
    }

    private void ApplySafeAreaIfChanged()
    {
        if (targetRect == null)
        {
            return;
        }

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        Rect safeArea = Screen.safeArea;

        if (lastScreenWidth == screenWidth &&
            lastScreenHeight == screenHeight &&
            lastSafeArea == safeArea)
        {
            return;
        }

        lastScreenWidth = screenWidth;
        lastScreenHeight = screenHeight;
        lastSafeArea = safeArea;

        // Canvas Scaler가 Device Simulator의 새 해상도를 반영한 뒤 변환해야
        // 화면 픽셀과 RectTransform 로컬 단위 사이의 비율이 정확해진다.
        Canvas.ForceUpdateCanvases();

        float unsafeTopPixels = ShouldApplyAndroidSafeArea()
            ? Mathf.Max(0f, screenHeight - safeArea.yMax)
            : 0f;
        float localTopInset = ConvertScreenPixelsToParentUnits(unsafeTopPixels);

        // 상단 inset은 화면 위에서 아래로 확보해야 하므로 Y축 음수 방향으로 적용한다.
        // X 위치와 하단 Safe Area는 이번 Portrait 상단 HUD 작업의 대상이 아니다.
        targetRect.anchoredPosition = authoredAnchoredPosition + Vector2.down * localTopInset;
    }

    private float ConvertScreenPixelsToParentUnits(float screenPixels)
    {
        if (screenPixels <= 0f || targetRect.parent is not RectTransform parentRect)
        {
            return 0f;
        }

        Canvas canvas = parentRect.GetComponentInParent<Canvas>();
        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        Vector2 screenBottom = Vector2.zero;
        Vector2 screenInsetPoint = new Vector2(0f, screenPixels);

        bool convertedBottom = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenBottom,
            eventCamera,
            out Vector2 localBottom);
        bool convertedInset = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenInsetPoint,
            eventCamera,
            out Vector2 localInsetPoint);

        if (convertedBottom && convertedInset)
        {
            return Mathf.Abs(localInsetPoint.y - localBottom.y);
        }

        // 일반적인 Screen Space Overlay Canvas에서는 위 변환이 성공한다. 예외적인 Canvas
        // 설정에서도 최소한 Canvas 배율을 반영할 수 있도록 안전한 대체 계산을 둔다.
        float canvasScale = canvas != null ? canvas.scaleFactor : 1f;
        return screenPixels / Mathf.Max(0.0001f, canvasScale);
    }

    private static bool ShouldApplyAndroidSafeArea()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        // Editor에서는 Android Device Simulator 검증을 위해 동일한 계산을 허용한다.
        return true;
#else
        return false;
#endif
    }

    private void InvalidateScreenCache()
    {
        lastScreenWidth = -1;
        lastScreenHeight = -1;
        lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
    }
}
