using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Event/Event Data")]
public class EventData : ScriptableObject
{
    public string eventTitle;
    public string eventMainDescription;
    public EventType eventType;
    public DialogueSequence dialogue;
    public List<EventChoice> choices;
}