using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopProductUI : MonoBehaviour
{
    public Image productImage;
    public TMP_Text priceText;
    public TMP_Text NameText;
    public TMP_Text descriptionText;
    public Image valueIcon;
    public Button buyBtn;

    ShopItem shopItem;

    public void Init(ShopItem item)
    {
        shopItem = item;
        productImage.sprite = item.itemInfo.itemImage;
        priceText.text = item.price.ToString();
        NameText.text = item.itemInfo.itemName;
        descriptionText.text = item.itemInfo.itemDescription;
        valueIcon.sprite = item.valueIcon;
        //
        buyBtn.onClick.AddListener(delegate
        {
           // bool isBuySuccess = shopItem.TryBuyItem();
            CartManager.instance.AddToCart(shopItem);
           
        });
    }
}
