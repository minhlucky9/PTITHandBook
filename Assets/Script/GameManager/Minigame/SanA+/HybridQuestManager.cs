using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameManager;
using Interaction;
using PlayerController;
using UnityEngine;


/// <summary>
/// Manager cho HybridQuest (Trace + PhotoPhaoThi) + countdown + chuỗi popup bánh mì tích hợp sẵn.
/// Gọi Init từ HybridEventSO.Init() ở Step 1 (missionType = MINIGAME).
/// DS gọi các API thông qua HybridQuestRelayer như trước.
/// </summary>
public class HybridQuestManager : MonoBehaviour
{
    public static HybridQuestManager instance;

    [Header("Default Bread Popups (tuỳ chọn)")]
    [Tooltip("Nếu để trống, bạn có thể đăng ký runtime bằng Hybrid_RegisterBreadPopups(...)")]
    public List<UIAnimationController> defaultBreadPopups = new();

    [Header("Shop UI (tìm theo tên 'Shop' nếu để trống)")]
    public UIAnimationController shopUI;

    [Header("Sliding Puzzle")]
    public UIAnimationController slidingPuzzleBoard;     
    private UISlidingPuzzleManager slidingPuzzle;

    private class QuestStateData
    {
        public HybridEventSO data;
        public GameObject targetNPC;

        // NPC6 Try flow
        public int npc6TryCount = 0;

        public System.Action puzzleListener;
        public bool puzzleHooked = false;
        public NPCController step3Npc;                       // để biết Relayer nào hiển thị popup Step 3
        public HybridQuestRelayer step3Relayer;

        // Bánh mì (Step 4)
        public bool breadWatcherEnabled = false;
        public bool pendingBreadPopup = false;

        // Popup sequence (tích hợp)
        public List<UIAnimationController> breadPopups = new();
        public int popupIndex = 0;
        public bool popupPlaying = false;
        public bool popupNextRequested = false;
        public Coroutine popupRoutine;

        // Countdown
        public Coroutine timerRoutine;
        public float timeRemaining;

        // --- Reward Popup Gating ---
        public UIAnimationController stepRewardPopup;   // popup đang dùng cho step hiện tại
        public NPCController stepRewardNPC;             // NPC gọi popup (để finish step)
        public string gateStepId;                       // stepId khi popup được hiển thị
        public bool gateActive;                         // đang chờ người chơi bấm nút


        // --- NPC6 Try flow ---
        public bool npc6TryEndLock = false;
    }

    [System.Serializable]
    public class StepNpcBinding
    {
        [Tooltip("stepId trong QuestInfoSO (vd: step1, step2, ...)")]
        public string stepId;

        [Tooltip("Script của NPC tương ứng (NPCPhoto, NPCThuVien, NPCDaThan, NPCController, ...)")]
        public MonoBehaviour npc; // script có field 'iconDefault: GameObject'
    }

    [Header("Bindings: stepId → NPC (để bật icon theo step)")]
    public List<StepNpcBinding> stepNpcBindings = new();


    private readonly Dictionary<string, QuestStateData> states = new();

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    // ===== INIT từ HybridEventSO tại Step 1 =====
    public void InitHybridQuest(GameObject targetNPC, HybridEventSO data)
    {
        string questId = data.questId;
        if (states.ContainsKey(questId)) return;

        var st = new QuestStateData
        {
            data = data,
            targetNPC = targetNPC,
            timeRemaining = Mathf.Max(1f, data.durationSeconds)
        };

        // copy default bread popups (nếu có)
        if (defaultBreadPopups != null && defaultBreadPopups.Count > 0)
            st.breadPopups = new List<UIAnimationController>(defaultBreadPopups);

        states.Add(questId, st);

      

        // Bắt timer kiểu CollectStar
        StartTimer(questId);

        // Khi main quest hoàn tất -> tắt timer + ẩn UI
        if (QuestManager.instance.questMap.TryGetValue(questId, out var quest))
        {
            quest.OnQuestFinish += () => StopTimer(questId, hideUI: true);
        }

        RefreshStepIcon(data.questId);

    }

    // ====== TIMER ======
    private void StartTimer(string questId)
    {
        if (!states.TryGetValue(questId, out var q)) return;
        if (q.timerRoutine != null) StopCoroutine(q.timerRoutine);

        q.timerRoutine = StartCoroutine(TimerRoutine(questId, q.timeRemaining));
    }

    private void StopTimer(string questId, bool hideUI)
    {
        if (!states.TryGetValue(questId, out var q)) return;
        if (q.timerRoutine != null)
        {
            StopCoroutine(q.timerRoutine);
            q.timerRoutine = null;
        }
        if (hideUI)
            ConservationManager.instance.timerContainer.Deactivate();
    }

    private IEnumerator TimerRoutine(string questId, float duration)
    {
        float t = duration;
        ConservationManager.instance.timerContainer.Activate();

        while (t > 0f)
        {
            int minutes = (int)(t / 60f);
            int seconds = (int)(t % 60f);
            ConservationManager.instance.timerText.text = $"{minutes:00}:{seconds:00}";
            t -= Time.deltaTime;

            if (states.TryGetValue(questId, out var q))
                q.timeRemaining = Mathf.Max(0f, t);

            yield return null;
        }

        ConservationManager.instance.timerText.text = "00:00";
        OnTimerExpired(questId);
    }

    private void OnTimerExpired(string questId)
    {
        if (!states.TryGetValue(questId, out var q)) return;

        if (q.timerRoutine != null)
        {
            StopCoroutine(q.timerRoutine);
            q.timerRoutine = null;
        }
        ConservationManager.instance.timerContainer.Deactivate();

        ForceCloseAllPopupsAndDialog(questId);


        // Reset quest về CAN_START + quay NPC về HAVE_QUEST + chuyển Fail
        QuestManager.instance.UpdateQuestStep(QuestState.CAN_START, questId);
        var target = q.targetNPC;
        if (q.targetNPC != null)
        {
            q.targetNPC.SendMessage("ChangeNPCState", NPCState.HAVE_QUEST);
            q.targetNPC.SendMessage("OnQuizTimerFail");
        }

        StartCoroutine(QuestFail(target));

        states.Remove(questId);
    }

    private IEnumerator QuestFail(GameObject targetNPC)
    {
        yield return new WaitForSeconds(1f);

       

        DialogConservation correctDialog = new DialogConservation();
        DialogResponse response = new DialogResponse();

        correctDialog.message = "Thời gian đã hết. Bạn đã không thể hoàn thành nhiệm vụ. Hãy thử lại vào lần tới";
        response.executedFunction = DialogExecuteFunction.Hybrid_QuestFail;

        response.message = "Đã hiểu";
        correctDialog.possibleResponses.Add(response);
        TalkInteraction.instance.StartCoroutine(TalkInteraction.instance.SmoothTransitionToTraceMiniGame());
        StartCoroutine(ConservationManager.instance.UpdateConservation(correctDialog));

        targetNPC.SendMessage("OnQuizTimerFail");
    }

    private bool IsInStep(string questId, string stepId)
    {
        if (!QuestManager.instance.questMap.ContainsKey(questId)) return false;
        var qu = QuestManager.instance.questMap[questId];
        return qu.state == QuestState.IN_PROGRESS
            && qu.isCurrentStepExists()
            && qu.info.questSteps[qu.currentQuestStepIndex].stepId == stepId;
    }

    // =========================
    // === API cho các bước ===
    // =========================

    
    public void ShowStepRewardPopup(NPCController npc, UIAnimationController popup)
    {
        if (npc == null || popup == null) { Debug.LogWarning("[Hybrid] Popup hoặc NPC null"); return; }

        string questId = npc.questConversation.id;
        if (!states.TryGetValue(questId, out var q)) return;

        // Lấy stepId hiện tại để "gating" đúng bước
        var quest = QuestManager.instance.questMap[questId];
        if (!quest.isCurrentStepExists()) return;
        string currentStepId = quest.info.questSteps[quest.currentQuestStepIndex].stepId;

        // Ghi nhận và bật popup
        q.stepRewardNPC = npc;
        q.stepRewardPopup = popup;
        q.gateStepId = currentStepId;
        q.gateActive = true;

        StartCoroutine(ShowPopupAfterConservation(q.stepRewardPopup));   // hiển thị popup
    }

   
    public void ConfirmStepRewardAndFinish()
    {
        // Tìm quest đang chờ gate
        foreach (var kv in states)
        {
            var questId = kv.Key;
            var q = kv.Value;
            if (!q.gateActive) continue;

            // Ẩn popup (nếu còn)
            if (q.stepRewardPopup != null) q.stepRewardPopup.Deactivate();
            
            // Delay nhẹ cho animation (giữ cùng nhịp với UI hiện có của bạn)
            instance.StartCoroutine(__ConfirmAfterDelay(questId, 1.0f));
            break;
        }
    }

    private IEnumerator __ConfirmAfterDelay(string questId, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!states.TryGetValue(questId, out var q)) yield break;

        // Vẫn đúng bước? (tránh trường hợp người chơi làm gì đó khiến step đổi trước khi bấm)
        var quest = QuestManager.instance.questMap[questId];
        if (!quest.isCurrentStepExists() || quest.info.questSteps[quest.currentQuestStepIndex].stepId != q.gateStepId)
        {
            // Bước đã thay đổi, hủy gate
            q.gateActive = false;
            q.gateStepId = null;
            q.stepRewardNPC = null;
            q.stepRewardPopup = null;
            yield break;
        }

        // 1) Hoàn tất step hiện tại (ví dụ: step3)
        if (q.stepRewardNPC != null) q.stepRewardNPC.FinishQuestStep();
        else QuestManager.instance.OnFinishQuestStep(questId);

        RefreshStepIcon(questId);
        // Clear gate
        q.gateActive = false;
        q.gateStepId = null;
        q.stepRewardNPC = null;
        q.stepRewardPopup = null;

        // 2) Nếu bước kế tiếp là Step4 (bread) -> tự chạy flow bánh mì
        //    Chờ 1 frame để QuestManager kịp set currentQuestStepIndex
        yield return null;
        if (QuestManager.instance.questMap[questId].info.questSteps[
                QuestManager.instance.questMap[questId].currentQuestStepIndex].stepId
            == q.data.step4_BreadPopup)
        {
            StartCoroutine(Step4_AutoWaitAndPopup(questId));
        }
    }



    // -------------------------------STEP 2 -----------------------------------------
    public void TryPayCostThenFinish(NPCController npc, NPCQuanPhoTo nPCQuanPhoTo)
    {

        string questId = npc.questConversation.id;
        var qu = QuestManager.instance.questMap[questId];
        Debug.Log(qu.info.questSteps[qu.currentQuestStepIndex].stepId);
        if (!states.TryGetValue(questId, out var q)) return;
        if (!IsInStep(questId, q.data.step2_NPC2_cost10)) return;

        int cost = Mathf.Max(0, q.data.coinCostAtNPC2);
        if (PlayerInventory.instance.gold < cost)
        {
            nPCQuanPhoTo.SendMessage("SwitchDialogueGroupInstance", "NotEnoughCoin");
            return;
        }
        else
        {
            nPCQuanPhoTo.SendMessage("SwitchDialogueGroupInstance", "EnoughCoin");
        }

        PlayerInventory.instance.SubtractGold(cost);
        

    }

    // --------------------------------STEP 3 -------------------------------------
    public void CheckStudentCard(NPCController npc, NPCThuVien npcThuVien)
    {
        string questId = npc.questConversation.id;
        if (!states.TryGetValue(questId, out var q)) return;
        if (!IsInStep(questId, q.data.step3_NPC3_checkCard)) return;

        // Cache relayer để POPUP Step 3 sau khi thắng
       
        q.step3Relayer = npcThuVien.GetComponent<HybridQuestRelayer>();

        // Lấy refs puzzle/board
        if (slidingPuzzle == null)
        {
            if (slidingPuzzleBoard != null)
                slidingPuzzle = slidingPuzzleBoard.GetComponentInChildren<UISlidingPuzzleManager>(true);
            if (slidingPuzzle == null)
                slidingPuzzle = FindObjectOfType<UISlidingPuzzleManager>(true);
            if (slidingPuzzleBoard == null && slidingPuzzle != null)
                slidingPuzzleBoard = slidingPuzzle.GetComponent<UIAnimationController>();
        }

        // Gắn listener 1 lần
        if (slidingPuzzle != null && !q.puzzleHooked)
        {
            q.puzzleListener = () => StartCoroutine(OnSlidingPuzzleComplete(questId));
            slidingPuzzle.PuzzleCompleted += q.puzzleListener;
            q.puzzleHooked = true;
        }

        if (ConservationManager.instance != null)
        {          
            StartCoroutine(ConservationManager.instance.DeactivateConservationDialog());           
        }


        // Khoá điều khiển & mở board
        PlayerManager.instance.DeactivateController();
        if (slidingPuzzleBoard != null) slidingPuzzleBoard.Activate();
    }


    private IEnumerator OnSlidingPuzzleComplete(string questId)
    {
        if (!states.TryGetValue(questId, out var q)) yield break;

        // Tháo listener (an toàn khi chơi lại)
        if (slidingPuzzle != null && q.puzzleListener != null)
            slidingPuzzle.PuzzleCompleted -= q.puzzleListener;
        q.puzzleHooked = false;

        // Ẩn board
        if (slidingPuzzleBoard != null) slidingPuzzleBoard.Deactivate();

        // Chờ 1 giây theo yêu cầu
        yield return new WaitForSeconds(1f);

        // Mở POPUP thưởng của Step 3 (dùng relayer hiện có để map đúng popup)
        var rel = q.step3Relayer ?? q.step3Npc?.GetComponent<HybridQuestRelayer>();
        rel?.Hybrid_ShowRewardPopupForCurrentStep();
    }

    // -----------------------------------------------------------------------------

    // Kết thúc nhánh HaveCard -> enable watcher cho Step 4
    public void FinishStep3_EnableBreadWatcher(NPCController npc)
    {
        string questId = npc.questConversation.id;
        if (!states.TryGetValue(questId, out var q)) return;
        if (!IsInStep(questId, q.data.step3_NPC3_checkCard)) return;

        npc.FinishQuestStep(); // chuyển sang Step 4
                               // Không làm gì khác ở đây; phần auto popup Step4 được kích ngay sau khi user bấm nút confirm step3 (mục 4 bên dưới)
    }

    // Khi đóng hội thoại NPC3 hoặc Shop (sau khi có bánh mì)
   

   

    // =========================
    // === Chuỗi popup tích hợp
    // =========================

    /// <summary>
    /// Đăng ký danh sách popup cho quest hiện tại.
    /// Gọi 1 lần (ví dụ ở Awake của NPC3/Shop hoặc qua DS ExecutedFunction).
    /// </summary>
    public void Hybrid_RegisterBreadPopups(NPCController npc, UIAnimationController[] popups)
    {
        if (npc == null || popups == null) return;
        string questId = npc.questConversation.id;
        if (!states.TryGetValue(questId, out var q)) return;

        q.breadPopups.Clear();
        q.breadPopups.AddRange(popups);
    }

    /// <summary>
    /// Gọi từ button trong popup (onClick) để chuyển popup tiếp theo.
    /// </summary>
    public void Hybrid_BreadPopupNext()
    {
        // tìm quest đang phát chuỗi
        foreach (var kv in states)
        {
            var q = kv.Value;
            if (q.popupPlaying)
            {
                // tắt popup hiện tại (an toàn)
                if (q.popupIndex < q.breadPopups.Count && q.breadPopups[q.popupIndex] != null)
                {
                    q.breadPopups[q.popupIndex].Deactivate();
                }
                q.popupNextRequested = true;
                break;
            }
        }
    }

    private IEnumerator PlayBreadPopupSequence(string questId)
    {
        if (!states.TryGetValue(questId, out var q)) yield break;
        if (q.breadPopups == null || q.breadPopups.Count == 0) yield break;

        // 1) TẮT Conservation trước khi hiện popup đầu tiên
        if (ConservationManager.instance != null)
            yield return ConservationManager.instance.DeactivateConservationDialog();

        yield return new WaitForSeconds(1f);

        q.popupPlaying = true;
        q.popupIndex = 0;
        q.popupNextRequested = false;

        // Hiện popup đầu
        PlayerManager.instance.DeactivateController();
        SafeActivate(q.breadPopups[0]);

        while (q.popupIndex < q.breadPopups.Count)
        {
            // Đợi người chơi bấm nút (Hybrid_BreadPopupNext)
            while (!q.popupNextRequested) yield return null;

            q.popupNextRequested = false;

            // Ẩn popup hiện tại
            if (q.popupIndex < q.breadPopups.Count && q.breadPopups[q.popupIndex] != null)
                q.breadPopups[q.popupIndex].Deactivate();

            // 2) CHỜ 1 GIÂY rồi mới hiện popup kế tiếp
            yield return new WaitForSeconds(POPUP_INTERVAL);

            q.popupIndex++;
            if (q.popupIndex < q.breadPopups.Count)
                SafeActivate(q.breadPopups[q.popupIndex]);
        }
        PlayerManager.instance.ActivateController();
        q.popupPlaying = false;
        q.popupRoutine = null;
    }


    private void SafeActivate(UIAnimationController ui)
    {
        if (ui == null) return;
        ui.Activate();
    }

    // =========================
    // === Step 6 Try flow
    // =========================

    public void OnNPC6Interact(NPCController npc, NPCDaThan npcDaThan)
    {
        string questId = npc.questConversation.id;
        if (!states.TryGetValue(questId, out var q)) return;
        if (!IsInStep(questId, q.data.step6_NPC6_Try)) return;

        string group = q.npc6TryCount switch
        {
            0 => q.data.npc6_Try1,
            1 => q.data.npc6_Try2,
            _ => q.data.npc6_Try3
        };
        npcDaThan.SendMessage("SwitchDialogueGroupInstance", group);
    }

    public void OnNPC6TryEnded(NPCController npc, NPCDaThan npcDaThan)
    {
        string questId = npc.questConversation.id;
        if (!states.TryGetValue(questId, out var q)) return;
        if (!IsInStep(questId, q.data.step6_NPC6_Try)) return;

        if (q.npc6TryEndLock) return;   // chống double-fire
        q.npc6TryEndLock = true;

        // 1) đóng hội thoại
        npcDaThan.StopInteract();

        // 2) tăng đếm rồi “đặt trước” group cho lần tương tác tiếp theo
        q.npc6TryCount = Mathf.Clamp(q.npc6TryCount + 1, 0, 2);
        string nextGroup = q.npc6TryCount == 1 ? q.data.npc6_Try2 : q.data.npc6_Try3;

        npcDaThan.SetNextGroupOnNextInteract(nextGroup);

        npcDaThan.StartCoroutine(__ReleaseTryEndLock(questId));
    }

    private IEnumerator __ReleaseTryEndLock(string questId)
    {
        yield return null;               // 1 frame
        if (states.TryGetValue(questId, out var q)) q.npc6TryEndLock = false;
    }

    public void FinishNPC6(NPCController npc)
    {
        string questId = npc.questConversation.id;
        if (!states.TryGetValue(questId, out var q)) return;
        if (!IsInStep(questId, q.data.step6_NPC6_Try)) return;

        npc.FinishQuestStep(); // sang Step 7
    }

    #region Force Dialogue về NotQuest

    // Trả về true nếu NPC này KHÔNG phải lượt của họ (nên route NotQuest)
    public bool ShouldRouteNotQuest(string questId, string expectedStepId)
    {
        var map = QuestManager.instance.questMap;
        if (!map.TryGetValue(questId, out var quest)) return true;

        // Đã FINISHED thì để AfterFinish tự xử lý, không ép NotQuest
        if (quest.state == QuestState.FINISHED) return false;

        // Lấy stepId bước 1 (để cho phép NPC mở quest khi chưa IN_PROGRESS)
        string step1Id = (quest.info.questSteps != null && quest.info.questSteps.Count > 0)
            ? quest.info.questSteps[0].stepId
            : string.Empty;

        // Chưa IN_PROGRESS (REQUIREMENTS_NOT_MET/CAN_START):
        // -> chỉ cho phép NPC có stepId == step1Id
        if (quest.state != QuestState.IN_PROGRESS)
            return string.IsNullOrEmpty(expectedStepId) || expectedStepId != step1Id;

        // Đang IN_PROGRESS: phải đúng step hiện tại
        if (!quest.isCurrentStepExists()) return true;

        string currentStepId = quest.info.questSteps[quest.currentQuestStepIndex].stepId;
        return string.IsNullOrEmpty(expectedStepId) || expectedStepId != currentStepId;
    }

    // Tiện ích: ép NPC đổi group sang NotQuest (gọi khi out-of-turn)
    public void ForceNotQuest(GameObject npcGO)
    {
        if (npcGO == null) return;
        // Hầu hết NPC script đều có hàm SwitchDialogueGroup(string)
      //  npcGO.SendMessage("SwitchDialogueGroup", "NotQuest", SendMessageOptions.DontRequireReceiver);
        // Với một số NPC có hàm SwitchDialogueGroupInstance thì cũng gọi thử (nếu muốn auto mở hội thoại)
       npcGO.SendMessage("SwitchDialogueGroupInstance", "NotQuest", SendMessageOptions.DontRequireReceiver);
    }

    #endregion

    #region Transition to Next Step

    private const float POPUP_INTERVAL = 1f;

    // Đóng Conservation rồi mới hiện popup
    private IEnumerator ShowPopupAfterConservation(UIAnimationController popup)
    {
        if (ConservationManager.instance != null)
            yield return ConservationManager.instance.DeactivateConservationDialog(); // tắt hộp thoại

        yield return new WaitForSeconds(1f); // cushion nhỏ cho animation
        if (popup != null) popup.Activate();
    }

    #endregion

    #region Bread Coroutine

    private IEnumerator Step4_AutoWaitAndPopup(string questId)
    {
        if (!states.TryGetValue(questId, out var q)) yield break;

        // 1) Sau khi hoàn thành step3, đợi 1 giây rồi mới bắt đầu kiểm tra
        yield return new WaitForSeconds(1f);

        // 2) Chờ cho tới khi ĐỒNG THỜI:
        //    - Không có UI nào đang tương tác (isInteract == false)
        //    - Trong kho đã có "bánh mì"
        yield return new WaitUntil(() =>
            !PlayerManager.instance.isInteract &&                    // UI off
            PlayerInventory.instance.HasItem(q.data.breadItemId)     // đã có bánh mì
        );

        // 3) Sau khi isInteract chuyển sang false, đợi thêm 1 giây rồi mới popup
        yield return new WaitForSeconds(1f);

        // 4) Tắt hội thoại (nếu còn), rồi chạy chuỗi popup bánh mì
        if (ConservationManager.instance != null)
            yield return ConservationManager.instance.DeactivateConservationDialog();

        yield return new WaitForSeconds(0.1f);

        if (q.breadPopups != null && q.breadPopups.Count > 0)
        {
            if (q.popupRoutine != null) StopCoroutine(q.popupRoutine);
            q.popupRoutine = StartCoroutine(PlayBreadPopupSequence(questId));
            yield return q.popupRoutine;
        }

     
        QuestManager.instance.OnFinishQuestStep(questId);
        QuestManager.instance.UpdateRequirementsMetQuest();
        RefreshStepIcon(questId);
    }


    #endregion

    #region Switch Icon

    public void RefreshStepIcon(string questId)
    {
        if (!QuestManager.instance.questMap.TryGetValue(questId, out var quest)) return;
        var steps = quest.info.questSteps;
        if (steps == null || steps.Count == 0) return;

        string targetStepId = null;

        if (quest.state == QuestState.IN_PROGRESS && quest.isCurrentStepExists())
            targetStepId = steps[quest.currentQuestStepIndex].stepId;
        else if (quest.state != QuestState.FINISHED)
            targetStepId = steps[0].stepId; // trước khi start → bật icon step1
        else
        {
            // FINISHED → tắt tất cả
            foreach (var b in stepNpcBindings) SetNpcIcon(b.npc, false);
            return;
        }

        foreach (var b in stepNpcBindings)
        {
            bool on = !string.IsNullOrEmpty(b.stepId) && b.stepId == targetStepId;
            SetNpcIcon(b.npc, on);
        }
    }

    private void SetNpcIcon(MonoBehaviour npcScript, bool isOn)
    {
        if (npcScript == null) return;
        var fld = npcScript.GetType().GetField(
            "iconDefault",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fld == null) return;

        if (fld.GetValue(npcScript) is GameObject iconGo && iconGo != null)
            iconGo.SetActive(isOn);
    }

 

    private void ForceCloseAllPopupsAndDialog(string questId)
    {
        foreach (var b in stepNpcBindings)
            SetNpcIcon(b.npc, false);

        // 1) Popups thuộc quest (reward + bread)
        if (states.TryGetValue(questId, out var q))
        {
            // dừng chuỗi popup nếu đang chạy
            if (q.popupRoutine != null)
            {
                StopCoroutine(q.popupRoutine);
                q.popupRoutine = null;
            }
            q.popupPlaying = false;
            q.popupNextRequested = false;
            q.popupIndex = 0;

            // tắt reward popup (nếu có)
            if (q.stepRewardPopup != null) q.stepRewardPopup.Deactivate();

            if (slidingPuzzleBoard != null) slidingPuzzleBoard.Deactivate();

            // tắt toàn bộ bread popups đã đăng ký
            if (q.breadPopups != null)
            {
                for (int i = 0; i < q.breadPopups.Count; i++)
                    if (q.breadPopups[i] != null) q.breadPopups[i].Deactivate();
            }
        }

        // 2) Đóng Conservation (dialog) và các UI liên quan
        if (Interaction.ConservationManager.instance != null)
        {
            // dùng coroutine đóng hộp thoại: tắt response/image rồi tắt messageContainer
            StartCoroutine(Interaction.ConservationManager.instance.DeactivateConservationDialog()); // :contentReference[oaicite:0]{index=0}

            // tắt thêm các khung phụ nếu đang bật
            Interaction.ConservationManager.instance.imageQuizContainer?.Deactivate();
            Interaction.ConservationManager.instance.timerContainer?.Deactivate();
            Interaction.ConservationManager.instance.StarContainer?.Deactivate();
        }

        
        var pm = PlayerController.PlayerManager.instance;
        if (pm != null)
        {
            pm.isInteract = false;               
            pm.ActivateController();           
        }
    }

    #endregion


}
