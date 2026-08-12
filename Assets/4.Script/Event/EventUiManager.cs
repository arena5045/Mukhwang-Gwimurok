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
        // 이벤트 선택 패널과 대화 매니저는 씬 객체이므로 이 매니저도 씬 종속으로 관리한다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        // 씬이 닫힌 뒤 파괴된 이벤트 패널로 접근하는 것을 방지한다.
        if (Instance == this)
        {
            Instance = null;
        }
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
        EventEnd(null);
    }

    /// <summary>
    /// 선택 이벤트를 끝내고 대화 흐름을 이어간다.
    /// nextDialogue가 있으면 현재 대화 상태를 Stack에 저장하고 분기 대화를 시작하며,
    /// 없으면 기존 동작 그대로 현재 Sequence의 다음 DialogueLine을 실행한다.
    /// </summary>
    public void EventEnd(DialogueSequence nextDialogue)
    {
        // Inspector 참조가 비어 있어도 씬의 DialogueManager 싱글턴으로 한 번 복구한다.
        // 둘 다 없다면 분기 상태를 변경하지 않고 오류를 남겨 잘못된 참조를 추적할 수 있게 한다.
        if (dialogueManager == null)
        {
            dialogueManager = DialogueManager.Instance;
        }

        if (dialogueManager == null)
        {
            Debug.LogError("[EventUIManager] 이벤트 종료 후 이어갈 DialogueManager가 없습니다.");
            return;
        }

        if(!dialogueManager.canInput && dialogueManager.isTexting)
        {//대화중 이벤트였다면 다음대사
            if (nextDialogue != null && dialogueManager.StartBranchDialogue(nextDialogue))
            {
                return;
            }

            // nextDialogue가 없거나 잘못된 데이터라면 기존 이벤트의 다음 줄로 계속 진행한다.
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
