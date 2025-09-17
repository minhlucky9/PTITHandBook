using GameManager;
using PlayerController;
using PlayerStatsController;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : MonoBehaviour
{
    public static MouseManager instance;
    public GameObject menuUI;
    public GameObject pauseUI;
    public UIAnimationController QuestUI;
    public UIAnimationController LeaderBoardUI;
    public UIAnimationController SupportUI;


    [Header("Minigame - Extend Time")]
    public UIAnimationController TimeExtendUI;
    public UIAnimationController NotInQuestUI;


    [Header("QR")]
    public UIAnimationController QR_UI;
    public UIAnimationController Finish_UI;

    [Header("NavMesh")]
    public UIAnimationController NavMeshUI;

    [Header("PassWordMiniGame_3")]
    public UIAnimationController PassWordMiniGame_3_UI;



    public enum MousePermission
    {
        All,
        OnlyQuest,
        OnlyLeaderBoard,
        Inventory,
        EnterPress,
        None,
        OnlySupport
    }

    public MousePermission permission = MousePermission.All;

    void Awake()
    {
        instance = this;
        // Ẩn con trỏ chuột khi bắt đầu game
        Cursor.visible = false;
        // Khóa con trỏ chuột ở giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (permission != MousePermission.None)
        {
            if (permission == MousePermission.All && Input.GetKeyDown(KeyCode.Escape) && !PlayerManager.instance.isInteract)
            {
                pauseUI.SetActive(true);
                PlayerManager.instance.DeactivateController();
                PlayerManager.instance.isInteract = true;
                ShowCursor();
            }

            if ((permission == MousePermission.All || permission == MousePermission.OnlyQuest)
                && Input.GetKeyDown(KeyCode.Q) && !PlayerManager.instance.isInteract)
            {
                
                QuestLogManager.instance.OpenQuestLog();
                QuestUI.UpdateObjectChange();
                QuestUI.Activate();

                PlayerManager.instance.DeactivateController();
                PlayerManager.instance.isInteract = true;
                ShowCursor();
                if (TutorialManager.Instance.isRunning)
                {
                    TutorialManager.Instance.currentUI.Deactivate();
                    TutorialManager.Instance.ShowNextStepDelayed();
                    permission = MousePermission.None; 

                }
            }
            if ((permission == MousePermission.All || permission == MousePermission.OnlyLeaderBoard)
                && Input.GetKeyDown(KeyCode.I) && !PlayerManager.instance.isInteract)
            {
                LeaderBoardUI.Activate();           
                PlayerManager.instance.DeactivateController();
                PlayerManager.instance.isInteract = true;
                ShowCursor();
            }
            if ((permission == MousePermission.All || permission == MousePermission.OnlySupport)
                && Input.GetKeyDown(KeyCode.P) && !PlayerManager.instance.isInteract)
            {
                SupportUI.Activate();
                PlayerManager.instance.DeactivateController();
                PlayerManager.instance.isInteract = true;
                ShowCursor();
            }
            if ((permission == MousePermission.All || permission == MousePermission.Inventory)
              && Input.GetKeyDown(KeyCode.M) && !PlayerManager.instance.isInteract)
            {
                InventoryUIManager.instance.OpenInventory();
                if (TutorialManager.Instance.isRunning)
                {
                    TutorialManager.Instance.currentUI.Deactivate();
                    TutorialManager.Instance.ShowNextStepDelayed();
                    permission = MousePermission.None;

                }
            }
           
        }
    }
    public void ShowCursor()
    {
        // Hiện con trỏ chuột
        Cursor.visible = true;
        // Mở khóa con trỏ chuột
        Cursor.lockState = CursorLockMode.None;
    }


    public void HideCursor()
    {
        if (PlayerManager.instance.isTalkingWithNPC) return;
        // Ẩn con trỏ chuột
        Cursor.visible = false;
        // Khóa con trỏ chuột
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenInteract()
    {
        PlayerManager.instance.isInteract = false;
  
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void CloseQuestUI()
    {
        QuestUI.Deactivate();
        PlayerManager.instance.isInteract = false;
        PlayerManager.instance.ActivateController();
        HideCursor();
    }

    public void CloseSupportUI()
    {
        SupportUI.Deactivate();
        PlayerManager.instance.isInteract = false;
        PlayerManager.instance.ActivateController();
        HideCursor();
    }

    public void CloseLeaderBoardUI()
    {
        LeaderBoardUI.UpdateObjectChange();
        LeaderBoardUI.Deactivate();
        PlayerManager.instance.isInteract = false;
        PlayerManager.instance.ActivateController();
        HideCursor();
    }

    public void CloseQR_UI()
    {
        QR_UI.Deactivate();
        PlayerManager.instance.isInteract = false;
        PlayerManager.instance.ActivateController();
        HideCursor();
    }

    public void CloseAdvancedFinish_UI()
    {
        Finish_UI.Deactivate();
        PlayerManager.instance.isInteract = false;
        PlayerManager.instance.ActivateController();
        HideCursor();
    }

    public void CloseNavMeshUI()
    {
        NavMeshUI.Deactivate();
        PlayerManager.instance.isInteract = false;
        PlayerManager.instance.ActivateController();
        HideCursor();
    }

    public void OpenNavMeshUI()
    {
        NavMeshUI.Activate();
        PlayerManager.instance.isInteract = true;
        PlayerManager.instance.DeactivateController();
        ShowCursor();
    }

    public void ClosePassWord_3_UI()
    {
        PassWordMiniGame_3_UI.Deactivate();
        PlayerManager.instance.isInteract = false;
        PlayerManager.instance.ActivateController();
        HideCursor();
    }

    #region Extend Time

    public void OnClickExtendTime()
    {
        string questId = FindActiveEligibleQuestId();
        if (string.IsNullOrEmpty(questId))
        {
            NotInQuestUI.Activate();
            Debug.Log("[ExtendTime] Không có quest Hybrid/Collect/Trace nào đang IN_PROGRESS.");
            return;
        }

        if (QuestTimeExtender.TryAddOneMinute(questId))
        {
            Debug.Log($"[ExtendTime] +60s cho '{questId}'. Đã trừ 50 vàng.");
        }
        else
        {
            TimeExtendUI.Activate();
            Debug.Log("[ExtendTime] Gia hạn thất bại (có thể không đủ vàng / quest không còn IN_PROGRESS / Trace chưa expose timer).");
        }
    }

    // Tìm questId của 1 trong 3 loại Hybrid/Collect/Trace đang IN_PROGRESS
    private string FindActiveEligibleQuestId()
    {
        var qm = QuestManager.instance;
        if (qm == null) return null;

        // 1) Collect
        if (CollectQuestManager.instance != null)
        {
            var id = CollectQuestManager.instance.currentCollectQuestId;
            if (!string.IsNullOrEmpty(id) && qm.questMap.TryGetValue(id, out var q) && q.state == QuestState.IN_PROGRESS)
                return id;
        }

        // 2) Hybrid (bất kỳ questId nào có trong dictionary 'states' và đang IN_PROGRESS)
        if (HybridQuestManager.instance != null)
        {
            var h = HybridQuestManager.instance;
            var statesF = typeof(HybridQuestManager).GetField("states", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var states = statesF?.GetValue(h);
            if (states != null)
            {
                var dictT = states.GetType();
                var keysProp = dictT.GetProperty("Keys");
                var keys = keysProp?.GetValue(states) as System.Collections.IEnumerable;
                if (keys != null)
                {
                    foreach (var k in keys)
                    {
                        string id = k as string;
                        if (!string.IsNullOrEmpty(id) && qm.questMap.TryGetValue(id, out var q) && q.state == QuestState.IN_PROGRESS)
                            return id;
                    }
                }
            }
        }

        // 3) Trace (lấy questId hiện tại từ field private traceEvent.questId)
        if (TraceQuestManager.instance != null)
        {
            var t = TraceQuestManager.instance;
            var traceEventF = typeof(TraceQuestManager).GetField("traceEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            var traceEvent = traceEventF?.GetValue(t);
            var questIdF = traceEvent?.GetType().GetField("questId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var id = questIdF?.GetValue(traceEvent) as string;

            if (!string.IsNullOrEmpty(id) && qm.questMap.TryGetValue(id, out var q) && q.state == QuestState.IN_PROGRESS)
                return id;
        }

        return null;
    }

    #endregion

    #region Healing Suuport

    public void HealingSupport()
    {
        if(PlayerInventory.instance.gold < 50)
        {
            TimeExtendUI.Activate();
            return;
        }
        HealthBar.instance.SetCurrentHealth(HealthBar.instance.slider.maxValue);
        PlayerInventory.instance.SubtractGold(50);
        CloseSupportUI();
    }

    #endregion
}
