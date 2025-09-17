using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    public Image itemImage;
    public Button useBtn;
    public TMP_Text quantityText;

    public void InitInventorySlot(InventoryItem item, int slotIndex)
    {
        itemImage.sprite = item.item.itemImage;
        itemImage.enabled = true;
        quantityText.text = item.quantity.ToString();
        useBtn.onClick.AddListener(() =>
            InventoryUIManager.instance.ShowItemDetail(item, slotIndex)
        );
    }

    public void ResetInventorySlot()
    {
        itemImage.sprite = null;
        itemImage.enabled = false;
        useBtn.onClick.RemoveAllListeners();
        quantityText.text = string.Empty;
    }
}
