
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class EventSet
{
    [LabelText("이펙트 종류")]
    [SerializeReference]
    public EventEffectBase effectAsset;

    [LabelText("이펙트 파라미터")]
    public EffectParam param;
}
