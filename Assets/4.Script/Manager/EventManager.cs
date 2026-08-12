using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    //인 게임에서 ui를 관리하는 싱글턴
    public static EventManager Instance { get; private set; }

    // Inspector에서 편집하는 일회성 이벤트 원본이다. 런타임에는 이 목록을 직접 삭제하지 않는다.
    public List<DialogueSequence> events;

    // 일회성 이벤트가 모두 소진된 뒤에도 선택할 수 있는 반복 이벤트 원본이다.
    public List<DialogueSequence> repeat_event;

    // 현재 런에서 아직 소비하지 않은 이벤트만 담는 복사본이다.
    // 원본 events와 분리해야 게임오버 후 새 런에서 전체 이벤트를 다시 사용할 수 있다.
    private readonly List<DialogueSequence> availableEvents = new();

    private void Awake()
    {
        // EventManager는 씬에 배치된 이벤트 데이터와 UI 흐름을 사용하므로 씬 종속으로 둔다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResetForNewRun();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 일회성 이벤트의 런타임 소진 목록을 원본 데이터로부터 다시 만든다.
    /// null 항목은 시작 시 제외하여 PickEvent가 비어 있는 참조를 반환하지 않게 한다.
    /// </summary>
    public void ResetForNewRun()
    {
        // 원본 이벤트 목록은 보존하고, 런마다 별도의 소진 목록을 사용한다.
        availableEvents.Clear();
        if (events == null) return;

        foreach (DialogueSequence dialogueEvent in events)
        {
            if (dialogueEvent != null)
            {
                availableEvents.Add(dialogueEvent);
            }
        }
    }


    public DialogueSequence PickEvent()
    {
        if (availableEvents.Count > 0)
        {
            int rand = Random.Range(0, availableEvents.Count);
            DialogueSequence selectedEvent = availableEvents[rand];
            availableEvents.RemoveAt(rand);
            return selectedEvent;
        }

        if (repeat_event != null && repeat_event.Count > 0)
        {
            int rand = Random.Range(0, repeat_event.Count);
            return repeat_event[rand];
        }

        Debug.LogError("[EventManager] 실행 가능한 이벤트가 없습니다.");
        return null;
    }

    /// <summary>
    /// 선택지 성공 확률을 한 번 판정하고, 성공 또는 실패 쪽 효과 목록 하나만 실행한다.
    /// 각 EventSet의 인라인 효과를 정확히 한 번만 호출하여 재화·체력 효과의 중복 적용을 막는다.
    /// </summary>
    public bool Execute(GameContext context,EventChoice eventC)
    {
        if (context == null || eventC == null)
        {
            Debug.LogError("[EventManager] 이벤트 실행에 필요한 데이터가 없습니다.");
            return false;
        }

        float roll = Random.Range(0f, 100f);
        bool isSuccess = roll <= eventC.success_rate;

        var effectsToRun = isSuccess ? eventC.success_Effects : eventC.failed_Effects;

        if (effectsToRun == null || effectsToRun.Count == 0)
        {
            Debug.LogWarning($"[EventChoice] 효과 없음: {(isSuccess ? "성공" : "실패")} 상태에서 실행할 이벤트가 없습니다");
            return isSuccess;
        }

        foreach (var effect in effectsToRun)
        {
            if (effect != null)
            {
                effect.Execute(context);
            }
            else
            {
                Debug.LogWarning("[EventManager] 비어 있는 EventSet은 실행할 수 없습니다.");
            }
        }

        return isSuccess;
    }

    /// <summary>
    /// 이미 결정된 효과 목록을 순서대로 한 번씩 실행한다.
    /// 확률 판정이 필요 없는 대화 액션이나 고정 이벤트 효과에서 사용하는 경로다.
    /// </summary>
    public bool Execute(GameContext context, List<EventSet> effects)
    {
        if (context == null || effects == null)
        {
            Debug.LogError("[EventManager] 효과 실행에 필요한 데이터가 없습니다.");
            return false;
        }

        foreach (EventSet effect in effects)
        {
            if (effect != null)
            {
                effect.Execute(context);
            }
            else
            {
                Debug.LogWarning("[EventManager] 비어 있는 EventSet은 실행할 수 없습니다.");
            }
        }

        return true;
    }

}
