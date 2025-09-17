using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using static GlobalResponseData_Login;

using Newtonsoft.Json; // Bạn đang dùng để stringify, có thể dùng để parse nâng cao
using Interaction;
using GameManager;
using static PlayerDataManager;

public class PlayerDataLoader : MonoBehaviour
{
    public TMP_InputField outputArea;

    public static PlayerDataLoader Instance;
    
    // Đường dẫn API
    private string apiUrl = "http://games.ptit.edu.vn:8080/DemoBackend3D_API/player/getLastStatePlayer";


    private void Awake()
    {
        Instance = this;    
    }

    void Start()
    {
        if (outputArea == null)
        {
            outputArea = GameObject.Find("OutputArea").GetComponent<TMP_InputField>();
        }
    }

    /// <summary>
    /// Hàm này được gọi (VD: bấm nút Load hoặc nhấn phím Z)
    /// để tải dữ liệu cuối cùng từ server theo session_id.
    /// </summary>
    public void LoadPlayerData()
    {
        // Kiểm tra session_id trước khi gọi

        StartCoroutine(LoadPlayerData_Coroutine());
    }

    public IEnumerator LoadPlayerData_Coroutine()
    {
        if (string.IsNullOrEmpty(GlobalResponseData.session_id))
        {
            Debug.LogWarning("session_id is null or empty! Please login first.");
            if (outputArea) outputArea.text = "session_id is empty! Please login first.";
            yield break;
        }

        if (outputArea) outputArea.text = "Loading player data...";

        // 1) Chuẩn bị JSON
        SessionData sessionData = new SessionData
        {
            session_id = GlobalResponseData.session_id
        };
        string jsonData = JsonUtility.ToJson(sessionData);

        // 2) Gửi request POST
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        // 3) Kiểm tra lỗi mạng
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("[GetLastState] Network/HTTP error: " + request.error);
            if (outputArea) outputArea.text = "Error: " + request.error;
            yield break;
        }

        // 4) Parse JSON response
        string jsonResponse = request.downloadHandler.text;
        ResponseData responseData = JsonUtility.FromJson<ResponseData>(jsonResponse);

        if (responseData.code == 200)
        {
            // Thành công
            if (outputArea) outputArea.text = "Player data loaded successfully!";

            // In thử JSON để debug
            string prettyJson = JsonConvert.SerializeObject(responseData.player_state, Formatting.Indented);
            Debug.Log("[GetLastState] Full player_state:\n" + prettyJson);

            PlayerState playerState = responseData.player_state;

            // Nếu là lần đầu => set default
            if (playerState.last_state.FirstTimeQuest != 1)
            {
                GlobalResponseData.gold = 200;
                GlobalResponseData.x = 20.799999237060547f;
                GlobalResponseData.y = 0.7609999775886536f;
                GlobalResponseData.z = 69.4000015258789f;
                GlobalResponseData.level = 1;
                GlobalResponseData.HealthSlider = 100;
                GlobalResponseData.FirstTimeQuest = 0;
            }
            else
            {
                // Cập nhật dữ liệu game
                GlobalResponseData.gold = playerState.last_state.gold;
                GlobalResponseData.x = playerState.last_state.x;
                GlobalResponseData.y = playerState.last_state.y;
                GlobalResponseData.z = playerState.last_state.z;
                GlobalResponseData.level = playerState.last_state.level;
                GlobalResponseData.HealthSlider = playerState.last_state.HealthSlider;
                GlobalResponseData.FirstTimeQuest = playerState.last_state.FirstTimeQuest;
                GlobalResponseData.Medal = playerState.last_state.Medal;
                GlobalResponseData.bonus_medal = playerState.last_state.bonus_medal;
                GlobalResponseData.CharacterName = playerState.last_state.CharacterName;
                GlobalResponseData.gender = playerState.last_state.gender;
            }

            // 5) Lấy dialoguesJson (nếu có) để tái tạo tiến trình hội thoại
            if (!string.IsNullOrEmpty(playerState.last_state.dialoguesJson))
            {
                Debug.Log("[GetLastState] Found dialoguesJson, length: " + playerState.last_state.dialoguesJson.Length);

                // Parse sang cấu trúc DialogueFolderDataWrapper
                DialogueFolderDataWrapper dialoguesData = JsonUtility.FromJson<DialogueFolderDataWrapper>(playerState.last_state.dialoguesJson);

                // Giờ bạn có cấu trúc dialoguesData.folders, 
                // tùy bạn xử lý (VD: gán ngược lại cho Dialogue System).
                // Ở đây ta chỉ in log để demo
                foreach (var folder in dialoguesData.folders)
                {
                    Debug.Log($"FolderName: {folder.folderName}, ScriptableObjCount = {folder.scriptableObjects.Count}");
                    // … bạn có thể parse chi tiết group, dialogue, v.v.
                }
            }
            else
            {
                Debug.Log("[GetLastState] dialoguesJson is empty or null => no saved dialogues to restore.");
            }

            if (playerState.last_state.questdata != null &&
    playerState.last_state.questdata.quests != null &&
    playerState.last_state.questdata.quests.Count > 0)
            {
                GlobalResponseData.quests = new Dictionary<string, Quest>();

                // Load toàn bộ QuestInfoSO có trong Resources/Quests
                QuestInfoSO[] allQuests = Resources.LoadAll<QuestInfoSO>("Quests");
                Dictionary<string, QuestInfoSO> questInfoLookup = new Dictionary<string, QuestInfoSO>();
                foreach (QuestInfoSO info in allQuests)
                {
                    if (!questInfoLookup.ContainsKey(info.id))
                        questInfoLookup.Add(info.id, info);
                }

                // Duyệt qua từng entry được gửi qua questdata
                foreach (var questEntry in playerState.last_state.questdata.quests)
                {
                    if (questInfoLookup.ContainsKey(questEntry.questId))
                    {
                        Quest quest = new Quest(
                            questInfoLookup[questEntry.questId],
                            questEntry.questData.state,
                            questEntry.questData.questStepIndex,
                            questEntry.questData.questStepStates
                        );
                        GlobalResponseData.quests.Add(questEntry.questId, quest);
                    }
                    else
                    {
                        Debug.LogWarning("[LoadPlayerData] Không tìm thấy QuestInfoSO cho quest id: " + questEntry.questId);
                    }
                }

                GlobalResponseData.FirstTimeQuest = playerState.last_state.FirstTimeQuest;
            }


            var last = responseData.player_state.last_state;

            // 1) Guard nếu không có inventory
            if (last.inventory == null || last.inventory.items == null)
            {
                Debug.LogWarning("[LoadPlayerData] Không có dữ liệu inventory để lưu.");
            }
            else
            {
                // 2) Duyệt JSON và tạo thẳng List<InventoryItem>
                var storedInventory = new List<InventoryItem>();
                foreach (var itemData in last.inventory.items)
                {
                    // Load đúng ItemSO
                    var so = Resources.Load<ItemSO>($"Items/UsableItem/{itemData.itemName}");
                    if (so != null)
                    {
                        storedInventory.Add(new InventoryItem
                        {
                            item = so,
                            quantity = itemData.quantity
                        });
                    }
                    else
                    {
                        Debug.LogWarning($"Không tìm thấy ItemSO '{itemData.itemName}'");
                    }
                }

                // 3) Gán vào GlobalResponseData mà không động đến PlayerInventory.instance
                GlobalResponseData.inventoryItems = storedInventory;
                }
            }
        else
        {
            // code != 200
            Debug.LogWarning("[GetLastState] Fail: code=" + responseData.code + ", desc=" + responseData.desc);
            if (outputArea) outputArea.text = "Fail to load data! code=" + responseData.code + ", desc=" + responseData.desc;
        }
    }

    // ----------------------------------------------------------
    // Dưới đây là các lớp dữ liệu JSON
    // ----------------------------------------------------------

    [System.Serializable]
    public class SessionData
    {
        public string session_id;
    }

    [System.Serializable]
    public class ResponseData
    {
        public int code;
        public string desc;
        public PlayerState player_state;
    }

    [System.Serializable]
    public class PlayerState
    {
        public int ID;
        public PlayerLastState last_state;
        public int score;
        public string last_login;
    }

    // 
    // CHÚ Ý: Thêm "public string dialoguesJson;" để nhận lại
    //        chuỗi JSON chứa hội thoại do PostMethob_LastState gửi lên.
    //
    [System.Serializable]
    public class PlayerLastState
    {
        public float x;
        public float y;
        public float z;
        public int level;
        public int FirstTimeQuest;
        public float HealthSlider;
        public int Medal;
        public int bonus_medal;
        public int gold;
        public string CharacterName;
        public int gender;
        // Thêm biến này để nhận JSON
        public string dialoguesJson;

        // Bản thân questMap, inventory có sẵn
        public QuestSaveData questdata;
        public QuestMap questMap;
        public InventoryData inventory;
    }

    [System.Serializable]
    public class QuestMap
    {
        public List<QuestData> quests = new List<QuestData>();
    }

    [System.Serializable]
    public class QuestData
    {
        public string id;
        public QuestState state;
        public int questStepIndex;
        public QuestStepState[] stepStates;
    }

    [System.Serializable]
    public class InventoryData
    {
        public List<InventoryItemData> items = new List<InventoryItemData>();
    }

    [System.Serializable]
    public class InventoryItemData
    {
        public string itemName;
        public int quantity;
    }



    // ----------------------------------------------------------
    // (A) Các lớp để parse lại dialoguesJson
    //     Đồng nhất với PostMethob_LastState
    // ----------------------------------------------------------
    [System.Serializable]
    public class DialogueFolderDataWrapper
    {
        public List<DialogueFolderData> folders;
    }

    [System.Serializable]
    public class DialogueFolderData
    {
        public string folderName;
        public List<DialogueScriptableData> scriptableObjects;
    }

    [System.Serializable]
    public class DialogueScriptableData
    {
        public string objectName;
        public string objectType;

        public DSDialogueContainerData containerData;
        public DSDialogueData dialogueData;
        public DSDialogueGroupData groupData;
    }

    [System.Serializable]
    public class DSDialogueContainerData
    {
        public string fileName;
        public List<DSDialogueData> ungroupedDialogues;
        public List<GroupedDialogueData> groupedDialogues;
    }

    [System.Serializable]
    public class GroupedDialogueData
    {
        public string groupName;
        public List<DSDialogueData> dialogues;
    }

    [System.Serializable]
    public class DSDialogueData
    {
        public string dialogueName;
        public string text;
        public string dialogueType;
        public bool isStartingDialogue;
        public List<ChoiceData> choices;
    }

    [System.Serializable]
    public class ChoiceData
    {
        public string text;
        public string nextDialogueName;
    }

    [System.Serializable]
    public class DSDialogueGroupData
    {
        public string groupName;
    }
}
