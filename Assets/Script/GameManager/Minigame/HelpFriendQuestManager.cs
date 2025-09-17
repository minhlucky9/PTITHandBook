using GameManager;
using Interaction.Minigame;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HelpFriendQuestManager : MonoBehaviour
{
    public static HelpFriendQuestManager instance;

    // Lưu quest theo questId
    [HideInInspector] public Dictionary<string, HelpFriendQuest> quests = new Dictionary<string, HelpFriendQuest>();
    // Lưu các listener để có thể unsubscribe
    private Dictionary<string, Action<Dictionary<int, InventoryItem>>> inventoryListeners = new Dictionary<string, Action<Dictionary<int, InventoryItem>>>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    /// <summary>
    /// Khởi tạo quest giúp đỡ bạn bè, tự động lắng nghe inventory
    /// </summary>
    public void InitHelpFriendQuest(GameObject targetNPC, HelpFriendEventSO data)
    {
        string questId = data.questId;
        // Tạo quest mới và lưu vào dictionary
        var quest = new HelpFriendQuest
        {
            requiredItemId = data.requiredItemId,
            OnFinishQuest = () =>
            {
                targetNPC.SendMessage("OnQuestMinigameSuccess");
            
                //     QuestManager.instance.questMap[questId].OnQuestFinish += OnMainQuestComplete;
                //     PlayerInventory.instance.RemoveItemById(data.requiredItemId, 1);
            }
        };
        quests.Add(questId, quest);

        // Tạo listener và subscribe vào sự kiện inventory update
        Action<Dictionary<int, InventoryItem>> listener = null;
        listener = (inventoryState) =>
        {
            // Khi inventory có item cần thiết
            if (PlayerInventory.instance.HasItem(data.requiredItemId))
            {
                // Hoàn thành quest
                quest.OnFinishQuest?.Invoke();
                // Unsubscribe sau khi hoàn thành
                PlayerInventory.instance.OnInventoryUpdated -= listener;
                inventoryListeners.Remove(questId);
                quests.Remove(questId);
            }
        };

        inventoryListeners.Add(questId, listener);
        PlayerInventory.instance.OnInventoryUpdated += listener;

        // Kiểm tra ngay lập tức phòng khi đã có sẵn item
        listener.Invoke(PlayerInventory.instance.GetCurrentInventoryState());

        QuestManager.instance
            .questMap[questId]
            .OnQuestFinish += () =>
            {
                // a) Xóa item khỏi inventory
                PlayerInventory.instance.RemoveItemById(data.requiredItemId, 1);

                // b) Dọn dẹp listener và dictionary để không leak memory
                PlayerInventory.instance.OnInventoryUpdated -= listener;
                quests.Remove(questId);
            };

    }

    private void OnMainQuestComplete()
    {
        // Callback khi quest chính hoàn tất (nếu cần)
    }
}

/// <summary>
/// Lớp nội bộ theo dõi quest giúp đỡ bạn bè
/// </summary>
public class HelpFriendQuest
{
    public string requiredItemId;
    public Action OnFinishQuest;
}
