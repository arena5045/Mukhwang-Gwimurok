using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class ShopSlot : MonoBehaviour
{
    [SerializeField]
    private Image item_icon;
    [SerializeField]
    private Image goods_icon;

    [SerializeField]
    private Sprite gold_icon;
    [SerializeField]
    private Sprite soul_icon;
    [SerializeField]
    private TMP_Text price_text;

    public ItemData cur_item;

    public goodsType goodsType;

    public void Setup(ItemData data)
    {
        cur_item = data;

        item_icon.sprite = cur_item.icon;

        // 0이면 금화, 1이면 혼백 (Random.Range 정수형은 마지막 숫자 미포함이므로 0, 2 사용)
        goodsType = (goodsType)Random.Range(0, (int)goodsType.count);

        switch(goodsType)
        {
            case goodsType.gold :
                price_text.text = cur_item.gold_price.ToString();
                goods_icon.sprite = gold_icon;
                break;
            case goodsType.soul :
                price_text.text = cur_item.soul_price.ToString();
                goods_icon.sprite = soul_icon;
                break;
        }

    }

    public void ResetData()
    {
        cur_item = null;
        goods_icon.sprite = null;
        price_text.text = "";
    }

    public void ButtonClick()
    {
        bool canbuy = false;

        switch (goodsType)
        {
            case goodsType.gold:
                canbuy = Instance.CanBuyGold(cur_item.gold_price);
                break;
            case goodsType.soul:
                canbuy = Instance.CanBuySoul(cur_item.soul_price);
                break;
        }


        ShopManager.Instance.ClickGoods(cur_item, canbuy);
    }

    public void Refresh()
    { 
    
    
    
    }

}
