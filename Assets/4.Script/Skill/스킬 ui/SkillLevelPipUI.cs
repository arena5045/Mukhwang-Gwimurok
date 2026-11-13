using UnityEngine;

public class SkillLevelPipUI : MonoBehaviour
{
    [SerializeField] private GameObject filled;

    /// <summary>
    /// 이 수련도 칸을 표시하고,
    /// 현재 수련 단계에 포함되는 칸인지 설정한다.
    /// </summary>
    public void SetState(
        bool visible,
        bool isFilled)
    {
        gameObject.SetActive(visible);

        if (!visible)
        {
            return;
        }

        filled.SetActive(isFilled);
    }
}