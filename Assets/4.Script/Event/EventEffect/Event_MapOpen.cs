using Sirenix.OdinInspector;
using UnityEngine;

// 기존 "MapUi_Open(Bool)" SO 에셋을 검증 전까지 유지하기 위한 호환 클래스다.
// 새 이벤트 데이터에서는 같은 기능을 인라인 OpenMapEffect로 설정한다.
// 새 구형 효과 에셋이 더 생기지 않도록 CreateAssetMenu는 노출하지 않는다.
public class Event_MapOpen : ScriptableObject, IEventEffect
{
    [ReadOnly]
    [Tooltip("맵ui를 엽니다")]
    [TextArea]
    public string description = "맵ui를 엽니다";

    public void Execute(GameContext context, EffectParam param)
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
