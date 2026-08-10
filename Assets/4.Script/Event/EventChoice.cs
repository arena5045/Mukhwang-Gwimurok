using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventChoice
{
    public string choiceText;
    public string descriptText;
    public DialogueSequence nextDialogue;
    public RewardType rewardType;
    public int rewardAmount;

    public float success_rate = 100f;

    [TableList]
    public List<EventSet> success_Effects = new List<EventSet>(); // 성공시 발생하는 이벤트
    [TableList]
    public List<EventSet> failed_Effects = new List<EventSet>(); // 실패시 발생하는 이벤트

    public bool Execute(GameContext context)
    {
        float roll = Random.Range(0f, 100f);
        bool isSuccess = roll <= success_rate;

        var effectsToRun = isSuccess ? success_Effects : failed_Effects;

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
}