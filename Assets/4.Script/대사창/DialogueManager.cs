using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;


public class DialogueManager : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image portraitImage;
    public GameObject dialoguePanel;

    public bool isTexting = false; //지금 대화중인지
    public bool canInput = true; //스페이스나 클릭 눌러도 진행되는지 

    [SerializeField]
    private DialogueSequence currentSequence;
    private int currentIndex;
    private Coroutine typingCoroutine;

    /// <summary>
    /// 다른 DialogueSequence로 분기하기 직전의 복귀 위치다.
    /// currentIndex는 DialogueLine을 실행하기 전에 이미 증가하므로,
    /// 저장된 index는 이벤트가 끝난 뒤 실행할 정확한 다음 줄을 가리킨다.
    /// </summary>
    private readonly struct DialogueState
    {
        public readonly DialogueSequence sequence;
        public readonly int index;

        public DialogueState(DialogueSequence sequence, int index)
        {
            this.sequence = sequence;
            this.index = index;
        }
    }

    // 분기 대화 안에서 다시 분기할 수 있으므로 Queue가 아닌 LIFO Stack을 사용한다.
    // A → B → C 순서로 진입했다면 C → B → A 순서로 복귀한다.
    private readonly Stack<DialogueState> dialogueStateStack = new();

    // EndDialogue가 페이드 중일 때 추가 입력이나 다른 종료 액션이 들어오는 것을 차단한다.
    // 종료 코루틴이 겹치면 콜백 중복 실행과 패널 상태 불일치가 발생할 수 있다.
    private bool isEndingDialogue;

    private System.Action onDialogueComplete;

    public float typingSpeed = 0.05f; // 타자기 효과 속도

    public DialogueSequence testDialogue;
    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 대화 UI 참조는 씬 전용이므로 씬과 함께 생성·해제한다.
        Instance = this;
    }

    private void OnDestroy()
    {
        // 다음 씬의 DialogueManager가 정상 등록될 수 있도록 현재 씬의 참조를 해제한다.
        if (Instance == this)
        {
            Instance = null;
        }
    }


    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            canInput)
        {
            if(isTexting)
            {
                OnNextButtonClicked();
            }
            else
            {
#if UNITY_EDITOR
                StartDialogue(testDialogue);
#endif
            }
          
        }
    }


    public void StartDialogue(DialogueSequence sequence)
    {
        StartDialogue(sequence, null);
    }
    public void StartDialogue(DialogueSequence sequence, System.Action onComplete = null)
    {
        // 대화 데이터가 비어 있으면 ShowNextLine에서 lines에 접근하기 전에 중단한다.
        // 잘못된 콘텐츠 하나가 전체 진행을 크래시시키지 않도록 오류 로그만 남긴다.
        if (sequence == null || sequence.lines == null || sequence.lines.Count == 0)
        {
            Debug.LogError("[DialogueManager] 재생할 대화 데이터가 없습니다.", sequence);
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isEndingDialogue = false;
        isTexting = true;
        canInput = true;

        // StartDialogue는 새로운 최상위 대화를 시작하는 진입점이다.
        // 이전 대화의 분기 복귀 정보가 새 대화로 섞이지 않도록 Stack을 비운다.
        dialogueStateStack.Clear();
        currentSequence = sequence;
        currentIndex = 0;
        onDialogueComplete = onComplete;

        dialoguePanel.SetActive(true);
        ShowNextLine();
    }

    /// <summary>
    /// 이벤트 선택지에 연결된 분기 DialogueSequence를 현재 대화 안에서 실행한다.
    /// 부모 Sequence와 이미 증가한 currentIndex를 Stack에 저장한 뒤 분기 대화의 첫 줄로 전환한다.
    /// 잘못된 분기 데이터라면 false를 반환하여 호출부가 기존 대화 진행 방식으로 복구할 수 있게 한다.
    /// </summary>
    public bool StartBranchDialogue(DialogueSequence sequence)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Count == 0)
        {
            Debug.LogError("[DialogueManager] 선택지에 연결된 분기 대화 데이터가 없습니다.", sequence);
            return false;
        }

        if (!isTexting || isEndingDialogue || currentSequence == null)
        {
            Debug.LogError("[DialogueManager] 복귀할 기존 대화가 없어 분기 대화를 시작할 수 없습니다.");
            return false;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // TriggerEventData 처리 시 currentIndex가 먼저 증가했으므로 현재 값을 그대로 저장해야 한다.
        // 복귀할 때 이 index부터 실행하면 이벤트 다음 DialogueLine으로 자연스럽게 이어진다.
        dialogueStateStack.Push(new DialogueState(currentSequence, currentIndex));

        currentSequence = sequence;
        currentIndex = 0;
        canInput = true;
        dialoguePanel.SetActive(true);
        ShowNextLine();
        return true;
    }

    public void ShowNextLine()
    {
        if (isEndingDialogue) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


        if (currentSequence == null || currentSequence.lines == null)
        {
            Debug.LogError("[DialogueManager] 진행 중인 대화 데이터가 유실되었습니다.");
            StartCoroutine(EndDialogue());
            return;
        }

        if (currentIndex >= currentSequence.lines.Count)
        {
            // 마지막 줄 뒤에 End가 없는 Sequence도 현재 Sequence가 끝난 것으로 처리한다.
            // 분기 중이면 부모로 복귀하고, 최상위 Sequence라면 기존처럼 전체 대화를 종료한다.
            CompleteCurrentSequence(true);
            return;
        }


        var line = currentSequence.lines[currentIndex];
        currentIndex++;

        if (line == null)
        {
            Debug.LogWarning("[DialogueManager] 비어 있는 대화 줄을 건너뜁니다.", currentSequence);
            ShowNextLine();
            return;
        }


        switch (line.actionType)
        {
            case DialogueActionType.Talk:
                nameText.text = line.characterName;
                portraitImage.sprite = line.portrait;
                // 이미지가 있으면 켜고, 없으면 끈다
                portraitImage.gameObject.SetActive(line.portrait != null);

                typingCoroutine = StartCoroutine(TypeText(line.text));
                break;


            case DialogueActionType.TriggerEventData:
                Debug.Log(line.text);
                EventUIManager.Instance.ShowEvent(line.triggeredEvent_Data);
                canInput = false;
                break;

            case DialogueActionType.TriggerEventEffect:
                Debug.Log(line.text);
                EventManager.Instance.Execute(GameManager.Instance.Context,line.triggeredEvent_Effect);
                ShowNextLine();
                break;

            case DialogueActionType.End:
                // End는 현재 Sequence만 종료한다. 복귀 상태가 있으면 대화창을 닫지 않고 부모로 돌아간다.
                CompleteCurrentSequence(false);
                break;

            case DialogueActionType.End_OpenMap:
                StartCoroutine(EndDialogue(true));
                break;
        }
    }

    /// <summary>
    /// 현재 Sequence 종료를 처리한다.
    /// Stack에 부모 상태가 있으면 Fade나 패널 초기화 없이 즉시 복원하고,
    /// 부모가 없을 때만 기존 EndDialogue 코루틴으로 전체 대화를 종료한다.
    /// </summary>
    private void CompleteCurrentSequence(bool invokeCompletionCallback)
    {
        if (dialogueStateStack.Count > 0)
        {
            DialogueState previousState = dialogueStateStack.Pop();
            currentSequence = previousState.sequence;
            currentIndex = previousState.index;
            canInput = true;

            // 같은 대화창을 유지한 채 부모 Sequence의 이벤트 다음 줄을 바로 실행한다.
            ShowNextLine();
            return;
        }

        StartCoroutine(EndDialogue());

        // 기존 동작과 동일하게 Sequence가 줄 끝까지 자연 종료된 경우에만 완료 콜백을 실행한다.
        if (invokeCompletionCallback)
        {
            onDialogueComplete?.Invoke();
            onDialogueComplete = null;
        }
    }

    IEnumerator TypeText(string text)
    {
        text ??= string.Empty;
        dialogueText.text = "";
        int i = 0;


        while (i < text.Length)
        {
            // 특수 대기 코드 처리: \w1, \w2 등
            if (text[i] == '\\' && i + 2 < text.Length && text[i + 1] == 'w')
            {
                string waitCode = "";
                int j = i + 2;
                while (j < text.Length && (char.IsDigit(text[j]) || text[j] == '.'))
                {
                    waitCode += text[j];
                    j++;
                }
                if (float.TryParse(waitCode, out float waitTime))
                {
                    yield return new WaitForSeconds(waitTime);
                    i = j;
                    continue;
                }
            }


            dialogueText.text += text[i];
            yield return new WaitForSeconds(typingSpeed);
            i++;
        }


        typingCoroutine = null;
    }


    public IEnumerator EndDialogue(bool mapOpen = false)
    {
        if (isEndingDialogue) yield break;

        isEndingDialogue = true;
        canInput = false;
        Debug.Log("대화 종료코루틴");
        // 1. 화면 덮기
        yield return GameUiManager.Instance.FadeIn();

        // 2. 환경 설정
        // End_OpenMap을 포함한 전체 종료에서는 남아 있는 모든 복귀 상태도 폐기한다.
        // 이후 새 대화가 과거 Sequence로 잘못 복귀하는 것을 방지한다.
        dialogueStateStack.Clear();
        currentSequence = null;
        currentIndex = 0;
        isTexting = false;
        dialoguePanel.SetActive(false);
        GameUiManager.Instance.AllPanelOff();

        if (mapOpen)
        {
            GameUiManager.Instance.MapUiOpen(false);
        }
        // 3. 잠시 대기 (너무 빠르면 깜빡이는 느낌이 들 수 있음)
        yield return new WaitForSeconds(0.1f);

        // 4. 화면 치우기
        yield return GameUiManager.Instance.FadeOut();

        canInput = true;
        isEndingDialogue = false;

    }

    private string CleanText(string rawText)
    {
        // \w숫자 or \w소수점숫자 → 제거
        return System.Text.RegularExpressions.Regex.Replace(rawText, @"\\w[0-9.]+", "");
    }

    // Call this on button click
    public void OnNextButtonClicked()
    {
        if (isEndingDialogue || currentSequence == null || currentSequence.lines == null) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            int lineIndex = currentIndex - 1;
            if (lineIndex < 0 || lineIndex >= currentSequence.lines.Count || currentSequence.lines[lineIndex] == null)
            {
                typingCoroutine = null;
                return;
            }

            string fullText = currentSequence.lines[lineIndex].text ?? string.Empty;
            dialogueText.text = CleanText(fullText);
            typingCoroutine = null;
        }
        else
        {
            ShowNextLine();
        }
    }
}
