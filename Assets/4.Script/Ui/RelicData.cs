public abstract class RelicData : ItemData
{

    // 유물을 잃었을 때(혹은 효과 제거 시) 실행될 로직 (선택적)
    public virtual void OnRemove() { }
}