using GameManager;
using Interaction;
using Interaction.Minigame;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragAndDropGameManager : MonoBehaviour
{
    public static DragAndDropGameManager instance;

    [HideInInspector] public Dictionary<string, DragAndDropQuest> quests = new Dictionary<string, DragAndDropQuest>();

    [Header("Slots & UI")]
    public List<InventorySlot> dropSlots;    // kéo tất cả các ô đích vào đây
    public Slider timerSlider;               // UI Slider 

    [Header("Thời gian")]
    public float totalTime = 25f;            
    private float timeRemaining;
    private int correctCount;
    private string questId;

    DialogConservation dialog = null;
    GameObject targetNPC;
    CryptogramEventSO data;
    public string currentCollectQuestId;

    // Lưu trạng thái ban đầu của các item
    private List<ItemInitialState> initialStates = new List<ItemInitialState>();

    [System.Serializable]
    public class ItemInitialState
    {
        public DraggableItem item;
        public Transform originalParent;
        public Vector3 originalPosition;
        public bool wasEnabled;
    }


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
    
    }

    void Update()
    {
        if (timeRemaining <= 0) return;

        timeRemaining -= Time.deltaTime;
        timerSlider.value = timeRemaining;

        if (timeRemaining <= 0)
            EndGame(false);
    }


    public void InitDragAndDropGame(GameObject targetNPC, CryptogramEventSO data)
    {
       StartCoroutine(ConservationManager.instance.ClearConservation());

        questId = data.questId;
        this.targetNPC = targetNPC;
        this.data = data;
        timeRemaining = totalTime;
        correctCount = 0;
        timerSlider.maxValue = totalTime;
        timerSlider.value = totalTime;

        ResetGameState();

        foreach (var slot in dropSlots)
         //   slot.gameManager = this;



        StartCoroutine(DragAndDropUIManager.instance.ActivateMiniGameUI());

        var quest = new DragAndDropQuest
        {
           
            OnFinishQuest = () =>
            {
                targetNPC.SendMessage("OnQuestMinigameSuccess");

               
            }
        };
        quests.Add(questId, quest);

        Action listener = null;
        listener = () =>
        {
            if (correctCount >= dropSlots.Count)
            {
                quest.OnFinishQuest?.Invoke();
                EndGame(true);
            }
                
        };

    }

    // Reset game về trạng thái ban đầu
    private void ResetGameState()
    {
        // Lưu trạng thái ban đầu nếu chưa có
        if (initialStates.Count == 0)
        {
            SaveInitialStates();
        }

        // Reset tất cả items về vị trí ban đầu
        foreach (var state in initialStates)
        {
            if (state.item != null)
            {
                state.item.transform.SetParent(state.originalParent);
                state.item.transform.localPosition = state.originalPosition;
                state.item.transform.localRotation = Quaternion.identity;
                state.item.enabled = state.wasEnabled;
                state.item.image.raycastTarget = true;
            }
        }

        correctCount = 0;
    }

    // Lưu trạng thái ban đầu của tất cả draggable items
    private void SaveInitialStates()
    {
        initialStates.Clear();

        // Tìm tất cả DraggableItem trong scene
        DraggableItem[] allItems = FindObjectsOfType<DraggableItem>();

        foreach (var item in allItems)
        {
            ItemInitialState state = new ItemInitialState
            {
                item = item,
                originalParent = item.transform.parent,
                originalPosition = item.transform.localPosition,
                wasEnabled = item.enabled
            };
            initialStates.Add(state);
        }
    }

    // Gọi khi thả đúng
    public void OnCorrectDrop(DraggableItem item)
    {
        correctCount++;
       item.enabled = false;   

        if (correctCount >= dropSlots.Count)
            EndGame(true);
    }

    // Gọi khi thả sai (tuỳ feedback)
    public void OnWrongDrop(DraggableItem item)
    {
        Debug.Log($"Wrong {item.itemID}");
    }

    // Kết thúc game: win=true hoặc false
    private void EndGame(bool win)
    {
        timeRemaining = 0f;

        // Vô hiệu hoá kéo thả toàn bộ
        foreach (var slot in dropSlots)
            foreach (Transform child in slot.transform)
                if (child.TryGetComponent<DraggableItem>(out var di))
                    di.enabled = false;

        if (win)
        {
            Debug.Log("Win");
            if (quests.TryGetValue(questId, out var quest))
                quest.OnFinishQuest?.Invoke();

            StartCoroutine(DragAndDropUIManager.instance.DeActivateMiniGameUI());

        }
        else
        {
            Debug.Log("Lose");
            GameManager.QuestManager.instance.UpdateQuestStep(
       QuestState.CAN_START,
        questId
    );
            quests.Remove(questId);
            targetNPC.SendMessage("ChangeNPCState", NPCState.HAVE_QUEST);
            StartCoroutine(DragAndDropUIManager.instance.DeActivateMiniGameUI());
            targetNPC.SendMessage("OnQuestMinigameFail");
        }
    }

    public class DragAndDropQuest
    {
  
        public Action OnFinishQuest;
    }

}
