
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RealmRewardUI : MonoBehaviour
{
    // 한 번에 여러 경지가 올랐을 때 아직 받아야 할 보상 선택 횟수
    private int pendingRewards;

    // 모든 경지 보상 선택이 끝난 뒤 실행할 다음 작업
    private Action onAllRewardsSelected;



    [SerializeField]
    private List<RealmRewardData> rewardPool = new();

    [SerializeField]
    private RealmRewardOptionUI[] options;

    public void Open(int rewardCount = 1, Action onComplete = null)
    {
        pendingRewards += rewardCount;

        // 경지 보상을 전부 고른 뒤 실행할 작업을 기억해둔다.
        onAllRewardsSelected = onComplete;

        ShowChoices();
    }

    private void ShowChoices()
    {
        List<RealmRewardData> candidates =
            rewardPool.FindAll(reward => reward != null);

        if (candidates.Count < 3)
        {
            Debug.LogError("경지 보상이 최소 3개 필요합니다.");
            return;
        }

        // 중복 없이 섞기
        for (int i = 0; i < candidates.Count; i++)
        {
            int randomIndex =
                UnityEngine.Random.Range(i, candidates.Count);

            RealmRewardData temp = candidates[i];
            candidates[i] = candidates[randomIndex];
            candidates[randomIndex] = temp;
        }

        for (int i = 0; i < 3; i++)
        {
            options[i].Setup(
                candidates[i],
                SelectReward);
        }

        gameObject.SetActive(true);

    }
    private void SelectReward(RealmRewardData reward)
    {
        PlayerData player =
            GameManager.Instance.Context.player;

        reward.Apply(player);

        GameManager.Instance.RefreshPlayerUi();

        pendingRewards--;

        // 받을 경지 보상이 더 남아있으면 다시 3택1
        if (pendingRewards > 0)
        {
            ShowChoices();
            return;
        }

        gameObject.SetActive(false);

        Debug.Log(
            $"경지 보상 선택 : {reward.rewardName}");

        // 완료 콜백을 먼저 꺼내고 비워서 중복 실행을 막는다.
        Action completeAction = onAllRewardsSelected;
        onAllRewardsSelected = null;

        completeAction?.Invoke();
    }

    [Button("경지 보상 완료 콜백 테스트")]
    private void TestOpen()
    {
        Open(3, TestComplete);
    }

    private void TestComplete()
    {
        Debug.Log("경지 보상 전부 선택 완료!");
    }

}
