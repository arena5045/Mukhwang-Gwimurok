using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    //인 게임에서 ui를 관리하는 싱글턴
    public static EventManager Instance { get; private set; }

    public List<DialogueSequence> events;

    public List<DialogueSequence> repeat_event;
    private void Awake()
    {
        Instance = this;
    }


    public DialogueSequence PickEvent()
    {
        int rand;
        DialogueSequence selected_event;
        if (events.Count == 0)
        {
             rand = Random.Range(0, repeat_event.Count);

            selected_event = repeat_event[rand];
            return selected_event;
        }

        rand = Random.Range(0, events.Count);
        selected_event = events[rand];
        events.Remove(selected_event);
        return selected_event;
    }

    //이벤트 초이스용 실행
    public bool Execute(GameContext context,EventChoice eventC)
    {
        float roll = Random.Range(0f, 100f);
        bool isSuccess = roll <= eventC.success_rate;

        var effectsToRun = isSuccess ? eventC.success_Effects : eventC.failed_Effects;

        if (effectsToRun.Count == 0)
            Debug.LogWarning($"[EventChoice] 효과 없음: {(isSuccess ? "성공" : "실패")} 상태에서 실행할 이벤트가 없습니다");

        foreach (var effect in effectsToRun)
        {
            if (effect.effectAsset is IEventEffect effectInstance)
            {
                effectInstance.Execute(context, effect.param);
            }
            else
            {
                Debug.LogWarning($"[EventChoice] {effect.effectAsset}은 IEventEffect가 아님");
            }
        }

        return isSuccess;
    }

    //1회용 실행
    public bool Execute(GameContext context, List<EventSet> effects)
    {

        foreach (EventSet effect in effects)
        {

            effect.effectAsset.Execute(context, effect.param);

            if (effect.effectAsset is IEventEffect effectInstance)
            {
              effectInstance.Execute(context, effect.param);
            }
            else
            {
               Debug.LogWarning($"[EventChoice] {effect.effectAsset}은 IEventEffect가 아님");
            }
        }

        return true;
    }

}
