using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TouchToStart : MonoBehaviour
{
    private bool touched = false;

    public GameObject main;
    public GameObject next;

    public void OnPanelClicked()
    {
        Debug.Log("실행됨");

        if (touched) return;
        touched = true;

        //화면 줌
        main.GetComponent<RectTransform>().DOScale(1.2f, 1f).SetEase(Ease.InOutSine);
        //여기서 다음 패널로 넘어감
        ScreenFader.Instance.FadeAtoB(main, next);
    }
}