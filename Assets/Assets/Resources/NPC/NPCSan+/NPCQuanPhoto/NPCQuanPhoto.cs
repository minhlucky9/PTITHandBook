using DS;
using DS.ScriptableObjects;
using GameManager;
using Interaction;
using Interaction.Minigame;
using PlayerStatsController;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCQuanPhoTo : TalkInteraction, IDialogueHandler
{
    [Header("NPC infos")]
    public NPCInfoSO npcInfo;
    public DSDialogue dsDialogue;
    public DSDialogueContainerSO container;
    public NPCState currentNPCState;
    public QuizConservationSO quizConversation;
    public QuestInfoSO questConversation;
    public string stepId = "step2";

    // Nếu dùng phương pháp manager-based, chuyển qua sử dụng DialogueManager.Instance.
    // Nếu false thì sử dụng component riêng trên NPC.
    [Header("Adapter Options")]
    public bool useManagerBasedAdapter = false;

    [Header("Default Dialogue State")]
    private DSDialogueGroupSO defaultGroup;    // lưu nhóm ban đầu
    private DSDialogueSO defaultDialogue; // lưu node ban đầu

    // Adapter sẽ được lấy theo 2 cách ở dưới:
    private DSDialogueAdapter dialogueAdapter;

    Dictionary<string, NPCConservationSO> npcConservationMap;
    Dictionary<string, MinigameDataSO> minigameDataMap;
    Dictionary<string, FunctionalWindowDataSO> functionalDataMap;

    [HideInInspector]
    public NPCConservationSO currentDialogConservation;
    QuestManager questManager;

    [Header("Quest Mission")]
    string questId;
    QuestStep questStep;


    [HideInInspector] public string lastDialogueId;
    [HideInInspector] public DialogConservation pausedDialog;      // lưu lại dialog đang pause
    [HideInInspector] public bool isConversationPaused = false;

    [Header("State Symbol")]
    public GameObject iconDefault;
    public GameObject iconQuestAvailable;
    public GameObject iconQuestComplete;
    public GameObject iconQuestInProgress;

    public NPCInfoSO NpcInfo => npcInfo;
    public DialogConservation PausedDialog { get => pausedDialog; set => pausedDialog = value; }
    public bool IsConversationPaused { get => isConversationPaused; set => isConversationPaused = value; }

    DialogConservation conv;

    DSDialogueSO startDialogue ;

    void Start()
    {
        if (container == null)
        {

            return;
        }

        QuestManager.instance.questMap[questConversation.id]
          .OnStateChanged += () =>
          {
              Debug.Log("Quest state changed: " + questConversation.id);
              QuestState state = QuestManager.instance.questMap[questConversation.id].state;

              // Xác định tên group tương ứng
              string groupName;
              switch (state)
              {
                  case QuestState.IN_PROGRESS:
                      groupName = "NPCPhoto";
                      break;
                  case QuestState.FINISHED:
                      groupName = "NotQuest";
                      break;
                  default:
                      groupName = "NotQuest";
                      break;
              }

              // Chuyển sang nhóm đã chọn
              SwitchDialogueGroup(groupName);
          };


        if (useManagerBasedAdapter)
        {

        }
        else
        {

            dialogueAdapter = GetComponent<DSDialogueAdapter>();
            if (dialogueAdapter == null)
            {
                dialogueAdapter = gameObject.AddComponent<DSDialogueAdapter>();
            }




            dialogueAdapter.Initialize(container);
            defaultGroup = dsDialogue.dialogueGroup;
            defaultDialogue = dsDialogue.dialogue;
        }



    }

    public override void Awake()
    {
        base.Awake();
        questManager = FindAnyObjectByType<QuestManager>();

   

        // Tạo các map dữ liệu NPC, minigame và functional
        npcConservationMap = CreateNPCConservationMap();
        minigameDataMap = CreateMinigameDataMap();
        functionalDataMap = CreateFunctionalDataMap();

        currentDialogConservation = npcInfo.normalConservation;
        QuizConservationSO[] allQuiz = Resources.LoadAll<QuizConservationSO>("NPC");
        foreach (QuizConservationSO quiz in allQuiz)
        {
            if (quiz.questId == questConversation.id)
            {
                quizConversation = quiz;
            }
        }
    }

    public override void Interact()
    {
        base.Interact();


        if (HybridQuestManager.instance != null &&
             HybridQuestManager.instance.ShouldRouteNotQuest(questConversation.id, stepId))

        {
            DSDialogueGroupSO afterFinishGroup = null;
            DSDialogueSO startingDialogue = null;
            foreach (var groupEntry in container.DialogueGroups)
            {
                if (groupEntry.Key.GroupName == "NotQuest")
                {
                    afterFinishGroup = groupEntry.Key;
                    // Tìm dialogue khởi đầu trong group này
                    foreach (var dialogue in groupEntry.Value)
                    {
                        if (dialogue.IsStartingDialogue)
                        {
                            startingDialogue = dialogue;
                            break;
                        }
                    }
                    break;
                }
            }

            if (afterFinishGroup != null && startingDialogue != null)
            {
                // Thay đổi dsDialogue
                dsDialogue.dialogueGroup = afterFinishGroup;
                dsDialogue.dialogue = startingDialogue;

                // QUAN TRỌNG: Khởi tạo lại dialogueAdapter
                if (dialogueAdapter != null)
                {
                    dialogueAdapter.Initialize(container);

                    // Debug log để kiểm tra
                    Debug.Log($"Changed to dialogue: {startingDialogue.DialogueName} in group: {afterFinishGroup.GroupName}");
                    Debug.Log($"Reinitialized dialogueAdapter with container");
                }
            }

            // Tìm dialogue khởi đầu trong group hiện tại
            DSDialogueSO startDialogue = null;
            if (dsDialogue.dialogueGroup != null)
            {
                foreach (var entry in container.DialogueGroups)
                {
                    if (entry.Key == dsDialogue.dialogueGroup)
                    {
                        foreach (var dialogue in entry.Value)
                        {
                            if (dialogue.IsStartingDialogue)
                            {
                                startDialogue = dialogue;
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            var dialogueSo = startDialogue != null ? startDialogue : dsDialogue.dialogue;


            DialogConservation conv = dialogueAdapter.ConvertDSDialogueToConservation(dialogueSo);
            conservationManager.InitConservation(gameObject, conv);

            Debug.Log($"After InitConservation - Using dialogue: {dialogueSo.DialogueName} from group: {dsDialogue.dialogueGroup?.GroupName}");
        }
        else if (QuestManager.instance.questMap[questConversation.id].state == QuestState.IN_PROGRESS && quizConversation == null &&
                QuestManager.instance.questMap[questConversation.id].info.questType != QuestInfoSO.QuestType.Quiz)
        {
            // Tìm group "AfterFinish" trong container
            DSDialogueGroupSO afterFinishGroup = null;
            DSDialogueSO startingDialogue = null;
            foreach (var groupEntry in container.DialogueGroups)
            {
                if (groupEntry.Key.GroupName == "NPCPhoto")
                {
                    afterFinishGroup = groupEntry.Key;
                    // Tìm dialogue khởi đầu trong group này
                    foreach (var dialogue in groupEntry.Value)
                    {
                        if (dialogue.IsStartingDialogue)
                        {
                            startingDialogue = dialogue;
                            break;
                        }
                    }
                    break;
                }
            }

            if (afterFinishGroup != null && startingDialogue != null)
            {
                // Thay đổi dsDialogue
                dsDialogue.dialogueGroup = afterFinishGroup;
                dsDialogue.dialogue = startingDialogue;

                // QUAN TRỌNG: Khởi tạo lại dialogueAdapter
                if (dialogueAdapter != null)
                {
                    dialogueAdapter.Initialize(container);

                    // Debug log để kiểm tra
                    Debug.Log($"Changed to dialogue: {startingDialogue.DialogueName} in group: {afterFinishGroup.GroupName}");
                    Debug.Log($"Reinitialized dialogueAdapter with container");
                }
            }

            // Tìm dialogue khởi đầu trong group hiện tại
            DSDialogueSO startDialogue = null;
            if (dsDialogue.dialogueGroup != null)
            {
                foreach (var entry in container.DialogueGroups)
                {
                    if (entry.Key == dsDialogue.dialogueGroup)
                    {
                        foreach (var dialogue in entry.Value)
                        {
                            if (dialogue.IsStartingDialogue)
                            {
                                startDialogue = dialogue;
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            var dialogueSo = startDialogue != null ? startDialogue : dsDialogue.dialogue;


            DialogConservation conv = dialogueAdapter.ConvertDSDialogueToConservation(dialogueSo);
            conservationManager.InitConservation(gameObject, conv);

            Debug.Log($"After InitConservation - Using dialogue: {dialogueSo.DialogueName} from group: {dsDialogue.dialogueGroup?.GroupName}");


        }

          else
        {

            if (dsDialogue.dialogueGroup != null)
            {
                foreach (var entry in container.DialogueGroups)
                {
                    if (entry.Key == dsDialogue.dialogueGroup)
                    {
                        foreach (var dialogue in entry.Value)
                        {
                            if (dialogue.IsStartingDialogue)
                            {
                                startDialogue = dialogue;
                                break;
                            }
                        }
                        break;
                    }
                }
            }


            var dialogueSo = startDialogue != null ? startDialogue : dsDialogue.dialogue;


            conv = dialogueAdapter.ConvertDSDialogueToConservation(dialogueSo);
            conservationManager.InitConservation(gameObject, conv);
        }
                  
    }

    private void HandleQuestStateChanged()
    {

    }

    public void OpenShopFunctionalWindow()
    {
        string functionId = FunctionType.Shop.ToString() + "_" + npcInfo.npcId;
        // Lấy functional từ map
        FunctionalWindowDataSO functionWindowData = functionalDataMap[functionId];
        functionWindowData.Init(gameObject);
    }

    #region NPC Data
    private Dictionary<string, NPCConservationSO> CreateNPCConservationMap()
    {
        // Loads all NPC conservation assets từ Resources/NPC/<npcId>
        NPCConservationSO[] allConservations = Resources.LoadAll<NPCConservationSO>("NPC/" + npcInfo.npcId);

        Dictionary<string, NPCConservationSO> map = new Dictionary<string, NPCConservationSO>();
        foreach (NPCConservationSO npcConservation in allConservations)
        {
            if (map.ContainsKey(npcConservation.conservationId))
            {
                Debug.LogWarning("Duplicate ID found when creating npc conservation map: " + npcConservation.conservationId);
            }
            else
            {
                map.Add(npcConservation.conservationId, npcConservation);
            }
        }
        return map;
    }

    private Dictionary<string, MinigameDataSO> CreateMinigameDataMap()
    {
        MinigameDataSO[] allMinigames = Resources.LoadAll<MinigameDataSO>("NPC/" + npcInfo.npcId);
        Dictionary<string, MinigameDataSO> map = new Dictionary<string, MinigameDataSO>();
        foreach (MinigameDataSO minigame in allMinigames)
        {
            if (map.ContainsKey(minigame.minigameId))
            {
                Debug.LogWarning("Duplicate ID found when creating minigame data map: " + minigame.minigameId);
            }
            else
            {
                map.Add(minigame.minigameId, minigame);
            }
        }
        return map;
    }

    private Dictionary<string, FunctionalWindowDataSO> CreateFunctionalDataMap()
    {
        FunctionalWindowDataSO[] allFunctionals = Resources.LoadAll<FunctionalWindowDataSO>("NPC/" + npcInfo.npcId);
        Dictionary<string, FunctionalWindowDataSO> map = new Dictionary<string, FunctionalWindowDataSO>();
        foreach (FunctionalWindowDataSO functional in allFunctionals)
        {
            if (map.ContainsKey(functional.functionId))
            {
                Debug.LogWarning("Duplicate ID found when creating functional data map: " + functional.functionId);
            }
            else
            {
                map.Add(functional.functionId, functional);
            }
        }
        return map;
    }
    #endregion

    #region Quest Minigame Handle
    public void StartQuestMinigame()
    {
        switch (questStep.missionType)
        {
            case StepMissionType.TALKING:
                break;

            case StepMissionType.MINIGAME:
                string minigameId = questId + "_" + questStep.stepId;
                // Tìm minigame trong map
                MinigameDataSO minigameData = minigameDataMap[minigameId];

                minigameData.Init(gameObject);
                break;
        }
    }

    public void OnQuestMinigameSuccess()
    {

        if (QuestManager.instance.questMap[questConversation.id].info.questType == QuestInfoSO.QuestType.Passive)
        {
            FinishPassiveQuestStep();
        }

        else if (QuestManager.instance.questMap[questConversation.id].info.questType != QuestInfoSO.QuestType.Quiz)
        {
            FinishQuestStep();
        }

        else if (QuestManager.instance.questMap[questConversation.id].info.questType == QuestInfoSO.QuestType.Quiz)
        {
            string questId = this.questId;

            // Kiểm tra questId có hợp lệ không
            if (!string.IsNullOrEmpty(questId))
            {
                FinishQuestStep();
                // StopInteract();
                QuestManager.instance.questMap[questId].state = QuestState.FINISHED;

                // Cập nhật một phần tử cụ thể
                int stepIndex = 1;
                QuestManager.instance.questMap[questId].questStepStates[stepIndex] = new QuestStepState("Bước 2", "Hoàn thành Quest Quiz A1");

                QuizManager.instance.currentQuizQuestId = string.Empty;


            }
            else
            {
                Debug.LogError("Cannot finish quest step: No valid quest ID from QuizManager");
            }
        }
    }

    public void OnQuestMinigameFail()
    {
        StopInteract();
      
    }

   
    #endregion

    #region Quest Step Handle
    public void FinishQuestStep()
    {
        // Lấy questId từ QuizManager nếu đang trong quiz, nếu không dùng questId hiện tại
        string currentQuestId = string.IsNullOrEmpty(QuizManager.instance.currentQuizQuestId)
                              ? this.questId
                              : QuizManager.instance.currentQuizQuestId;

        // Kiểm tra questId có hợp lệ không
        if (!string.IsNullOrEmpty(currentQuestId))
        {
            StopInteract();

            ResetNPC();
            questManager.OnFinishQuestStep(currentQuestId);
        }
        else
        {
            Debug.LogError("Cannot finish quest step: No valid quest ID");
        }



    }

    public void FinishPassiveQuestStep()
    {
        // Lấy questId từ QuizManager nếu đang trong quiz, nếu không dùng questId hiện tại
        string currentQuestId = string.IsNullOrEmpty(QuizManager.instance.currentQuizQuestId)
                              ? this.questId
                              : QuizManager.instance.currentQuizQuestId;

        // Kiểm tra questId có hợp lệ không
        if (!string.IsNullOrEmpty(currentQuestId))
        {
            // StopInteract();

            ResetNPC();
            questManager.OnFinishQuestStep(currentQuestId);
        }
        else
        {
            Debug.LogError("Cannot finish quest step: No valid quest ID");
        }
    }

    public void FinishQuestStepThenStartMinigame()
    {
        FinishQuestStep();
        StartQuestMinigame();
    }

    public void ResetNPC()
    {
        Debug.Log("zzzzzzzzzzz");
        ChangeNPCState(NPCState.NORMAL);
        UpdateConservation();
    }

    public void UpdateQuestStep(QuestStep step, string questId)
    {
        this.questId = questId;
        questStep = step;
        ChangeNPCState(step.npcStatus);

        UpdateConservation();
    }

    public void UpdateConservation()
    {
        if (currentNPCState == NPCState.NORMAL)
        {
            currentDialogConservation = npcConservationMap[currentNPCState.ToString()];
        }
        else
        {
            string questStepConservationId = GetQuestStepConservationId();
            currentDialogConservation = npcConservationMap[questStepConservationId];
        }
    }


    public string GetQuestStepConservationId()
    {
        return currentNPCState
                + (questId != "" ? ("_" + questId) : "")
                + (questStep.stepId != "" ? ("_" + questStep.stepId) : "");
    }

    public void ChangeNPCState(NPCState state)
    {
        currentNPCState = state;
        HideAllStateIcon();
        switch (currentNPCState)
        {
            case NPCState.NORMAL:
                if (iconDefault != null)
                    iconDefault.SetActive(true);
                break;
            case NPCState.HAVE_QUEST:
                iconQuestAvailable.SetActive(true);
                break;
            case NPCState.IN_PROGRESS:
                iconQuestInProgress.SetActive(true);
                break;
            case NPCState.QUEST_COMPLETE:
                iconQuestComplete.SetActive(true);
                break;
            case NPCState.COUNTDOWN:
                break;
        }
    }

    private void HideAllStateIcon()
    {
        if (iconQuestAvailable) iconQuestAvailable.SetActive(false);
        if (iconQuestComplete) iconQuestComplete.SetActive(false);
        if (iconQuestInProgress) iconQuestInProgress.SetActive(false);
    }

    public void ListenMusic()
    {


        StartCoroutine(DelayedPlayMusicCoroutine());
    }

    private IEnumerator DelayedPlayMusicCoroutine()
    {
        // Đợi 1 giây trước khi phát nhạc
        yield return new WaitForSeconds(1.2f);
        // Tiếp tục với quá trình phát nhạc
        yield return PlayMusicCoroutine();
    }

    private IEnumerator PlayMusicCoroutine()
    {

        // Tải AudioClip từ Resources/Audio/<QuestId>
        string audioPath = "Audio/" + questConversation.id;
        AudioClip clip = Resources.Load<AudioClip>(audioPath);
        if (clip == null)
        {
            Debug.LogError($"[ListenMusic] AudioClip không tìm thấy tại Resources/{audioPath}");
            yield break;
        }

        // Lấy hoặc thêm AudioSource
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = clip;

        ConservationManager.instance.StopAllCoroutines();

        ConservationManager.instance.StartCoroutine(ConservationManager.instance.DeactivateConservationChoice());

        // Phát nhạc
        audioSource.Play();

        // Đợi cho đến khi nhạc kết thúc
        yield return new WaitForSeconds(clip.length);

        ConservationManager.instance.StartCoroutine(ConservationManager.instance.ActivateConservationChoice());

    }


    #region Money and Blood

    public void DonateFloodRelief()
    {

        int donatedAmount = PlayerInventory.instance.gold / 2;

        PlayerInventory.instance.SubtractGold(donatedAmount);

        // Hoàn thành quest (Passive)
        FinishQuestStep();
    }

    public void DonateBlood()
    {
        // 1. Lấy HealthBar qua static instance
        var healthBar = PlayerStatsController.HealthBar.instance;
        if (healthBar == null)
        {
            Debug.LogError("HealthBar.instance chưa được thiết lập!");
            return;
        }

        float current = healthBar.slider.value;      // sinh lực hiện tại
        float max = healthBar.slider.maxValue;   // sinh lực tối đa

        // 2. Kiểm tra >90%
        if (current / max > 0.9f)
        {
            // a) Trừ 50%
            healthBar.SetCurrentHealth(current * 0.5f);

            // b) Hoàn thành quest
            FinishQuestStep();
        }
    }

    public void CheckBloodCondition()
    {
        var healthBar = HealthBar.instance;
        if (healthBar == null)
        {
            Debug.LogError("HealthBar.instance chưa được gán!");
            return;
        }

        float current = healthBar.slider.value;
        float max = healthBar.slider.maxValue;

        if (current / max > 0.9f)
            SwitchDialogueGroupInstance("DuDieuKien");
        else
            SwitchDialogueGroupInstance("KhongDuDieuKien");
    }


    private void SwitchDialogueGroupInstance(string groupName)
    {
        DSDialogueGroupSO targetGroup = null;
        DSDialogueSO startDia = null;

        foreach (var entry in container.DialogueGroups)
        {
            if (entry.Key.GroupName == groupName)
            {
                targetGroup = entry.Key;
                startDia = entry.Value.First(d => d.IsStartingDialogue);
                break;
            }
        }

        if (targetGroup != null && startDia != null)
        {
            dsDialogue.dialogueGroup = targetGroup;
            dsDialogue.dialogue = startDia;
            dialogueAdapter.Initialize(container);
            conv = dialogueAdapter.ConvertDSDialogueToConservation(startDia);
        
            StartCoroutine(ConservationManager.instance.UpdateConservation(conv));

            StartCoroutine(DelayedInitConservation(gameObject, conv));


        }
        else
        {
            Debug.LogWarning($"Group '{groupName}' hoặc Starting Dialogue không tìm thấy");
        }
    }

    private void SwitchDialogueGroup(string groupName)
    {
        DSDialogueGroupSO targetGroup = null;
        DSDialogueSO startDia = null;

        foreach (var entry in container.DialogueGroups)
        {
            if (entry.Key.GroupName == groupName)
            {
                targetGroup = entry.Key;
                startDia = entry.Value.First(d => d.IsStartingDialogue);
                break;
            }
        }

        if (targetGroup != null && startDia != null)
        {
            dsDialogue.dialogueGroup = targetGroup;
            dsDialogue.dialogue = startDia;
            dialogueAdapter.Initialize(container);
            conv = dialogueAdapter.ConvertDSDialogueToConservation(startDia);

          


        }
        else
        {
            Debug.LogWarning($"Group '{groupName}' hoặc Starting Dialogue không tìm thấy");
        }
    }

    private IEnumerator DelayedInitConservation(GameObject go, DialogConservation conv)
    {
        yield return new WaitForSeconds(1.2f);
        ConservationManager.instance.InitConservation(go, conv);
    }

    public void ResetConversation()
    {

        // 1. Reset DSDialogue về state gốc
        dsDialogue.dialogueGroup = defaultGroup;
        dsDialogue.dialogue = defaultDialogue;

        StopInteract();
    }

    #endregion

    #region Photo Phao Thi Quest


public void AcquirePhotoPhaoThi()
    {
        const int cost = 150;


        // 1) Kiểm tra đủ vàng
        if (PlayerInventory.instance.gold < cost)
        {


            Debug.LogWarning("Không đủ vàng để mua phao thi");

            SwitchDialogueGroupInstance("KhongDuTien");

            return;
        }
    }


    public void DeliverPhotoPhaoThi()
    {
        const string itemId = "PhaoThi";

        // Thử xóa 1 phao thi
        int removed = PlayerInventory.instance.RemoveItemById(itemId, 1);
        if (removed <= 0)
        {
            Debug.LogWarning("Không có phao thi để giao");
            return;
        }

        // Hoàn thành quest
        FinishQuestStep();
    }

    #endregion

    #endregion

    #region Mission Complete & Mission Fail Popup

    public void Hybrid_QuestFail()
    {
        StopInteract();
        StartCoroutine(MissionFail());
    }

    private IEnumerator MissionComplete()
    {
        yield return new WaitForSeconds(1f);

        ConservationManager.instance.missionCompleteText.text = "+ " + questConversation.goldReward;

        ConservationManager.instance.missionCompleteContainer.Activate();

        yield return new WaitForSeconds(3f);

        ConservationManager.instance.missionCompleteContainer.Deactivate();
    }

    private IEnumerator MissionFail()
    {
        yield return new WaitForSeconds(1f);

        ConservationManager.instance.missionFailContainer.Activate();

        yield return new WaitForSeconds(3f);

        ConservationManager.instance.missionFailContainer.Deactivate();
    }

    #endregion
}


