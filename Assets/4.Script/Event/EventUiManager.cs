using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EventUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject eventPanel;
    public GameObject ChoicePanel;
    public TextMeshProUGUI titleText;
    public DialogueManager dialogueManager;
    public Transform choiceParent;
    public GameObject choiceButtonPrefab;

    private EventData currentEvent;

    public static EventUIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        if (dialogueManager == null)
        {
            dialogueManager = DialogueManager.Instance; 
        }
    }

    public void ShowEvent(EventData eventData)
    {
        currentEvent = eventData;

        switch(eventData.eventType)
        {
            case EventType.Main:
                break;
            case EventType.Choice :
                ShowEvent_Choice();
                break;
            case EventType.Sequence:
                break;
            case EventType.Disposable:
                break;
            case EventType.Ending:
                break;
        }
        //titleText.text = eventData.eventTitle;
        //dialogueManager.StartDialogue(eventData.dialogue, OnDialogueFinished);
    }

    public void ShowEvent_Choice()
    {
        ChoicePanel.GetComponent<EventPanel_Choice>().SettingEvent(currentEvent);


        ChoicePanel.SetActive(true);

    }

    public void EventEnd()
    {
        if(!dialogueManager.canInput && dialogueManager.isTexting)
        {//대화중 이벤트였다면 다음대사
            dialogueManager.ShowNextLine();
            dialogueManager.canInput = true;
        }
    }

    private void OnDialogueFinished()
    {
        ShowChoices();
    }

    private void ShowChoices()
    {
        // 기존 선택지 제거
        foreach (Transform child in choiceParent)
            Destroy(child.gameObject);

        foreach (var choice in currentEvent.choices)
        {
            var btn = Instantiate(choiceButtonPrefab, choiceParent);
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = choice.choiceText;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"[이벤트 선택됨] {choice.choiceText}");

                // 선택 결과 설명 출력
                //dialogueManager.StartSingleLine(choice.resultDescription);

                // 보상 실행
                //choice.onSelected?.Invoke();

                // 버튼 비활성화
                foreach (Transform c in choiceParent)
                    c.GetComponent<Button>().interactable = false;
            });
        }
    }

    public void HideEvent()
    {
        eventPanel.SetActive(false);
    }
}