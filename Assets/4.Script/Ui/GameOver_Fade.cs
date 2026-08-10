using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOver_Fade : MonoBehaviour
{
    public Image fade_Panel;

    private void OnEnable()
    {
        Color a = Color.black;
        fade_Panel.color = a;
        StartCoroutine(Fade());
    }
    private void OnDisable()
    {
        fade_Panel.DOKill();
        StopCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        while(true)
        {
            float rand_alpha = Random.Range(0.0f, 0.5f);
            float rand_time = Random.Range(0.1f, 0.7f); 

            yield return fade_Panel.DOFade(rand_alpha, rand_time).WaitForCompletion();
        }
    }

}
