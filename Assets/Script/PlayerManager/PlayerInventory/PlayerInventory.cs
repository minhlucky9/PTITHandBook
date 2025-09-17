using Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

using static GlobalResponseData_Login;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;

    [SerializeField]
    public List<InventoryItem> inventoryItems;

    public int gold ;
    public TextMeshProUGUI goldText;
    public int Medal;
    public int bonus_medal = 0;

    [field: SerializeField]
    public int Size { get; private set; } = 10;

    public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

    public event Action<int, int> OnGoldChanged;

    private void Awake()
    {
        instance = this;

        if (GlobalResponseData.FirstTimeQuest == 1)
        {
            Medal = GlobalResponseData.Medal;
            bonus_medal = GlobalResponseData.bonus_medal;

            inventoryItems = new List<InventoryItem>();
            for (int i = 0; i < GlobalResponseData.inventoryItems.Count; i++)
            {
                if (GlobalResponseData.inventoryItems[i].item != null)
                {
                    inventoryItems.Add(GlobalResponseData.inventoryItems[i]);
                
                }
                
            }
            for( int x = GlobalResponseData.inventoryItems.Count ; x < Size; x++)
            {
                inventoryItems.Add(InventoryItem.GetEmptyItem());
            }
            
        }
        else
        {
            inventoryItems = new List<InventoryItem>();
            for (int i = 0; i < Size; i++)
            {
                inventoryItems.Add(InventoryItem.GetEmptyItem());
            }
        }

      

        if(GlobalResponseData.FirstTimeQuest == 1)
        {
            gold = GlobalResponseData.gold;
        }

    }

    private void Start()
    {
        goldText.text = gold.ToString();
        OnGoldChanged += UpdateGoldText;
      
    }

    public void UpdateGoldText(int changeAmount, int goldAmount)
    {
        goldText.text = goldAmount.ToString();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(amount, gold);
    }

    public void AddBonusMedal(int quanlity)
    {
        bonus_medal+=quanlity;
    }

    public void SubtractGold(int amount)
    {
        gold -= amount;
        OnGoldChanged?.Invoke(-amount, gold);
    }

    public bool TryBuyItem(int price, ItemSO item, int quantity = 1)
    {
        int cost = price * quantity;
        if (gold >= cost)
        {
            SubtractGold(cost);
            AddItem(item, quantity);
            return true;
        } 
        else
        {
            return false;
        }
    }

    public bool TryUseItem(int slotIndex, int quantity = 1)
    {
        if(inventoryItems[slotIndex].quantity >= quantity)
        {
            inventoryItems[slotIndex].item.UseItem();
            RemoveItem(slotIndex, quantity);
            return true;
        } else
        {
            return false;
        }
    }

    public int AddItem(ItemSO item, int quantity)
    {
        if (item.isStackable == false)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                while (quantity > 0 && IsInventoryFull() == false)
                {
                    quantity -= AddNonStackableItem(item, 1);

                }
                InformAboutChange();
                return quantity;
            }
        }
        quantity = AddStackableItem(item, quantity);
        InformAboutChange();
        return quantity;
    }

    private int AddNonStackableItem(ItemSO item, int quantity)
    {
        InventoryItem newItem = new InventoryItem
        {
            item = item,
            quantity = quantity
        };
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].IsEmpty)
            {
                inventoryItems[i] = newItem;
                return quantity;
            }
        }
        return 0;
    }

    private int AddItemToFirstFreeSlot(ItemSO item, int quantity
        /*, List<ItemParameter> itemState = null*/)
    {

        InventoryItem newItem = new InventoryItem
        {
            item = item,
            quantity = quantity,

        };

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].IsEmpty)
            {
                inventoryItems[i] = newItem;
                return quantity;
            }
        }
        return 0;
    }

    private bool IsInventoryFull()
        => inventoryItems.Where(item => item.IsEmpty).Any() == false;

    private int AddStackableItem(ItemSO item, int quantity)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].IsEmpty)
                continue;
            if (inventoryItems[i].item.itemId == item.itemId)
            {
                int amountPossibleToTake =
                    inventoryItems[i].item.maxStackSize - inventoryItems[i].quantity;

                if (quantity > amountPossibleToTake)
                {
                    inventoryItems[i] = inventoryItems[i]
                        .ChangeQuantity(inventoryItems[i].item.maxStackSize);
                    quantity -= amountPossibleToTake;
                }
                else
                {
                    inventoryItems[i] = inventoryItems[i]
                        .ChangeQuantity(inventoryItems[i].quantity + quantity);
                    InformAboutChange();
                    return 0;
                }
            }
        }
        while (quantity > 0 && IsInventoryFull() == false)
        {
            int newQuantity = Mathf.Clamp(quantity, 0, item.maxStackSize);
            quantity -= newQuantity;
            AddItemToFirstFreeSlot(item, newQuantity);
        }
        return quantity;
    }

    public void RemoveItem(int itemIndex, int amount)
    {
        if (inventoryItems.Count > itemIndex)
        {
            if (inventoryItems[itemIndex].IsEmpty)
                return;
            int reminder = inventoryItems[itemIndex].quantity - amount;
            if (reminder <= 0)
                inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
            else
                inventoryItems[itemIndex] = inventoryItems[itemIndex]
                    .ChangeQuantity(reminder);

            InformAboutChange();
        }
    }

    public void AddItem(InventoryItem item)
    {
        AddItem(item.item, item.quantity);
    }

    public Dictionary<int, InventoryItem> GetCurrentInventoryState()
    {
        Dictionary<int, InventoryItem> returnValue =
            new Dictionary<int, InventoryItem>();

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].IsEmpty)
                continue;
            returnValue[i] = inventoryItems[i];
        }
        return returnValue;
    }

    public InventoryItem GetItemAt(int itemIndex)
    {
        return inventoryItems[itemIndex];
    }

    public void SwapItems(int itemIndex_1, int itemIndex_2)
    {
        InventoryItem item1 = inventoryItems[itemIndex_1];
        inventoryItems[itemIndex_1] = inventoryItems[itemIndex_2];
        inventoryItems[itemIndex_2] = item1;
        InformAboutChange();
    }

    private void InformAboutChange()
    {
        OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
    }

    /// <summary>
    /// Ki?m tra xem inventory có ít nh?t <paramref name="requiredQuantity"/> c?a <paramref name="itemId"/> hay không
    /// </summary>
    public bool HasItem(string itemId, int requiredQuantity = 1)
    {
        int total = inventoryItems
            .Where(i => !i.IsEmpty && i.item.itemId == itemId)
            .Sum(i => i.quantity);
        return total >= requiredQuantity;
    }

    /// <summary>
    /// Lo?i b? t?i ?a <paramref name="quantity"/> c?a <paramref name="itemId"/>, tr? v? s? l??ng th?c s? ?ã xóa
    /// </summary>
    public int RemoveItemById(string itemId, int quantity = 1)
    {
        int toRemove = quantity;
        for (int i = 0; i < inventoryItems.Count && toRemove > 0; i++)
        {
            var inv = inventoryItems[i];
            if (!inv.IsEmpty && inv.item.itemId == itemId)
            {
                int removed = Math.Min(inv.quantity, toRemove);
                RemoveItem(i, removed);
                toRemove -= removed;
            }
        }
        return quantity - toRemove;
    }

}

[Serializable]
public struct InventoryItem
{
    public int quantity;
    public ItemSO item;
  
    public bool IsEmpty => item == null;

    public InventoryItem ChangeQuantity(int newQuantity)
    {
        return new InventoryItem
        {
            item = this.item,
            quantity = newQuantity,
        };
    }

    public static InventoryItem GetEmptyItem()
        => new InventoryItem
        {
            item = null,
            quantity = 0,
        };
}
