using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 보상 무공들을 화면에 표시하고,
/// 선택된 무공을 보상 관리자에게 전달한다.
/// 보상풀과 추첨 규칙은 소유하지 않는다.
/// </summary>
public class BattleRewardUI : MonoBehaviour
{
    [SerializeField]
    private BattleRewardOptionUI[] options;

    /// <summary>
    /// 전달받은 무공 목록을 보상 선택지에 표시한다.
    /// 정상적으로 열었으면 true를 반환한다.
    /// </summary>
    public bool Open(
        IReadOnlyList<SkillData> rewards,
        Action<SkillData> onSelected)
    {
        if (rewards == null ||
            onSelected == null)
        {
            Debug.LogError(
                "전투 보상 UI에 필요한 데이터가 없습니다.");

            return false;
        }

        if (options == null ||
            options.Length != rewards.Count)
        {
            Debug.LogError(
                $"전투 보상 선택지 개수와 후보 개수가 다릅니다. " +
                $"UI: {options?.Length ?? 0}, " +
                $"후보: {rewards.Count}");

            return false;
        }

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == null)
            {
                Debug.LogError(
                    $"전투 보상 선택지 {i}번이 연결되지 않았습니다.");

                return false;
            }

            options[i].Setup(
                rewards[i],
                onSelected);
        }

        gameObject.SetActive(true);
        return true;
    }

    /// <summary>
    /// 전투 보상 선택창을 닫는다.
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }
}