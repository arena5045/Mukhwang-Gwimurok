using System.Collections.Generic;
using UnityEngine;

public class BattleSkillSelectUI : MonoBehaviour
{
    [Header("스킬 목록")]
    [SerializeField] private Transform skillContent;
    [SerializeField] private BattleSkillSlotUI skillSlotPrefab;

    [Header("스킬 상세 정보Ui")]
    [SerializeField] private BattleSkillDetailUI skillDetailUI;

    //저장해둘 슬롯
    private readonly List<BattleSkillSlotUI> skillSlots = new();

    // 현재 선택한 스킬
    private BattleSkillState selectedSkill;

    /// <summary>
    /// 무공 선택창을 열고
    /// 기존 스킬 슬롯의 현재 상태를 갱신한다.
    /// </summary>
    public void Open()
    {
        RefreshSkillStates();

        selectedSkill = null;
        skillDetailUI.Clear();

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 현재 플레이어가 보유한 Manual 액티브 스킬을
    /// Grid Content에 슬롯으로 생성한다.
    /// </summary>
    public void RebuildSkillList()
    {
        ClearSkillList();

        List<BattleSkillState> skills =
            BattleManager.instance.GetManualActiveSkills();

        foreach (BattleSkillState state in skills)
        {
            BattleSkillSlotUI slot =
                Instantiate(skillSlotPrefab, skillContent);

            slot.Setup(
                state,
                SelectSkill);

            skillSlots.Add(slot);
        }
    }

    private void RefreshSkillStates()
    {
        foreach (BattleSkillSlotUI slot in skillSlots)
        {
            slot.RefreshState();
        }
    }

    /// <summary>
    /// 스킬 목록을 다시 생성하기 전에
    /// 기존에 생성된 슬롯을 모두 제거한다.
    /// </summary>
    private void ClearSkillList()
    {
        foreach (Transform child in skillContent)
        {
            Destroy(child.gameObject);
        }

        skillSlots.Clear();
    }

    /// <summary>
    /// 스킬 슬롯을 클릭했을 때
    /// 선택한 스킬을 저장하고 상세 정보를 표시한다.
    /// </summary>
    private void SelectSkill(BattleSkillState state)
    {
        if (state == null)
        {
            return;
        }

        selectedSkill = state;

        skillDetailUI.SetSkill(state);
    }
}