using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Interaction;

public class InventoryItemDetailUI : MonoBehaviour
{
    public Image itemImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public Button dropBtn;
    public Button useBtn;

    private int slotIndex;
    private InventoryItem currentItem;

    /// <summary>
    /// Khởi tạo preview
    /// </summary>
    public void InitDetail(InventoryItem item, int slotIndex)
    {
        this.slotIndex = slotIndex;
        this.currentItem = item;

        // fill UI
        itemImage.sprite = item.item.itemImage;
        nameText.text = item.item.itemName;
        descriptionText.text = item.item.itemDescription;

        // Drop luôn toàn bộ quantity
        dropBtn.onClick.AddListener(OnDropClicked);

        // Chỉ show/use nếu item thật sự implement UseItem()
        if (item.item is UsableItemSO)
        {
            useBtn.onClick.AddListener(OnUseClicked);
        }
        else
        {
            useBtn.gameObject.SetActive(false);
        }
    }

    private void OnDropClicked()
    {
        // xoá hết số lượng trong slot
        PlayerInventory.instance.RemoveItem(slotIndex, currentItem.quantity);
        InventoryUIManager.instance.RefreshUI();
        Destroy(gameObject); // đóng preview
    }

    private void OnUseClicked()
    {
        // thử dùng 1 cái
        PlayerInventory.instance.TryUseItem(slotIndex, 1);
        InventoryUIManager.instance.RefreshUI();
        Destroy(gameObject);
    }
}
