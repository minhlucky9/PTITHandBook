using Interaction;  // ?? có ItemSO
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CartItemUI : MonoBehaviour
{
    public Image productImage;
    public TMP_Text nameText;
    public TMP_Text quantityText;
    public TMP_Text priceText;
  
    public Button dropBtn;

    private ItemSO itemInfo;         // l?u ItemSO g?c
    private int unitPrice;

    private int quantity;
    private CartManager manager;

    public int Quantity => quantity;
    public ItemSO ItemInfo => itemInfo;
    public int UnitPrice => unitPrice;

    public void Init(ItemSO itemInfo, int price, CartManager mgr)
    {
        this.manager = mgr;
        this.itemInfo = itemInfo;   // gán ? ?ây
        this.unitPrice = price;
        this.quantity = 1;

        productImage.sprite = itemInfo.itemImage;
        nameText.text = itemInfo.itemName;
        UpdateUI();
        manager.UpdateTotalPrice();
        dropBtn.onClick.AddListener(() => manager.RemoveFromCart(itemInfo.itemId));

    }

    public void Increment()
    {
        quantity++;
        UpdateUI();
        manager.UpdateTotalPrice();
    }

    private void UpdateUI()
    {
  
        quantityText.text = "x" + quantity;
        priceText.text = (unitPrice * quantity).ToString();
    
      
    }
}
