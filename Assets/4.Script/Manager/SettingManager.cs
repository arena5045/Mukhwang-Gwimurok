using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    //시작버튼확인
    private bool touched = false;

    public List<CharacterData> chars;
    public List<DungeonData> dungeons;

    [HideInInspector]
    public CharacterData selectedChars;
    [HideInInspector]
    public DungeonData selectedDungeon;

    [Header("캐선창")]
    public TMP_Text chartext;
    public Image charimg;

    [Header("확인창")]
    public Image check_dungeon;
    public TMP_Text check_dungeonText;

    public Image check_char;
    public TMP_Text check_charText;

    [Header("페이드창")]
    public GameObject settingCanvas;


    private void Start()
    {
        SelecteChars(0);
     }

    public void SelecteChars(int char_num)
    {
        selectedChars = chars[char_num];

        chartext.text = selectedChars.description;
        charimg.sprite = selectedChars.fullBody;
        charimg.SetNativeSize();
    }

    public void SelecteDungeon(int dun_num)
    {
        selectedDungeon = dungeons[dun_num];
    }

    public void SettingCheck()
    {
        check_dungeon.sprite = selectedDungeon.dungeonSprite;
        check_dungeonText.text = "원정 지역 :" + selectedDungeon.dungeonName;

        check_char.sprite = selectedChars.portrait;
        check_charText.text = "선택 영웅 :" + selectedChars.charName;
    }

    public void StartClicked()
    {
        if (touched) return;
        touched = true;

        // Main 씬의 SettingManager는 씬 전환과 함께 파괴되므로 선택값을 자신이 보관할 수 없다.
        // ScriptableObject 선택값만 GameManager의 임시 요청 공간에 전달하고,
        // 실제 PlayerData와 GameContext 생성은 Ingame의 BeginNewRun에서 한 번만 수행한다.
        GameManager.RequestNewRun(selectedChars, selectedDungeon);

        //화면 줌
        settingCanvas.GetComponent<RectTransform>().DOScale(1.2f, 1f).SetEase(Ease.InOutSine);
        //여기서 다음 패널로 넘어감
        ScreenFader.Instance.FadeAtoB(null, null, "Ingame");
    }
}
