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
                EventManager.Instance.Execute(GameManager.Instance.Context, data.choices[capturedIndex]);

                //원래쓰던 초이스 실행함수
                //data.choices[capturedIndex].Execute(GameManager.Instance.Context);

                EventUIManager.Instance.EventEnd(); // 이벤트 매니저에서 종료 이벤트 호출
                this.gameObject.SetActive(false); // UI 닫기 등
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
