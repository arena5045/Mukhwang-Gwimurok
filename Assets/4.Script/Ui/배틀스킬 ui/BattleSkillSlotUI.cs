using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSkillSlotUI : MonoBehaviour
{
    [Header("공용 무공 아이콘")]
    [SerializeField]
    private SkillIconUI skillIconUI;

    [Header("스킬 슬롯 UI")]
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private Button selectButton;


    // 현재 이 슬롯이 표시하고 있는 전투용 스킬 상태
    private BattleSkillState currentState;

    // 슬롯을 클릭했을 때 부모 UI에게 선택된 스킬을 전달할 콜백
    private Action<BattleSkillState> onSelected;


    // 스킬 슬롯에 표시할 데이터를 설정한다
    public void Setup(
        BattleSkillState state,
        Action<BattleSkillState> selectAction)
    {
        if (state == null ||
            state.Data == null)
        {
            return;
        }

        currentState = state;
        onSelected = selectAction;

        // 스킬 이름 표시
        nameText.text = state.Data.skillName;

        // 아이콘, 비용, 희귀도, 수련도는
        // 공용 무공 아이콘 UI가 표시한다.
        skillIconUI.SetSkill(
            state.Data,
            state.Level);

        // 프리팹을 재사용하더라도 클릭 이벤트가 중복 등록되지 않게 초기화
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(Select);

        // 처음 생성될 때 현재 전투 상태도 같이 반영
        RefreshState();
    }

    /// <summary>
    /// 행동력, 쿨타임, 이번 턴 사용 여부에 따라
    /// 스킬 슬롯의 현재 상태를 갱신한다.
    /// </summary>
    public void RefreshState()
    {
        if (currentState == null ||
            BattleManager.instance == null)
        {
            return;
        }

        SkillLevelData levelData =
            currentState.Data.GetLevelData(currentState.Level);

        if (levelData == null)
        {
            return;
        }

        // 실제 무공 실행 때 사용하는 것과
        // 동일한 판정 함수를 사용한다.
        bool canUse = BattleManager.instance.CanUseManualSkill(currentState);


        skillIconUI.SetBattleState(
            canUse,
            currentState.CooldownRemaining);
    }


    // 슬롯 클릭 시 현재 스킬을 부모 UI에게 전달
    // 실제 스킬 사용은 여기서 처리하지 않음
    private void Select()
    {
        if (currentState == null)
        {
            return;
        }

        onSelected?.Invoke(currentState);
    }
}