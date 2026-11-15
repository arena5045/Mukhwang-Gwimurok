
using TMPro;
using UnityEngine;

public class BattleSkillDetailUI : MonoBehaviour
{
    [Header("공용 무공 아이콘")]
    [SerializeField]
    private SkillIconUI skillIconUI;

    [Header("무공 상세 정보")]
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text cooldownText;

    [SerializeField]
    private TMP_Text initialCooldownText;

    [SerializeField]
    private TMP_Text tagText;




    /// <summary>
    /// 선택된 스킬의 상세 정보를 표시한다.
    /// </summary>
    public void SetSkill(BattleSkillState state)
    {
        if (state == null ||
            state.Data == null)
        {
            Clear();
            return;
        }

        SkillData skill =
            state.Data;

        SkillLevelData levelData =
            skill.GetLevelData(
                state.Level);

        if (levelData == null)
        {
            Clear();
            return;
        }

        // 스킬이 선택되면 상세 정보창을 표시
        gameObject.SetActive(true);

        // 아이콘, 비용, 희귀도, 수련도
        skillIconUI.SetSkill(
            skill,
            state.Level);

        nameText.text =
          skill.skillName;

        // 현재 적과 전투 상태를 반영한 설명
        descriptionText.text =
            SkillDescriptionBuilder
                .BuildBattleDescription(
                    skill,
                    state.Level,
                    BattleManager.instance);


        // 여기 표시되는 값은 현재 남은 시간이 아니라
        // 이 무공 자체에 설정된 기본 재사용 대기시간이다.
        cooldownText.text =
            $"재사용 대기시간: " +
            $"{levelData.cooldownTurns}";

        initialCooldownText.text =
            $"준비시간: " +
            $"{levelData.initialCooldownTurns}";

        // 태그 한글화는 이후 별도 작업으로 둔다.
        tagText.text =
            skill.tags.ToString();

        //쿨타임
        bool canUse =BattleManager.instance.CanUseManualSkill(state);

        skillIconUI.SetBattleState(canUse,state.CooldownRemaining);
    }


    /// <summary>
    /// 선택된 스킬이 없으면 상세 정보창을 숨긴다.
    /// </summary>
    public void Clear()
    {
        gameObject.SetActive(false);
    }
}