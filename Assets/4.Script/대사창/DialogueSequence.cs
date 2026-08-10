using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Dialogue/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    public List<DialogueLine> lines;
}