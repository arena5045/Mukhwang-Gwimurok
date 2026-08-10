using TMPro;
using UnityEngine;

public class MapBtn : MonoBehaviour
{
    public GameObject MapUi;
    public TMP_Text ment;

    public void MapBtnClick()
    {
        //꺼져있었으면 false 켜져있을때 누르면 true
        bool isOpen = MapUi.activeSelf;

        MapUi.SetActive(!isOpen);
        ment.gameObject.SetActive(isOpen);

        if(!isOpen)
        {
            GameManager.Instance.canClick = false;
            Time.timeScale = 0f;
        }
        else
        {
            GameManager.Instance.canClick = true;
            Time.timeScale = 1f;
        }
       
    }
}
