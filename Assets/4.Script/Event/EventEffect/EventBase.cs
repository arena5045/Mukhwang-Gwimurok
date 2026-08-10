using UnityEngine;
using Sirenix.OdinInspector;

public abstract class EventEffectBase : ScriptableObject, IEventEffect
{
    // 필요한 공통 메서드나 필드 정의 가능
    public abstract void Execute(GameContext context, EffectParam param);
}