using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleRewardUI : MonoBehaviour
{
    [SerializeField]
    private List<SkillData> rewardPool = new();

    [SerializeField]
    private BattleRewardOptionUI[] options;

    // 스킬 보상 선택이 끝난 뒤 실행할 다음 작업
    private Action onRewardSelected;


    public void Open(Action onComplete = null)
    {
        List<SkillData> rewards = RollRewards();

        if (rewards == null)
        {
            // 받을 스킬이 부족하면 이번 스킬 보상은 건너뛰고 다음 흐름으로 진행한다.
            onComplete?.Invoke();
            return;
        }
        // 이번 스킬 보상 선택이 끝난 뒤 실행할 작업을 기억해둔다.
        onRewardSelected = onComplete;


        for (int i = 0; i < 3; i++)
        {
            // 카드가 선택되면 SelectReward가 호출되도록 같이 전달
            options[i].Setup(
                rewards[i],
                SelectReward);
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
    private void SelectReward(SkillData skill)
    {
        PlayerData player =
            GameManager.Instance.Context.player;

        // 신규 스킬이면 Lv1 획득,
        // 이미 보유 중이면 한 레벨 강화한다.
        bool success =
            player.AcquireOrLevelUpSkill(skill);

        if (!success)
        {
            Debug.LogWarning(
                $"스킬 보상 적용 실패 : {skill.skillName}");

            return;
        }

        OwnedSkill ownedSkill =
            player.GetOwnedSkill(skill);

        Debug.Log(
            $"전투 보상 선택 : {skill.skillName} Lv.{ownedSkill.level}");

        gameObject.SetActive(false);

        // 현재 완료 작업을 먼저 꺼내고 비운 뒤 한 번만 실행한다.
        Action completeAction = onRewardSelected;
        onRewardSelected = null;

        completeAction?.Invoke();
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


    [Button("전투 스킬 보상 완료 콜백 테스트")]
    private void TestOpen()
    {
        Open(TestComplete);
    }

    private void TestComplete()
    {
        Debug.Log("전투 스킬 보상 처리 완료!");
    }


}