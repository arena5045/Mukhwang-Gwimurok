using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 전투 스킬 보상 후보를 구성하고,
/// 선택된 스킬을 플레이어에게 적용한다.
/// UI는 완성된 후보 목록만 전달받는다.
/// </summary>
public sealed class SkillRewardManager : MonoBehaviour
{
    private const int RewardChoiceCount = 3;

    [Header("공용 보상풀")]
    [Tooltip("모든 캐릭터의 전투 보상에서 공통으로 등장하는 무공풀")]
    [SerializeField]
    private SkillRewardPoolData commonRewardPool;

    [Header("보상 UI")]
    [SerializeField]
    private BattleRewardUI rewardUI;

    // 현재 보상 선택이 끝난 뒤 실행할 다음 작업
    private Action onRewardCompleted;

    // 같은 보상창이 중복으로 열리는 것을 방지
    private bool isRewardOpen;

    /// <summary>
    /// 현재 캐릭터 전용 풀과 공용 풀을 합쳐
    /// 전투 보상 3개를 생성한다.
    /// </summary>
    public void Open(Action onComplete = null)
    {
        if (isRewardOpen)
        {
            Debug.LogWarning(
                "이미 전투 스킬 보상을 선택 중입니다.");

            return;
        }

        if (rewardUI == null)
        {
            Debug.LogError(
                "스킬 보상 UI가 연결되지 않았습니다.");

            onComplete?.Invoke();
            return;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.Context == null ||
            GameManager.Instance.Context.player == null)
        {
            Debug.LogError(
                "스킬 보상을 받을 플레이어 정보가 없습니다.");

            onComplete?.Invoke();
            return;
        }

        GameContext context =
            GameManager.Instance.Context;

        List<SkillData> rewards =
            RollRewards(
                context.player,
                context.character);

        // 받을 수 있는 후보가 부족하면 보상을 건너뛴다.
        if (rewards == null)
        {
            onComplete?.Invoke();
            return;
        }

        onRewardCompleted = onComplete;
        isRewardOpen = true;

        // UI 설정 실패로 전투 흐름이 멈추지 않게 한다.
        if (!rewardUI.Open(
                rewards,
                SelectReward))
        {
            CompleteReward();
        }
    }

    /// <summary>
    /// 현재 플레이어가 받을 수 있는 무공을
    /// 공용 풀과 캐릭터 전용 풀에서 모은다.
    /// </summary>
    private List<SkillData> GetAvailableSkills(
        PlayerData player,
        CharacterData character)
    {   // 같은 무공이 공용 풀과 전용 풀에 동시에 들어가도
        // 후보에는 한 번만 등장하게 한다.
        HashSet<SkillData> uniqueSkills = new HashSet<SkillData>();

        AddPoolSkills(commonRewardPool, uniqueSkills);

        if(character != null)
        {
            AddPoolSkills(
             character.rewardSkillPool,
             uniqueSkills);
        }

        List<SkillData> candidates = new List<SkillData>();

        foreach (SkillData skill in uniqueSkills)
        {
            if (skill != null &&
                player.CanReceiveSkill(skill))
            {
                candidates.Add(skill);
            }
        }

        return candidates;
    }

    /// <summary>
    /// 지정한 보상풀의 유효한 무공을 후보 집합에 추가한다.
    /// </summary>
    private static void AddPoolSkills(
        SkillRewardPoolData pool,
        HashSet<SkillData> target)
    {
        if(pool == null || target == null || pool.Skills == null)
        {
            return;
        }

        foreach (SkillData skill in pool.Skills)
        {
            if(skill != null)
            {
                target.Add(skill);
            }
        }
    }

    /// <summary>
    /// 받을 수 있는 후보를 섞은 뒤
    /// 서로 다른 무공 3개를 반환한다.
    /// </summary>
    private List<SkillData> RollRewards(
        PlayerData player,
        CharacterData character)
    {
        List<SkillData> candidates =
            GetAvailableSkills(
                player,
                character);

        if (candidates.Count < RewardChoiceCount)
        {
            Debug.LogWarning(
                $"전투 보상으로 제시할 수 있는 무공이 " +
                $"{RewardChoiceCount}개 미만입니다. " +
                $"현재 후보: {candidates.Count}개");

            return null;
        }

        // Fisher-Yates 방식으로 후보 목록 섞기
        for (int i = 0; i < candidates.Count; i++)
        {
            int randomIndex =
                UnityEngine.Random.Range(
                    i,
                    candidates.Count);

            SkillData temp =
                candidates[i];

            candidates[i] =
                candidates[randomIndex];

            candidates[randomIndex] =
                temp;
        }

        return candidates.GetRange(
            0,
            RewardChoiceCount);
    }
    /// <summary>
    /// UI에서 선택한 무공을 신규 획득하거나
    /// 이미 보유한 무공의 수련 단계를 올린다.
    /// </summary>
    private void SelectReward(
        SkillData skill)
    {
        if (!isRewardOpen ||
            skill == null)
        {
            return;
        }

        PlayerData player =
            GameManager.Instance.Context.player;

        bool success =
            player.AcquireOrLevelUpSkill(
                skill);

        if (!success)
        {
            Debug.LogWarning(
                $"무공 보상 적용 실패: {skill.skillName}");

            return;
        }

        OwnedSkill ownedSkill =
            player.GetOwnedSkill(skill);

        Debug.Log(
            $"전투 보상 선택: {skill.skillName} " +
            $"수련 {ownedSkill.level}");

        rewardUI.Close();
        CompleteReward();
    }

    /// <summary>
    /// 보상 선택 상태를 정리하고
    /// 기존 전투 종료 흐름을 이어간다.
    /// </summary>
    private void CompleteReward()
    {
        isRewardOpen = false;

        Action completeAction =
            onRewardCompleted;

        onRewardCompleted = null;

        completeAction?.Invoke();
    }

}
