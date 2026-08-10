using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DialogueLine
{
    public DialogueActionType actionType = DialogueActionType.Talk;


    public string characterName;
    [TextArea(2, 5)] public string text;
    public Sprite portrait;
    public Emotion emotion;


    public EventData triggeredEvent_Data; // TriggerEvent일 때만 사용
    [TableList]
    public List<EventSet> triggeredEvent_Effect; // TriggerEvent일 때만 사용
}
public enum Emotion
{
    Neutral,
    Happy,
    Sad,
    Angry,
    Surprised
}