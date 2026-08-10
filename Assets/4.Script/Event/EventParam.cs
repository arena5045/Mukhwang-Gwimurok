using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class EffectParam
{
    [LabelText("사용할 파라미터 타입")]
    public EffectParamType type;

    [ShowIf("type", EffectParamType.Int)]
    [LabelText("정수값")]
    public int intValue;

    [ShowIf("type", EffectParamType.Float)]
    [LabelText("실수값")]
    public float floatValue;

    [ShowIf("type", EffectParamType.String)]
    [LabelText("문자열")]
    public string stringValue;

    [ShowIf("type", EffectParamType.Bool)]
    [LabelText("참/거짓")]
    public bool boolValue;

    [ShowIf("type", EffectParamType.Reference)]
    [LabelText("스크립터블 오브젝트")]
    public ScriptableObject referenceValue;
}