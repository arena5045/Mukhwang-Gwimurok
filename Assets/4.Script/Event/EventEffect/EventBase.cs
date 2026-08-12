/// <summary>
/// EventData나 DialogueSequence 안에 직접 저장되는 이벤트 효과의 공통 기반 클래스다.
///
/// ScriptableObject를 상속하지 않는 이유는 효과마다 별도 에셋을 만들지 않고,
/// 각 이벤트가 사용하는 대상과 수치를 이벤트 데이터 안에 함께 보관하기 위해서다.
/// 파생 클래스에 [Serializable]을 붙이면 EventSet의 [SerializeReference] 필드가
/// 실제 파생 타입과 그 타입 전용 데이터를 함께 직렬화한다.
/// </summary>
[System.Serializable]
public abstract class EventEffectBase
{
    /// <summary>
    /// 현재 게임 상태에 이 효과를 한 번 적용한다.
    /// 효과에 필요한 값은 공용 파라미터가 아니라 각 파생 클래스가 직접 가진다.
    /// </summary>
    public abstract void Execute(GameContext context);
}
