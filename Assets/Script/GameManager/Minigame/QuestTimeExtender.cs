using System.Reflection;
using UnityEngine;
using GameManager; // QuestManager, QuestState

public static class QuestTimeExtender
{
    private const int GOLD_COST = 50;
    private const float EXTRA_SECONDS = 60f;

    /// <summary>
    /// Tăng 60s cho quest (Hybrid/Collect/Trace) đang IN_PROGRESS theo questId.
    /// Chỉ trừ 50 vàng nếu gia hạn thành công.
    /// </summary>
    public static bool TryAddOneMinute(string questId)
    {
        // Kiểm tra vàng
        if (PlayerInventory.instance == null || PlayerInventory.instance.gold < GOLD_COST)
        {
            Debug.Log("[TimeExtender] Không đủ vàng.");
            return false;
        }

        // Kiểm tra quest IN_PROGRESS
        var qm = QuestManager.instance;
        if (qm == null || !qm.questMap.TryGetValue(questId, out var qInfo) || qInfo.state != QuestState.IN_PROGRESS)
        {
            
            Debug.Log($"[TimeExtender] Quest '{questId}' không ở trạng thái IN_PROGRESS.");
            return false;
        }

        bool extended = false;

        // ---- HYBRID ----
        if (!extended && HybridQuestManager.instance != null)
        {
            var h = HybridQuestManager.instance;

            // states: Dictionary<string, QuestStateData>
            var statesF = typeof(HybridQuestManager).GetField("states", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var states = statesF?.GetValue(h);
            if (states != null)
            {
                var dictT = states.GetType();
                var containsKeyMI = dictT.GetMethod("ContainsKey");
                var has = (bool)containsKeyMI.Invoke(states, new object[] { questId });
                if (has)
                {
                    var indexer = dictT.GetProperty("Item");
                    var q = indexer.GetValue(states, new object[] { questId });
                    var qT = q.GetType();

                    var timeF = qT.GetField("timeRemaining", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var routineF = qT.GetField("timerRoutine", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    float cur = (float)timeF.GetValue(q);
                    timeF.SetValue(q, cur + EXTRA_SECONDS);

                    var routine = (Coroutine)routineF.GetValue(q);
                    if (routine != null)
                    {
                        h.StopCoroutine(routine);
                        routineF.SetValue(q, null);
                    }

                    var startTimerMI = typeof(HybridQuestManager).GetMethod("StartTimer", BindingFlags.Instance | BindingFlags.NonPublic);
                    startTimerMI?.Invoke(h, new object[] { questId });

                    extended = true;
                }
            }
        }

        // ---- COLLECT ----
        if (!extended && CollectQuestManager.instance != null)
        {
            var c = CollectQuestManager.instance;
            if (!string.IsNullOrEmpty(c.currentCollectQuestId) && c.currentCollectQuestId == questId)
            {
                var timeF = typeof(CollectQuestManager).GetField("timeRemaining", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var routineF = typeof(CollectQuestManager).GetField("CollectTimerRoutine", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var startMI = typeof(CollectQuestManager).GetMethod("StartCollectTimer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (timeF != null && startMI != null)
                {
                    float cur = (float)timeF.GetValue(c);
                    timeF.SetValue(c, cur + EXTRA_SECONDS);

                    var routine = routineF != null ? (Coroutine)routineF.GetValue(c) : null;
                    if (routine != null)
                    {
                        c.StopCoroutine(routine);
                        routineF.SetValue(c, null);
                    }

                    startMI.Invoke(c, null);
                    extended = true;
                }
            }
        }

        // ---- TRACE ----
        if (!extended && TraceQuestManager.instance != null)
        {
            var t = TraceQuestManager.instance;

            // Lấy questId hiện tại của Trace từ field private traceEvent.questId
            var traceEventF = typeof(TraceQuestManager).GetField("traceEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            var traceEvent = traceEventF?.GetValue(t);
            var qidF = traceEvent?.GetType().GetField("questId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var curId = qidF?.GetValue(traceEvent) as string;

            if (!string.IsNullOrEmpty(curId) && curId == questId)
            {
                var timeF = typeof(TraceQuestManager).GetField("timeRemaining", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var routineF = typeof(TraceQuestManager).GetField("CollectTimerRoutine", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var startMI = typeof(TraceQuestManager).GetMethod("StartCollectTimer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (timeF != null && startMI != null)
                {
                    float cur = (float)timeF.GetValue(t);
                    timeF.SetValue(t, cur + EXTRA_SECONDS);

                    var routine = routineF != null ? (Coroutine)routineF.GetValue(t) : null;
                    if (routine != null)
                    {
                        t.StopCoroutine(routine);
                        routineF.SetValue(t, null);
                    }

                    startMI.Invoke(t, null);
                    extended = true;
                }
                else
                {
                    Debug.LogWarning("[TimeExtender] TraceQuestManager chưa expose timer (timeRemaining/traceTimerRoutine/StartTraceTimer).");
                }
            }
        }

        if (!extended) 
        {
           
            return false;
        }
        

        // Trừ vàng khi gia hạn thành công
        MouseManager.instance.CloseSupportUI();
        PlayerInventory.instance.SubtractGold(GOLD_COST);
        return true;
    }
}
