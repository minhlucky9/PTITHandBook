using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class CartManager : MonoBehaviour
{
    public static CartManager instance;

    [Header("References")]
    public Transform cartContainer;    // Vertical Layout Group
    public GameObject cartItemPrefab;  // Prefab chứa CartItemUI
    public Button checkoutBtn;
    public TMP_Text TotalPriceText;
    public Action OnBuyItemSuccess;
    int totalPrice = 0;

    // lưu quantity theo itemId
    private Dictionary<string, int> cartData = new Dictionary<string, int>();
    private Dictionary<string, CartItemUI> uiItems = new Dictionary<string, CartItemUI>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        checkoutBtn.onClick.AddListener(Checkout);
    }

    public void UpdateTotalPrice()
    {
        int sum = 0;
        foreach (var itemUI in uiItems.Values)
        {
            sum += itemUI.UnitPrice * itemUI.Quantity;
        }

        totalPrice = sum;
        TotalPriceText.text = "Tổng: " + sum.ToString();
    }

    // gọi từ ShopProductUI khi bấm “Mua”
    public void AddToCart(ShopItem shopItem)
    {
        string id = shopItem.itemInfo.itemId;
        if (cartData.ContainsKey(id))
        {
            cartData[id]++;
            uiItems[id].Increment();
        }
        else
        {
            cartData[id] = 1;
            GameObject go = Instantiate(cartItemPrefab, cartContainer);
            var ui = go.GetComponent<CartItemUI>();
            ui.Init(shopItem.itemInfo, shopItem.price, this);
            uiItems[id] = ui;
        }
        UpdateTotalPrice();
        
    }

    // xóa 1 mục khỏi giỏ
    public void RemoveFromCart(string itemId)
    {
        if (!cartData.ContainsKey(itemId)) return;
        cartData.Remove(itemId);
        Destroy(uiItems[itemId].gameObject);
        uiItems.Remove(itemId);
        UpdateTotalPrice();
    }

    // thanh toán toàn bộ
    private void Checkout()
    {
        if(PlayerInventory.instance.gold <= totalPrice)
        {
            Debug.LogWarning($"Không đủ tiền để mua vật phẩm");
            return;
        }

        foreach (var kv in cartData)
        {
            string id = kv.Key;
            int qty = kv.Value;
            CartItemUI uiItem = uiItems[id];

            // Truyền đúng ItemSO và giá gốc
            bool ok = PlayerInventory.instance
                           .TryBuyItem(uiItem.UnitPrice, uiItem.ItemInfo, qty);

            if (!ok)
            {
                Debug.LogWarning($"Không đủ tiền để mua {uiItem.nameText.text}");
                return;
            }
        }

        OnBuyItemSuccess?.Invoke();
        ClearCart();
       
    }

    public void ClearCart()
    {
        foreach (var ui in uiItems.Values)
            Destroy(ui.gameObject);
        cartData.Clear();
        uiItems.Clear();
        UpdateTotalPrice();
    }
}
