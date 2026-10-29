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

    //스킬 사용중인지 추적
    private bool isUsingSkill;

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
    /// 현재 선택한 스킬을 해제하고 상세정보창을 닫는다.
    /// </summary>
    public void ClearSelection()
    {
        selectedSkill = null;
        skillDetailUI.Clear();
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

        // 이미 선택한 스킬을 다시 누르면 선택 해제
        if (selectedSkill == state)
        {
            ClearSelection();
            return;
        }


        selectedSkill = state;

        skillDetailUI.SetSkill(state);
    }

    /// <summary>
    /// 현재 선택한 스킬을 실제로 사용한다.
    /// </summary>
    public void ConfirmSelectedSkill()
    {
        if (selectedSkill == null ||
            isUsingSkill)
        {
            return;
        }

        // 실제 사용할 스킬은 먼저 보관
        BattleSkillState skillToUse =
            selectedSkill;


        isUsingSkill = true;
        // 확인한 순간 상세정보는 바로 닫는다.
        ClearSelection();

        // 행동 연출을 볼 수 있도록 선택창을 닫는다.
        gameObject.SetActive(false);

        BattleManager.instance.buiManager.ShowResolutionLog();

        //스킬 사용 시도
        bool started = BattleManager.instance.UseManualSkill(skillToUse, OnSkillResolved);

        // 스킬 사용이 거부됐다면 선택창으로 복귀
        if (!started)
        {
            isUsingSkill = false;

            BattleManager.instance.buiManager.HideResolutionLog();

            Open();
        }
    }

    /// <summary>
    /// 스킬과 자동 연계 실행이 모두 끝난 뒤 호출된다.
    /// </summary>
    private void OnSkillResolved()
    {
        isUsingSkill = false;
        selectedSkill = null;

        skillDetailUI.Clear();
        RefreshSkillStates();

        // 적이 쓰러졌다면 전투 종료 UI가 처리하므로 복귀하지 않는다.
        if (BattleManager.instance.currentMonsterInfo == null ||
            BattleManager.instance.currentMonsterInfo.currentHp <= 0)
        {
            return;
        }

        //로그창 닫은뒤 전투메뉴호출
        BattleManager.instance.buiManager.HideResolutionLog();
        BattleManager.instance.buiManager.ShowActionMenu();

        if (BattleManager.instance.CurrentActionPoints > 0)
        {
            Open();
        }
    }
}