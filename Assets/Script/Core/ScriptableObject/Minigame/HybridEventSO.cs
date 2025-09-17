using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Minigame/Hybrid Quest Event Data")]
public class HybridEventSO : MinigameDataSO
{
    [Header("Config")]
    [Tooltip("Chi phí vàng khi gặp NPC #2")]
    public int coinCostAtNPC2 = 10;

    [Tooltip("QuestItemSO ID của thẻ sinh viên")]
    public string studentCardItemId = "TheThuVien";

    [Tooltip("UseableItemSO ID của 'bánh mì'")]
    public string breadItemId = "PateBread";

    [Header("Step Ids (khớp với QuestInfoSO)")]
    public string step1_NPC1;
    public string step2_NPC2_cost10;
    public string step3_NPC3_checkCard;
    public string step4_BreadPopup;
    public string step5_NPC5;
    public string step6_NPC6_Try;
    public string step7_BackToNPC1;

    [Header("Group names tại NPC #6")]
    public string npc6_Try1 = "Try1";
    public string npc6_Try2 = "Try2";
    public string npc6_Try3 = "Try3";

    [Header("Countdown")]
    [Tooltip("Thời lượng quest (giây), hiển thị như CollectStar")]
    public float durationSeconds = 5f; // mặc định 5 phút

    public override void Init(GameObject targetGameObject)
    {
        base.Init(targetGameObject);
        // Kích hoạt giống PhotoPhaoThiEventSO
        HybridQuestManager.instance.InitHybridQuest(targetNPC, this);
    }
}
