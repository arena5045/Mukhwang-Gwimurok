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

        //화면 줌
        settingCanvas.GetComponent<RectTransform>().DOScale(1.2f, 1f).SetEase(Ease.InOutSine);
        //여기서 다음 패널로 넘어감
        ScreenFader.Instance.FadeAtoB(null, null, "Ingame");
    }
}
