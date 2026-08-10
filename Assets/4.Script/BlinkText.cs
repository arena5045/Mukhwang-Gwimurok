using DG.Tweening;
using TMPro;
using UnityEngine;

public class BlinkText : MonoBehaviour
{
    // using UnityEngine.UI;
    // using DG.Tweening;

    public TextMeshProUGUI touchText;
    public float loopTime = 1f;
    public float a_Value = 0;

    void Start()
    {
        touchText.DOFade(a_Value, loopTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
}
