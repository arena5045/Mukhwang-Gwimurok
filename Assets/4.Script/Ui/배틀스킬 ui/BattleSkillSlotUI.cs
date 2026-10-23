using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSkillSlotUI : MonoBehaviour
{
    [Header("스킬 슬롯 UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text actionPointText;
    [SerializeField] private Button selectButton;

    [Header("스킬 상태 UI")]
    [SerializeField] private GameObject disabledOverlay;
    [SerializeField] private TMP_Text cooldownText;


    // 현재 이 슬롯이 표시하고 있는 전투용 스킬 상태
    private BattleSkillState currentState;

    // 슬롯을 클릭했을 때 부모 UI에게 선택된 스킬을 전달할 콜백
    private Action<BattleSkillState> onSelected;


    // 스킬 슬롯에 표시할 데이터를 설정한다
    public void Setup(
        BattleSkillState state,
        Action<BattleSkillState> selectAction)
    {
        currentState = state;
        onSelected = selectAction;

        SkillData skill = state.Data;

        // 현재 스킬 레벨에 맞는 행동력 비용 등의 데이터를 가져옴
        SkillLevelData levelData =
            skill.GetLevelData(state.Level);

        // 스킬 이름 표시
        nameText.text = skill.skillName;

        // 스킬 아이콘 표시
        iconImage.sprite = skill.icon;
        iconImage.enabled = skill.icon != null;

        // 스킬 사용에 필요한 행동력 표시
        actionPointText.text =
            levelData.actionPointCost.ToString();

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
        if (currentState == null)
        {
            return;
        }

        SkillLevelData levelData =
            currentState.Data.GetLevelData(currentState.Level);

        if (levelData == null)
        {
            return;
        }

        // 현재 이 스킬을 실제로 사용할 수 있는지 확인
        bool canUse =
            currentState.CanUseActive() &&
            BattleManager.instance.CurrentActionPoints >=
            levelData.actionPointCost;

        // 사용할 수 없는 스킬은 어둡게 표시
        disabledOverlay.SetActive(!canUse);

        // 재사용 대기시간 표시
        if (currentState.CooldownRemaining > 0)
        {
            cooldownText.gameObject.SetActive(true);
            cooldownText.text =
                currentState.CooldownRemaining.ToString();
        }
        else
        {
            cooldownText.gameObject.SetActive(false);
        }
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