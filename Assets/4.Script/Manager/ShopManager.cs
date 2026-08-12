using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public List<ItemData> allItems; // 전체 아이템 리스트
    public ShopSlot[] uiSlots;      // 화면에 있는 3개의 슬롯

    public GameObject closePopup;

    public Button Buy_btn;

    public ItemData selecteditem;

    [Header("텍스트들")]
    public TMP_Text shopMan_Text;
    public TMP_Text goods_name;
    public TMP_Text goods_text;
    public TMP_Text rare_text;

    [Header("상점주인 대사들")]
    public List<string> shopMan_log;
    private void Awake()
    {
        // 상점 슬롯과 팝업은 Ingame 씬 객체이므로 ShopManager도 씬과 함께 교체한다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        // 현재 인스턴스가 소유한 static 참조만 해제한다.
        if (Instance == this)
        {
            Instance = null;
        }
    }


    public void OpenShop()
    {
        // 상점 대사나 판매 아이템이 아직 채워지지 않은 개발 단계에서도
        // 빈 리스트 인덱싱으로 크래시하지 않고 준비된 슬롯만 표시한다.
        if (shopMan_Text != null)
        {
            shopMan_Text.text = shopMan_log != null && shopMan_log.Count > 0
                ? shopMan_log[Random.Range(0, shopMan_log.Count)]
                : string.Empty;
        }

        var selected = GetRandomItems(3);
        if (uiSlots == null) return;

        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (uiSlots[i] == null) continue;

            if (i < selected.Count)
            {
                uiSlots[i].Setup(selected[i]);
            }
            else
            {
                uiSlots[i].ResetData();
            }
        }
    }
    public void CloseShop()
    {
        closePopup.SetActive(false);
        Reset_Data();

        if (uiSlots == null) return;

        foreach(ShopSlot uiSlot in uiSlots)
        {
            if (uiSlot != null) uiSlot.ResetData();
        }
    }

    public void ClickGoods(ItemData data, bool canbuy)
    {
        if (data == null)
        {
            Reset_Data();
            return;
        }

        selecteditem = data;

        goods_name.text = data.itemName;
        goods_text.text = data.description;
        rare_text.text = "희귀도 : " + GameManager.Instance.RareString(data.rarity);

        Buy_btn.interactable = canbuy;
    }
    public void Reset_Data()
    {
        selecteditem = null;

        goods_name.text = null;
        goods_text.text = null;
        rare_text.text = null;
        Buy_btn.interactable= false;
    }


    // 구매 버튼을 눌렀을 때 호출될 함수
    public void BuyButton(ItemData item)
    {
        if (GameManager.Instance.Context.player.gold >= item.gold_price)
        {
            GameManager.Instance.ChangeGold(item.gold_price);
            item.OnAcquire(); // 유물/스킬 효과 발동!
            Debug.Log($"{item.itemName} 구매 완료!");
        }
    }

    public List<ItemData> GetRandomItems(int count)
    {
        if (count <= 0 || allItems == null || allItems.Count == 0)
        {
            return new List<ItemData>();
        }

        // 1. 원본 리스트 복사 (원본 보존을 위해)
        List<ItemData> shuffleList = allItems.FindAll(item => item != null);

        // 2. 셔플 알고리즘 (중복 방지용)
        for (int i = 0; i < shuffleList.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffleList.Count);
            ItemData temp = shuffleList[i];
            shuffleList[i] = shuffleList[randomIndex];
            shuffleList[randomIndex] = temp;
        }

        // 3. 요청한 개수만큼 반환
        return shuffleList.GetRange(0, Mathf.Min(count, shuffleList.Count));
    }

}
