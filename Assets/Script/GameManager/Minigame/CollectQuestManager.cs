
using GameManager;
using Interaction;
using Interaction.Minigame;
using PlayerController;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectQuestManager : MonoBehaviour
{
    public static CollectQuestManager instance;

    [HideInInspector] public Dictionary<string, CollectQuest> collectQuests = new Dictionary<string, CollectQuest>();
    public string currentCollectQuestId;
    GameObject container;
    GameObject targetNPC;
    CollectQuest __colectquest;
    LootEventSO lootEvent;
    private Coroutine CollectTimerRoutine;
    private float timeRemaining;
    private const float COLLECT_DURATION = 480f;

    private CoinLootInteraction[] coins;
    private int currentPointerIndex = 0;
    private bool[] coinsCollected;

    private void Awake()
    {
        instance = this;
    }

    public void InitCollectQuest(GameObject targetNPC, GameObject coinContainer, LootEventSO lootEvent)
    {
        container = coinContainer;
        timeRemaining = COLLECT_DURATION;
        this.targetNPC = targetNPC;
        this.lootEvent = lootEvent;
        currentCollectQuestId = lootEvent.questId;
        //
         coins = coinContainer.GetComponentsInChildren<CoinLootInteraction>();
        currentPointerIndex = 0;

        coinsCollected = new bool[coins.Length];
        for (int i = 0; i < coinsCollected.Length; i++)
        {
            coinsCollected[i] = false;
        }

        foreach (CoinLootInteraction coin in coins)
        {
            coin.SetupCoinMinigame(lootEvent.minigameId);
        }

        //setup collect quest
        CollectQuest collectQuest = new CollectQuest();
        __colectquest = collectQuest;
        collectQuest.numberToCollect = lootEvent.numberOfLoot;
        collectQuest.OnFinishQuest = () => {
            targetNPC.SendMessage("FinishQuestStep");
            StartCoroutine(HideContainer());
            ConservationManager.instance.StarContainer.Deactivate();
            if(CollectTimerRoutine != null)
            {
                StopCoroutine(CollectTimerRoutine);
                CollectTimerRoutine = null;
            }

            // ẩn UI timer
            ConservationManager.instance.timerContainer.Deactivate();
            QuestManager.instance.questMap[lootEvent.questId].OnQuestFinish += OnMainQuestComplete;
            HideAllPointers();
        };
        collectQuests.Add(lootEvent.minigameId, collectQuest);
        ConservationManager.instance.StarContainer.Activate();
        ConservationManager.instance.StarText.text =
             "Số lá cờ thu thập: " + $"0/{collectQuest.numberToCollect}";
        UpdatePointer();
        Invoke(nameof(StartCollectTimer), 0.5f);
        

    }

    private void StartCollectTimer()
    {
        // Tránh gọi nhiều lần
        if (CollectTimerRoutine != null)
        {
            StopCoroutine(CollectTimerRoutine);
        }

        CollectTimerRoutine = StartCoroutine(CollectCountdown(timeRemaining));
    }

    private IEnumerator CollectCountdown(float duration)
    {
        float t = duration;
        ConservationManager.instance.timerContainer.Activate();


        while (t > 0f)
        {
            // tính phút và giây
            int minutes = (int)(t / 60);
            int seconds = (int)(t % 60);
            // format “MM:SS”
            ConservationManager.instance.timerText.text = $"{minutes:00}:{seconds:00}";

            t -= Time.deltaTime;
            timeRemaining = t;
            yield return null;
        }

        // khi hết giờ
        ConservationManager.instance.timerText.text = "00:00";
        OnCollectTimerExpired();
    }

    private void OnCollectTimerExpired()
    {
        // dừng coroutine nếu còn chạy
        if (CollectTimerRoutine != null)
        {
            StopCoroutine(CollectTimerRoutine);
            CollectTimerRoutine = null;
        }

        // ẩn UI timer
        ConservationManager.instance.timerContainer.Deactivate();
        ConservationManager.instance.StarContainer.Deactivate();
        HideAllPointers();
        Destroy(container);
        collectQuests.Remove(lootEvent.minigameId);
        // reset quest về CAN_START
        GameManager.QuestManager.instance.UpdateQuestStep(
         QuestState.CAN_START,
          currentCollectQuestId
      );

        targetNPC.SendMessage("ChangeNPCState", NPCState.HAVE_QUEST);

        DialogConservation correctDialog = new DialogConservation();
        DialogResponse response = new DialogResponse();

        correctDialog.message = "Thời gian đã hết. Bạn đã không thể hoàn thành nhiệm vụ. Hãy thử lại vào lần tới";
        response.executedFunction = DialogExecuteFunction.OnQuestMinigameFail;

        response.message = "Đã hiểu";
        correctDialog.possibleResponses.Add(response);
        TalkInteraction.instance.StartCoroutine(TalkInteraction.instance.SmoothTransitionToTraceMiniGame());
        StartCoroutine(ConservationManager.instance.UpdateConservation(correctDialog));

        targetNPC.SendMessage("OnQuizTimerFail");
    }

    public void OnMainQuestComplete()
    {
        HideAllPointers();
        Destroy(container);
    }

    public void OnCollectQuestChange(string minigameId)
    {
        CollectQuest quest = collectQuests[minigameId];
        quest.OnCollectedChange();
        
        UpdatePointer();
    }

    IEnumerator HideContainer()
    {
        yield return new WaitForSeconds(3f);
        container.SetActive(false);
    }

    #region Position Flag

    private void UpdatePointer()
    {
        // Ẩn tất cả pointer trước
        HideAllPointers();

        // Tìm lá cờ tiếp theo chưa được thu thập
        for (int i = 0; i < coins.Length; i++)
        {
            if (!coinsCollected[i]) // Kiểm tra trạng thái thu thập thay vì activeInHierarchy
            {
                // Hiển thị pointer trên lá cờ này
                ShowPointerOnCoin(coins[i]);
                currentPointerIndex = i;
                return;
            }
        }

        currentPointerIndex = coins.Length;
    }

    private void ShowPointerOnCoin(CoinLootInteraction coin)
    {
        // Tìm pointer trong children của coin
        Transform pointer = coin.transform.Find("location_flag (1)");
        if (pointer != null)
        {
            pointer.gameObject.SetActive(true);
        }
    }

    private void HideAllPointers()
    {
        if (coins == null) return;

        foreach (var coin in coins)
        {
            if (coin != null)
            {
                Transform pointer = coin.transform.Find("location_flag (1)");
                if (pointer != null)
                {
                    pointer.gameObject.SetActive(false);
                }
            }
        }
    }

    public void MarkCoinAsCollected(CoinLootInteraction collectedCoin)
    {
        // Kiểm tra null safety
        if (coins == null || coinsCollected == null || collectedCoin == null) return;

        // Tìm index của coin được thu thập
        for (int i = 0; i < coins.Length; i++)
        {
            if (coins[i] == collectedCoin)
            {
                coinsCollected[i] = true;
                break;
            }
        }
    }

    #endregion

}

public class CollectQuest
{
    public int numberToCollect;
    public int currentCollected;

    public Action OnFinishQuest;

    public void OnCollectedChange()
    {
        currentCollected++;
        ConservationManager.instance.StarText.text = "Số lá cờ thu thập: " + $"{currentCollected}/{numberToCollect}";
        Debug.Log(currentCollected + "/" + numberToCollect);
        if(currentCollected == numberToCollect)
        {
            OnFinishQuest?.Invoke();
        }
    }
}
