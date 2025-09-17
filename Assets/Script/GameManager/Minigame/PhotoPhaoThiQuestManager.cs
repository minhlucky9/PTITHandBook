using System;
using UnityEngine;
using GameManager;
using System.Collections.Generic;
public class PhotoPhaoThiQuestManager : MonoBehaviour
{
    public static PhotoPhaoThiQuestManager instance;

    [HideInInspector] public Dictionary<string, PhotoPhaothiQuest> quests = new Dictionary<string, PhotoPhaothiQuest>();

    private Dictionary<string, Action<Dictionary<int, InventoryItem>>> PhotoPhaothi_inventoryListeners = new Dictionary<string, Action<Dictionary<int, InventoryItem>>>();

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
    /// ??ng ký logic cho quest Photo Phao Thi
    /// </summary>
    public void InitPhotoPhaoThiQuest(GameObject targetNPC, PhotoPhaoThiEventSO data)
    {
        string questId = data.questId;

        // T?o quest m?i và l?u vào dictionary
        var quest = new PhotoPhaothiQuest
        {
            requiredItemId = data.rewardItemId,
            OnFinishQuest = () =>
            {
                targetNPC.SendMessage("OnQuestMinigameSuccess");
                
            }
        };
        quests.Add(questId, quest);


        Action<Dictionary<int, InventoryItem>> listener = null;
        listener = (inventoryState) =>
        {
          
            if (PlayerInventory.instance.HasItem(data.rewardItemId))
            {
              
                quest.OnFinishQuest?.Invoke();
           
                PlayerInventory.instance.OnInventoryUpdated -= listener;
                PhotoPhaothi_inventoryListeners.Remove(questId);
                quests.Remove(questId);
            }
        };

        PhotoPhaothi_inventoryListeners.Add(questId, listener);
        PlayerInventory.instance.OnInventoryUpdated += listener;

       
        listener.Invoke(PlayerInventory.instance.GetCurrentInventoryState());

        QuestManager.instance.questMap[questId]
            .OnQuestFinish += () =>
            {
                PlayerInventory.instance.RemoveItemById(data.rewardItemId, 1);
            };
    }

    public class PhotoPhaothiQuest
    {
        public string requiredItemId;
        public Action OnFinishQuest;
    }
}
