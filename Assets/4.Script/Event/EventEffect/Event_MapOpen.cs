using Sirenix.OdinInspector;
using UnityEngine;
[CreateAssetMenu(menuName = "EventEffects/EventUi")]
public class Event_MapOpen : EventEffectBase
{
    [ReadOnly]
    [Tooltip("맵ui를 엽니다")]
    [TextArea]
    public string description = "맵ui를 엽니다";

    public override void Execute(GameContext context, EffectParam param)
    {
        bool amount = param.boolValue;
        if(GameUiManager.Instance != null)
        { 
            GameUiManager.Instance.MapUiOpen(amount); 
        }
        else
        {
            Debug.Log($"맵ui매니저없음. canClose값 : {param.boolValue}");
        }
        Debug.Log($"맵ui를 실행시킵니다. canClose값 : {param.boolValue}");
    }
}
