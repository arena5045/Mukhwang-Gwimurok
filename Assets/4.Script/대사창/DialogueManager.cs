using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


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

        Instance = this;

        // 씬 전환에도 유지하고 싶다면 아래 주석 해제
        // DontDestroyOnLoad(gameObject);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canInput)
        {
            if(isTexting)
            {
                OnNextButtonClicked();
            }
            else
            {
                StartDialogue(testDialogue);
            }
          
        }
    }


    public void StartDialogue(DialogueSequence sequence)
    {
        isTexting = true;
        currentSequence = sequence;
        currentIndex = 0;
        dialoguePanel.SetActive(true);
        ShowNextLine();
    }
    public void StartDialogue(DialogueSequence sequence, System.Action onComplete = null)
    {
        isTexting = true;
        currentSequence = sequence;
        currentIndex = 0;
        onDialogueComplete = onComplete;

        dialoguePanel.SetActive(true);
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


        if (currentIndex >= currentSequence.lines.Count)
        {
            Debug.Log("강종띠");
            StartCoroutine(EndDialogue());
            onDialogueComplete?.Invoke(); //  콜백 실행
            onDialogueComplete = null; //  콜백 초기화
            return;
        }


        var line = currentSequence.lines[currentIndex];
        currentIndex++;


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
               StartCoroutine(EndDialogue());
                break;

            case DialogueActionType.End_OpenMap:
                StartCoroutine(EndDialogue(true));
                break;
        }
    }

    IEnumerator TypeText(string text)
    {
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
        Debug.Log("대화 종료코루틴");
        // 1. 화면 덮기
        yield return GameUiManager.Instance.FadeIn();

        // 2. 환경 설정
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

    }

    private string CleanText(string rawText)
    {
        // \w숫자 or \w소수점숫자 → 제거
        return System.Text.RegularExpressions.Regex.Replace(rawText, @"\\w[0-9.]+", "");
    }

    // Call this on button click
    public void OnNextButtonClicked()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            string fullText = currentSequence.lines[currentIndex - 1].text;
            dialogueText.text = CleanText(fullText);
            typingCoroutine = null;
        }
        else
        {
            ShowNextLine();
        }
    }
}