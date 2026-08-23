
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class RealmRewardUI : MonoBehaviour
{
    // 한 번에 여러 경지가 올랐을 때 아직 받아야 할 보상 선택 횟수
    private int pendingRewards;


    [SerializeField]
    private List<RealmRewardData> rewardPool = new();

    [SerializeField]
    private RealmRewardOptionUI[] options;

    public void Open(int rewardCount = 1)
    {
        pendingRewards += rewardCount;

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
                Random.Range(i, candidates.Count);

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
    }


    [Button("경지 보상 테스트")]
    private void TestOpen()
    {
        Open();
    }

}
