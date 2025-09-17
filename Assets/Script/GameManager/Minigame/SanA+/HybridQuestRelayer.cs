using System;
using UnityEngine;

public class HybridQuestRelayer : MonoBehaviour
{
    [Serializable]
    public struct StepRewardPopupBinding
    {
        public string stepId;                    // điền đúng stepId trong QuestInfoSO
        public UIAnimationController popup;      // popup UI cho bước đó
    }

    public StepRewardPopupBinding[] stepRewardPopups; // kéo thả trong Inspector

    public NPCController npc;
    NPCQuanPhoTo nPCQuanPhoTo;
    NPCThuVien npcThuVien;
    NPCDaThan npcDaThan;

    private void Awake()
    {
       
        if (npc == null)
        {
            Debug.LogError("[HybridQuestRelayer] Không tìm thấy NPCController trên GameObject.");
        }
    }

    // ====== Gọi ở "cuối hội thoại của bước" thay vì FinishQuestStep ======
    // DS ExecutedFunction: Hybrid_ShowRewardPopupForCurrentStep
    public void Hybrid_ShowRewardPopupForCurrentStep()
    {
        if (HybridQuestManager.instance == null || npc == null) return;

        // Lấy stepId hiện tại
        string questId = npc.questConversation.id;
        var quest = GameManager.QuestManager.instance.questMap[questId];
        if (!quest.isCurrentStepExists()) return;
        string stepId = quest.info.questSteps[quest.currentQuestStepIndex].stepId;

        // Tìm popup tương ứng
        UIAnimationController popup = null;
        foreach (var b in stepRewardPopups)
            if (b.stepId == stepId) { popup = b.popup; break; }

        if (popup == null)
        {
            Debug.LogWarning($"[HybridRelayer] Chưa gán popup cho stepId: {stepId}");
            return;
        }

        HybridQuestManager.instance.ShowStepRewardPopup(npc, popup);
    }

    // ====== Gắn vào Button onClick trong chính popup ======
    // Button → OnClick → Hybrid_ConfirmRewardPopupAndFinish
    public void Hybrid_ConfirmRewardPopupAndFinish()
    {
        HybridQuestManager.instance?.ConfirmStepRewardAndFinish();
    }

    // =========================
    // Step 2: NPC #2 (trả coin)
    // =========================
    // DS ExecutedFunction: Hybrid_PayCostThenFinish
    public void Hybrid_PayCostThenFinish()
    {
        nPCQuanPhoTo = GetComponent<NPCQuanPhoTo>();
        if (HybridQuestManager.instance == null || npc == null) return;
        HybridQuestManager.instance.TryPayCostThenFinish(npc, nPCQuanPhoTo);
    }

    // =========================
    // Step 3: NPC #3 (thẻ SV)
    // =========================
    // DS ExecutedFunction: Hybrid_CheckStudentCard
    public void Hybrid_CheckStudentCard()
    {
        npcThuVien = GetComponent<NPCThuVien>();
        if (HybridQuestManager.instance == null || npc == null) return;
        HybridQuestManager.instance.CheckStudentCard(npc, npcThuVien);
    }

    // DS ExecutedFunction: Hybrid_FinishStep3_EnableBreadWatcher
    public void Hybrid_FinishStep3_EnableBreadWatcher()
    {
        if (HybridQuestManager.instance == null || npc == null) return;
        HybridQuestManager.instance.FinishStep3_EnableBreadWatcher(npc);
    }

    // =========================
    // Step 4: Bánh mì (NPC3/Shop)
    // =========================
    // Gọi khi đóng hội thoại NPC3 hoặc thoát cửa hàng
    // DS ExecutedFunction: Hybrid_OnExitNPC3OrShop
    public void Hybrid_OnExitNPC3OrShop()
    {
        if (HybridQuestManager.instance == null || npc == null) return;
       // HybridQuestManager.instance.OnExitNPC3OrShop(npc);
    }

    // =========================
    // Step 6: NPC #6 (Try1→Try2→Try3)
    // =========================
    // DS ExecutedFunction: Hybrid_OnNPC6Interact
    public void Hybrid_OnNPC6Interact()
    {
        npcDaThan = GetComponent<NPCDaThan>();
        if (HybridQuestManager.instance == null || npc == null) return;
        HybridQuestManager.instance.OnNPC6Interact(npc, npcDaThan);
    }

    // DS ExecutedFunction: Hybrid_OnNPC6TryEnded  (kết thúc Try1/Try2)
    public void Hybrid_OnNPC6TryEnded()
    {
        npcDaThan = GetComponent<NPCDaThan>();
        if (HybridQuestManager.instance == null || npc == null) return;
        HybridQuestManager.instance.OnNPC6TryEnded(npc, npcDaThan);
    }

    // DS ExecutedFunction: Hybrid_FinishNPC6  (kết thúc Try3)
    public void Hybrid_FinishNPC6()
    {
        if (HybridQuestManager.instance == null || npc == null) return;
        HybridQuestManager.instance.FinishNPC6(npc);
    }
}
