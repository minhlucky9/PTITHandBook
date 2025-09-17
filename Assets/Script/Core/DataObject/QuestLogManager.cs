using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameManager;    // n?u QuestManager n?m namespace GameManager
using Interaction;
using System.Collections;    // ?? dùng Quest, QuestState

public class QuestLogManager : MonoBehaviour
{
    public static QuestLogManager instance;

    [Header("Quest Order")]
    [SerializeField] private List<QuestInfoSO> basicQuestSOList;
    [SerializeField] private List<QuestInfoSO> advancedQuestSOList;

    [Header("NPC Targets")]
    [SerializeField] private List<Transform> basicQuestNPCs;      
    [SerializeField] private List<Transform> advancedQuestNPCs;

    [Header("Prefabs & UI References")]
    [SerializeField] private GameObject questItemPrefab;   // Prefab c?a ô Quest
    [SerializeField] private Transform contentContainer;   // GameObject ch?a VerticalLayoutGroup
    [SerializeField] private Button basicButton;
    [SerializeField] private Button advancedButton;

    [Header("Completion Counter UI")]
    [SerializeField] private TextMeshProUGUI completionText;

    [Header("Current Quest UI")]
    [SerializeField] private TextMeshProUGUI currentQuestNameText;
    [SerializeField] private TextMeshProUGUI currentQuestDescText;

    private List<Quest> basicQuests;
    private List<Quest> advancedQuests;

    

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {

        basicButton.onClick.AddListener(() => ShowQuests(basicQuestSOList, basicQuestNPCs));
        advancedButton.onClick.AddListener(() => ShowQuests(advancedQuestSOList, advancedQuestNPCs));

        QuestManager.instance.OnQuestFinished += _ =>
        {
          
            UpdateCurrentQuestUI();
        };
        ShowQuests(basicQuestSOList, basicQuestNPCs);      
        UpdateBasicQuest();
        UpdateCurrentQuestUI();
    }

  

  

    /*
        private void OnEnable()
        {
            QuestManager.instance.OnQuestsInitialized += InitiateQuestLog;
        }

        private void OnDisable()
        {
            QuestManager.instance.OnQuestsInitialized -= InitiateQuestLog;
        }

        public void InitiateQuestLog()
        {
            var allQuests = QuestManager.instance.questMap.Values;


            basicQuests = new List<Quest>();
            advancedQuests = new List<Quest>();
            foreach (var q in allQuests)
            {
                if (q.info.isAdvancedQuest) advancedQuests.Add(q);
                else basicQuests.Add(q);
            }

            // 3. Thêm listener cho button
            basicButton.onClick.AddListener(() => ShowQuests(basicQuests));
            advancedButton.onClick.AddListener(() => ShowQuests(advancedQuests));

            ShowQuests(basicQuests);
        }
    */
    private void ShowQuests(List<QuestInfoSO> soList, List<Transform> npcList)
    {
        // 1. Xoá UI cũ
        foreach (Transform c in contentContainer) Destroy(c.gameObject);

        // 2. Tạo theo đúng thứ tự trong soList
        for (int i = 0; i < soList.Count; i++)
        {
            var so = soList[i];
            if (!QuestManager.instance.questMap.TryGetValue(so.id, out var quest)) continue;

            var go = Instantiate(questItemPrefab, contentContainer);
            var itemUI = go.GetComponent<QuestLogItemUI>();
            itemUI.Bind(i , quest);

            // gắn NPC target
            if (i < npcList.Count)
                itemUI.SetNPC(npcList[i]);
        }
    }

    private void UpdateCompletionCount(List<QuestInfoSO> soList)
    {
        int total = soList.Count;
        int done = 0;
        foreach (var so in soList)
        {
            if (QuestManager.instance.questMap.TryGetValue(so.id, out var q)
             && q.state == QuestState.FINISHED)  
            {
                done++;
            }
        }
        completionText.text = $"Hoàn thành {done}/{total}";
    }

    public void UpdateBasicQuest()
    {
        UpdateCompletionCount(basicQuestSOList);
  
    }

    public void UpdateAdvancedQuest()
    {
        UpdateCompletionCount(advancedQuestSOList);
    }

    public void OpenQuestLog()
    {
        ShowQuests(basicQuestSOList, basicQuestNPCs);
        UpdateCompletionCount(basicQuestSOList);
    }

    #region Show Current Quest

    public void RefreshCurrentQuestUI() 
    {
        UpdateCurrentQuestUI();
    }

    private void Update()
    {
        UpdateCurrentQuestUI();
    }

    // Tìm và hiển thị “Quest hiện tại” duy nhất trên toàn chuỗi
    private void UpdateCurrentQuestUI()
    {
        // 1) Nếu Basic chưa hoàn tất toàn bộ -> current nằm trong Basic (nếu có)
        if (!AreAllFinished(basicQuestSOList))
        {
            if (TryFindCurrentInList(basicQuestSOList, out var q, out int idx))
            {
                SetCurrentQuestUI(q, "Cơ bản", idx);
                return;
            }
            // Không có HAVE_QUEST/IN_PROGRESS trong Basic (trường hợp người chơi chưa nhận)
            SetNoCurrentQuestMessage(false); // chưa xong toàn chuỗi
            return;
        }

        // 2) Basic đã hoàn tất, xét Advanced
       else if (!AreAllFinished(advancedQuestSOList))
        {
            StartCoroutine(DelayForAdvancedQuest());

            return;
        }

        // 3) Cả Basic & Advanced đều hoàn tất
        SetNoCurrentQuestMessage(true);
    }

    private IEnumerator DelayForAdvancedQuest()
    {
        yield return new WaitForSeconds(1f);

        if (TryFindCurrentInList(advancedQuestSOList, out var q, out int idx))
        {
            SetCurrentQuestUI(q, "Nâng cao", idx);
            yield break;
        }
        SetNoCurrentQuestMessage(false);
    }

    // Kiểm tra toàn bộ list đã FINISHED chưa
    private bool AreAllFinished(List<QuestInfoSO> soList)
    {
        foreach (var so in soList)
        {
            if (!QuestManager.instance.questMap.TryGetValue(so.id, out var q)) continue;
            if (q.state != QuestState.FINISHED) return false;
        }
        return true;
    }

    // Tìm quest có state HAVE_QUEST hoặc IN_PROGRESS trong list
    private bool TryFindCurrentInList(List<QuestInfoSO> soList, out Quest current, out int index)
    {
        for (int i = 0; i < soList.Count; i++)
        {
            if (!QuestManager.instance.questMap.TryGetValue(soList[i].id, out var q)) continue;
            if (q.state == QuestState.CAN_START || q.state == QuestState.IN_PROGRESS)
            {
                current = q;
                index = i;
                return true;
            }
        }
        current = null;
        index = -1;
        return false;
    }

    // Đổ dữ liệu ra 2 TMP
    private void SetCurrentQuestUI(Quest q, string chainLabel, int indexInChain)
    {
        if (currentQuestNameText != null)
            currentQuestNameText.text = $"{chainLabel} - Nhiệm vụ {indexInChain + 1}";
        if (currentQuestDescText != null)
            currentQuestDescText.text = GetCurrentStepDescription(q);
    }

    private void SetNoCurrentQuestMessage(bool allDoneWholeChain)
    {
        if (allDoneWholeChain)
        {
            if (currentQuestNameText) currentQuestNameText.text = "Hoàn thành";
            if (currentQuestDescText) currentQuestDescText.text = "Chúc mừng!!! bạn đã hoàn thành tất cả nhiệm vụ.";
        }
        else
        {
            if (currentQuestNameText) currentQuestNameText.text = "Chưa có nhiệm vụ hiện tại";
            if (currentQuestDescText) currentQuestDescText.text = "Hãy nhận/tiếp tục nhiệm vụ tiếp theo.";
        }
    }

    private string GetCurrentStepDescription(Quest q)
    {
        var steps = q.info.questSteps;
        if (steps == null || steps.Count == 0) return "Nhiệm vụ đã hoàn thành";
        int idx = Mathf.Clamp(q.currentQuestStepIndex, 0, steps.Count - 1);
        
        return steps[idx].stepDescription;
    }

    #endregion
}
