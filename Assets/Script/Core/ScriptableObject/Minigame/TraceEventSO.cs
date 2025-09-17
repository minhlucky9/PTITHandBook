// TraceEventSO.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Minigame/Trace Event Data")]
public class TraceEventSO : MinigameDataSO
{
    [Serializable]
    public class TraceObject
    {
        public GameObject prefab;
    }

    [Tooltip("Danh sách 6 vật phẩm sẽ xuất hiện tuần tự")]
    public List<TraceObject> traceObjects;

    public override void Init(GameObject targetGameObject)
    {
        base.Init(targetGameObject);
        // gọi manager để khởi tạo quest tuần tự
        TraceQuestManager.instance.InitTraceQuest(targetGameObject, this);
    }
}
