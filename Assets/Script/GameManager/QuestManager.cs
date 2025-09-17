using Interaction;
using System;
using Interaction.Minigame;
using PlayerStatsController;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using static GlobalResponseData_Login;
using static Interaction.QuestInfoSO;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace GameManager
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager instance;
        Dictionary<string, NPCController> npcMap;
        public int FirstTimeQuest = 0;
        [HideInInspector] public Dictionary<string, Quest> questMap;
        PlayerStats playerStats;
        public TextMeshProUGUI CharacterName;
        public event Action OnQuestsInitialized;
        public event Action<Quest> OnQuestFinished;

        [Header("Config")]
        [SerializeField] private bool loadQuestState = true;

        Quest currentQuest = null;
        private void Awake()
        {
            // Nếu đây là lần đầu cập nhật quest từ server (FirstTimeQuest == 1) 
            // và GlobalResponseData đã có dữ liệu quests (tức là được load từ server)
            //Debug.unityLogger.logEnabled = false;


            if (GlobalResponseData.FirstTimeQuest == 1 &&
                GlobalResponseData.quests != null && GlobalResponseData.quests.Count > 0)
            {
                questMap = GlobalResponseData.quests;
                Debug.Log("Sử dụng dữ liệu quest từ server (GlobalResponseData.quests)");
            }
            else
            {
                // Nếu không (ví dụ FirstTimeQuest == 0) thì khởi tạo quest mới
                questMap = CreateQuestMap();
                Debug.Log("Tạo dữ liệu quest mới với CreateQuestMap()");
            }
            instance = this;
          
           
        }
        #region Old Start()
        /*
        void Start()
        {
            
            npcMap = CreateNPCMap();
            playerStats = FindObjectOfType<PlayerStats>();
            UpdateRequirementsMetQuest();
            CharacterName.text = GlobalResponseData.fullname;

            foreach (var quest in questMap.Values)
            {
                if (quest.state == QuestState.CAN_START || quest.state == QuestState.IN_PROGRESS)
                {
                    // Gọi InitQuestStep để NPCController.questStep không còn null
                    InitQuestStep(quest.info.questSteps[quest.currentQuestStepIndex], quest.info.id);
                }
            }

        }
        */
        #endregion


        void Start()
        {
            npcMap = CreateNPCMap();
            playerStats = FindObjectOfType<PlayerStats>();
            CharacterName.text = GlobalResponseData.CharacterName;

            // 1. Reset hoàn toàn mọi quest đang IN_PROGRESS
            var inProgressIds = questMap
                .Where(kvp => kvp.Value.state == QuestState.IN_PROGRESS)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string questId in inProgressIds)
            {
                var info = questMap[questId].info;

                // Tạo lại một Quest mới (QuizQuest hoặc Quest thường)
                Quest freshQuest;
                bool isQuiz = (info.questType == QuestType.Quiz);
                if (isQuiz)
                    freshQuest = new QuizQuest(info);
                else
                    freshQuest = new Quest(info);

                // Thay thế instance cũ bằng instance mới
                questMap[questId] = freshQuest;
            }

            // 2. Kiểm tra lại điều kiện để chuyển sang CAN_START (nếu cần)
            UpdateRequirementsMetQuest();

            // 3. Khởi tạo bước đầu cho tất cả quest đang ở CAN_START
            foreach (var quest in questMap.Values)
            {
                if (quest.state == QuestState.CAN_START)
                {
                    InitQuestStep(
                        quest.info.questSteps[quest.currentQuestStepIndex],
                        quest.info.id
                    );
                }
            }
      //      OnQuestsInitialized?.Invoke();
        }


        #region Handle Quest Step Update

        public void UpdateRequirementsMetQuest()
        {
            // loop through ALL quests
            foreach (Quest quest in questMap.Values)
            {
                // if we're now meeting the requirements, switch over to the CAN_START state
                if (quest.state == QuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
                {
                    quest.ChangeQuestState(QuestState.CAN_START);
                    currentQuest = quest;
                    InitQuestStep(quest.info.questSteps[quest.currentQuestStepIndex], quest.info.id);
                    
                }
               
            }
        }

        public void InitQuestStep(QuestStep questStep, string questId)
        {
            NPCController npc = GetNPCById(questStep.npcId);
            npc.UpdateQuestStep(questStep, questId);
        }

        private void Update()
        {
            //UpdateRequirementsMetQuest();
            //if (Input.GetKeyUp(KeyCode.K))
            //{
            //    OnFinishQuestStep(currentQuest.info.id);
            //    Debug.Log(currentQuest.info.id);
            //}
        }

        public void OnFinishQuestStep(string questId)
        {
            bool isQuestFinished = !questMap[questId].TryNextStep();
            Quest quest = questMap[questId];
            if (isQuestFinished)
            {
                Debug.Log("Quest complete..reward");
                playerStats.ExpGain(quest.info.experienceReward);
                PlayerInventory.instance.AddGold(quest.info.goldReward);
                

                for (int i = 0; i < quest.info.itemRewards.Count; i++)
                {
                    PlayerInventory.instance.AddItem(quest.info.itemRewards[i].item, quest.info.itemRewards[i].quantity);
                }

                OnQuestFinished?.Invoke(quest);

            } else
            {
                InitQuestStep(quest.info.questSteps[quest.currentQuestStepIndex], quest.info.id);
            }

            if (QuestLogManager.instance != null)
                QuestLogManager.instance.RefreshCurrentQuestUI();
        }

        #endregion

        #region Handle Reward
        public int CheckFinishedQuestsForMedals()
        {
            int medal = 0; // Reset Medal trước khi kiểm tra

            foreach (Quest quest in questMap.Values)
            {
                if (quest.state == QuestState.FINISHED)
                {
                    medal++; // Tăng số huân chương nếu quest đã hoàn thành
                }
            }

            return medal;
        }

        #endregion

        #region Handle Quest Manager Data

        private bool CheckRequirementsMet(Quest quest)
        {
            // start true and prove to be false
            bool meetsRequirements = true;

            // check player level requirements
            if (playerStats.characterLevel < quest.info.levelRequirement)
            {
                meetsRequirements = false;
            }

            // check quest prerequisites for completion
            foreach (QuestInfoSO prerequisiteQuestInfo in quest.info.questPrerequisites)
            {
                if (GetQuestById(prerequisiteQuestInfo.id).state != QuestState.FINISHED)
                {
                    meetsRequirements = false;
                }
            }

            return meetsRequirements;
        }

        private Quest GetQuestById(string id)
        {
            Quest quest = questMap[id];
            if (quest == null)
            {
                Debug.LogError("ID not found in the Quest Map: " + id);
            }
            return quest;
        }

        private Dictionary<string, Quest> CreateQuestMap()
        {
            // loads all QuestInfo Scriptable Objects under the Assets/Resources/Quests folder
            QuestInfoSO[] allQuests = Resources.LoadAll<QuestInfoSO>("Quests");
            
            // Create the quest map
            Dictionary<string, Quest> questMap = new Dictionary<string, Quest>();
            foreach (QuestInfoSO questInfo in allQuests)
            {
                if (questMap.ContainsKey(questInfo.id))
                {
                    Debug.LogWarning("Duplicate ID found when creating quest map: " + questInfo.id);
                }
                questMap.Add(questInfo.id, LoadQuest(questInfo));
            }
            return questMap;
        }

        private Quest LoadQuest(QuestInfoSO questInfo)
        {
            Quest quest = null;
            try
            {
                // Xác định loại quest (ở đây giả sử bạn sử dụng trường questType)
                bool isQuizQuest = (questInfo.questType == QuestType.Quiz);

                // Kiểm tra dữ liệu đã lưu nếu có
                if (PlayerPrefs.HasKey(questInfo.id) && loadQuestState)
                {
                    string serializedData = PlayerPrefs.GetString(questInfo.id);
                    if (isQuizQuest)
                    {
                        // Khởi tạo đối tượng QuizQuest từ dữ liệu đã lưu
                        quest = QuizQuest.Deserialize(questInfo, serializedData);
                    }
                    else
                    {
                        QuestData questData = JsonUtility.FromJson<QuestData>(serializedData);
                        quest = new Quest(questInfo, questData.state, questData.questStepIndex, questData.questStepStates);
                    }
                }
                else
                {
                    // Nếu không có dữ liệu lưu, khởi tạo quest mới theo đúng loại
                    if (isQuizQuest)
                    {
                        quest = new QuizQuest(questInfo);
                    }
                    else
                    {
                        quest = new Quest(questInfo);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load quest with id " + questInfo.id + ": " + e);
            }
            return quest;
        }


        private Dictionary<string, NPCController> CreateNPCMap()
        {
            NPCController[] npcs = FindObjectsOfType<NPCController>();

            // Create the quest map
            Dictionary<string, NPCController> npcMap = new Dictionary<string, NPCController>();
            foreach (NPCController npc in npcs)
            {
                if (npcMap.ContainsKey(npc.npcInfo.npcId))
                {
                    Debug.LogWarning("Duplicate ID found when creating NPC map: " + npc.npcInfo.npcId);
                }
                npcMap.Add(npc.npcInfo.npcId, npc);
            }
            return npcMap;
        }

        private NPCController GetNPCById(string id)
        {
            NPCController npc = npcMap[id];
            if (npc == null)
            {
                Debug.LogError("ID not found in the NPC Map: " + id);
            }
            return npc;
        }

        #endregion

        public void UpdateQuestStep(QuestState questState, string questId)
        {
            foreach (Quest quest in questMap.Values)
            {
                if (quest.info.id == questId)
                {
                    quest.currentQuestStepIndex = 0; // Reset current step index
                    // quest.state = questState;
                    quest.ChangeQuestState(questState);
                                    
                }
            }
        }
    }
}

