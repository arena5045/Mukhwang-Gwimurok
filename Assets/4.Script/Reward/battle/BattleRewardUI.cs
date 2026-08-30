using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BattleRewardUI : MonoBehaviour
{
    [SerializeField]
    private List<SkillData> rewardPool = new();

    [SerializeField]
    private BattleRewardOptionUI[] options;

    public void Open()
    {
        List<SkillData> rewards =
            RollRewards();

        if (rewards == null)
        {
            return;
        }

        // 추첨된 세 스킬을 각각의 보상 카드에 표시한다.
        for (int i = 0; i < 3; i++)
        {
            options[i].Setup(rewards[i]);
        }

        gameObject.SetActive(true);
    }

    // 현재 플레이어가 실제로 받을 수 있는 스킬만 모은다.
    private List<SkillData> GetAvailableSkills()
    {
        PlayerData player =
            GameManager.Instance.Context.player;

        List<SkillData> candidates = new();

        foreach (SkillData skill in rewardPool)
        {
            if (player.CanReceiveSkill(skill))
            {
                candidates.Add(skill);
            }
        }

        return candidates;
    }

    // 받을 수 있는 후보 중 서로 다른 스킬 3개를 랜덤으로 뽑는다.
    private List<SkillData> RollRewards()
    {
        List<SkillData> candidates =
            GetAvailableSkills();

        if (candidates.Count < 3)
        {
            Debug.LogError(
                "전투 보상으로 제시할 수 있는 스킬이 3개 미만입니다.");

            return null;
        }

        // 후보 목록을 섞는다.
        for (int i = 0; i < candidates.Count; i++)
        {
            int randomIndex =
                UnityEngine.Random.Range(
                    i,
                    candidates.Count);

            SkillData temp = candidates[i];
            candidates[i] = candidates[randomIndex];
            candidates[randomIndex] = temp;
        }

        // 섞인 목록의 앞 3개만 보상 후보로 사용한다.
        return new List<SkillData>
        {
            candidates[0],
            candidates[1],
            candidates[2]
        };
    }

    [Button("전투 스킬 후보 테스트")]
    private void TestRollRewards()
    {
        List<SkillData> rewards =
            RollRewards();

        if (rewards == null)
        {
            return;
        }

        Debug.Log(
            $"전투 스킬 후보\n" +
            $"1. {rewards[0].skillName}\n" +
            $"2. {rewards[1].skillName}\n" +
            $"3. {rewards[2].skillName}");
    }


    [Button("전투 스킬 후보 테스트")]
    private void TestRollRewards2()
    {
        Open();
    }
}