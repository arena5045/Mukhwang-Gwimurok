using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPanel_Choice : MonoBehaviour
{
    public TMP_Text event_Maintext;

    public List<GameObject> event_Buttons;
    public List<TMP_Text> event_Texts;
    public List<TMP_Text> event_desTexts;



    public void SettingEvent(EventData data)
    {
        Initialize();

         event_Maintext.text = data.eventMainDescription;

        for (int i = 0; i < data.choices.Count; i++)
        {
            if (i >= event_Buttons.Count) break;

            var choice = data.choices[i];

            event_Buttons[i].SetActive(true);
            event_Texts[i].text = choice.choiceText;
            event_desTexts[i].text = choice.descriptText;

            // 선택지에 연결된 유니티 이벤트 실행 연결
            int capturedIndex = i; // 
            event_Buttons[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                EventChoice selectedChoice = data.choices[capturedIndex];
                EventManager.Instance.Execute(GameManager.Instance.Context, selectedChoice);

                //원래쓰던 초이스 실행함수
                //data.choices[capturedIndex].Execute(GameManager.Instance.Context);

                // 선택 UI를 먼저 닫은 뒤 분기 대화를 시작해야 같은 화면에서 두 UI가 겹치지 않는다.
                this.gameObject.SetActive(false);

                // nextDialogue가 비어 있으면 EventEnd 내부에서 기존 대화의 다음 줄로 바로 진행한다.
                EventUIManager.Instance.EventEnd(selectedChoice.nextDialogue);
            });
        }
    }


    public void Initialize()
    {
        event_Maintext.text = "";

        for (int i = 0; i < event_Texts.Count; i++)
        {
            event_Texts[i].text = "";
            event_Buttons[i].GetComponent<Button>().onClick.RemoveAllListeners();
            event_Buttons[i].SetActive(false);
        }
    }
}
