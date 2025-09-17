using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.IO; // Để duyệt thư mục runtime
using static GlobalResponseData_Login;
using GameManager;
using static PlayerDataLoader;
using UnityEngine.Analytics;
using System.Reflection;

// Nếu bạn dùng quest, import namespace liên quan ở đây
// using MyNamespace.Quest;

public class PlayerDataManager : MonoBehaviour
{
    public TMP_InputField outputArea;
    public GameObject player;

    public int FirstTimeQuest;
    public Slider HealthSlider;


    // API endpoint
    private string apiUrl = "http://games.ptit.edu.vn:8080/DemoBackend3D_API/player/updateLastStatePlayer";

    void Start()
    {
        if (outputArea == null)
        {
            outputArea = GameObject.Find("OutputArea").GetComponent<TMP_InputField>();
        }

        //save data when quest finish
        QuestManager.instance.OnQuestFinished += (quest) =>
        {
            Invoke(nameof(fomo), 1f);
        };

        CartManager.instance.OnBuyItemSuccess += () =>
        {
            Invoke(nameof(fomo), 1f);
        };

    }

    // Ví dụ nút bấm
    public void fomo()
    {
        Debug.Log("fomo");
        if (GlobalResponseData.session_id == null) return;

        Vector3 pos = player.transform.position;
        PostPlayerData(pos.x, pos.y, pos.z, 1, HealthSlider.value, GlobalResponseData.student_id, GlobalResponseData.fullname, PlayerInventory.instance.gold, PlayerInventory.instance.Medal, PlayerInventory.instance.bonus_medal, GlobalResponseData.CharacterName, GlobalResponseData.gender);
    }

    public void PostPlayerData(
        float x, float y, float z,
        int firstTimeQuest,
        float healthSlider,
        string student_id,
        string fullname,
        int gold,
        int Medal,
        int bonus_medal,
        string CharacterName,
        int gender
        )
    {
        StartCoroutine(PostPlayerData_Coroutine(
            x, y, z, firstTimeQuest, healthSlider, student_id, fullname, gold, Medal, bonus_medal, CharacterName, gender
            ));

        
    }

    IEnumerator PostPlayerData_Coroutine(
        float x, float y, float z,
        int firstTimeQuest,
        float healthSlider,
        string student_id,
        string fullname,
        int gold,
        int Medal,
        int bonus_medal,
        string CharacterName,
        int gender
        )
    {
        if (outputArea) outputArea.text = "Updating player data...";

        // 1) Thu thập JSON chứa thông tin DialogSystem
        //    Nếu bạn cần chia theo subfolder => gọi hàm ở dưới
      //  string dialoguesJson = CollectDialogues_BySubfolder("DialogSystem/Dialogues");
        QuestSaveData saveData = SerializeQuestData();
        InventoryData invData = SerializeInventoryData();
        GlobalResponseData.inventoryItems =  new List<InventoryItem>();
        string questdata = JsonUtility.ToJson(saveData);
        // 2) Tạo object PlayerData, gán chuỗi JSON
        PlayerData playerData = new PlayerData
        {
            session_id = GlobalResponseData.session_id,
            details = new PlayerDetails(
                x, y, z,
                firstTimeQuest,
                healthSlider,
                student_id,
                fullname,
                gold,
                Medal,
                bonus_medal,
                CharacterName,
                gender
            )
            {
              //  dialoguesJson = dialoguesJson,
                questdata = saveData,

                inventory = invData
            }
            
                
            
        };

        string jsonData = JsonUtility.ToJson(playerData);
        Debug.Log("Final JSON Payload =\n" + jsonData);

        // 3) Gửi request
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            if (outputArea) outputArea.text = request.error;
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(jsonResponse);

            if (responseData.code == 200)
            {
                if (outputArea) outputArea.text = "Update Success!";
            }
            else if (responseData.code == 700)
            {
                if (outputArea) outputArea.text = $"Error {responseData.code}: {responseData.desc}";
            }
            else
            {
                if (outputArea) outputArea.text = $"Unexpected response code: {responseData.code}";
            }
        }
    }


    private QuestSaveData SerializeQuestData()
    {
        QuestSaveData saveData = new QuestSaveData();
        saveData.session_id = GlobalResponseData.session_id;

        // Add all quests to the save data
        foreach (var questPair in QuestManager.instance.questMap)
        {
            string questId = questPair.Key;
            Quest quest = questPair.Value;

            // Create quest data object
            QuestData questData = new QuestData(
                quest.state,
                quest.currentQuestStepIndex,
                quest.questStepStates
            );

            // Add to list
            saveData.quests.Add(new QuestEntry
            {
                questId = questId,
                questData = questData
            });
        }

        return saveData;
    }


    private InventoryData SerializeInventoryData()
    {
        var invData = new InventoryData();
        var current = PlayerInventory.instance.GetCurrentInventoryState(); 
        foreach (var kv in current)
        {
            invData.items.Add(new InventoryItemData
            {
                itemName = kv.Value.item.itemId,
                quantity = kv.Value.quantity
            });
        }
        return invData;
    }


    // --------------------------------------------------------------------
    // (A) HÀM CHÍNH: LẤY DỮ LIỆU TỪ RESOURCES -> CHIA THEO SUBFOLDER -> JSON
    // --------------------------------------------------------------------
    private string CollectDialogues_BySubfolder(string resourceRelativePath)
    {
        // Tạo danh sách chứa thông tin “mỗi folder”
        List<DialogueFolderData> allFoldersData = new List<DialogueFolderData>();

        // Bước 1: Tìm đường dẫn thực trong ổ đĩa
        // “Application.dataPath” => trỏ tới “Assets” của project
        // Kết hợp với "/Resources/" và resourceRelativePath
        // => "Assets/Resources/DialogSystem/Dialogues"
        string fullPath = Path.Combine(Application.dataPath, "Resources", resourceRelativePath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning("Không tìm thấy thư mục: " + fullPath);
            return "";
        }

        // Bước 2: Duyệt tất cả subfolder (VD: container1, container2)
        string[] subFolders = Directory.GetDirectories(fullPath);

        // Nếu không có subfolder, ta vẫn có thể load asset trực tiếp từ resourceRelativePath
        // tuỳ logic bạn muốn.
        if (subFolders.Length == 0)
        {
            // Nếu không có folder con, ta vẫn loadEverything
            DialogueFolderData singleFolder = new DialogueFolderData
            {
                folderName = "(NoSubfolder)",
                scriptableObjects = LoadAndParseAllScriptableObjects(resourceRelativePath)
            };
            allFoldersData.Add(singleFolder);
        }
        else
        {
            // Với mỗi folder con, ta sẽ load theo path "DialogSystem/Dialogues/tênFolderCon"
            foreach (string folderPath in subFolders)
            {
                // Lấy tên folder (ví dụ "container1")
                string folderName = Path.GetFileName(folderPath);

                // Tạo resource path con. Ví dụ: "DialogSystem/Dialogues/container1"
                string subResourcePath = resourceRelativePath + "/" + folderName;

                // Load & parse
                List<DialogueScriptableData> folderAssets = LoadAndParseAllScriptableObjects(subResourcePath);

                DialogueFolderData folderData = new DialogueFolderData
                {
                    folderName = folderName,
                    scriptableObjects = folderAssets
                };
                allFoldersData.Add(folderData);
            }
        }

        // Đóng gói tất cả vào wrapper -> JSON
        DialogueFolderDataWrapper wrapper = new DialogueFolderDataWrapper
        {
            folders = allFoldersData
        };
        string finalJson = JsonUtility.ToJson(wrapper, prettyPrint: true);
        return finalJson;
    }

    // --------------------------------------------------------------------
    // (B) HÀM TẢI & CHUYỂN MỘT NHÓM ScriptableObject -> List<DialogueScriptableData>
    // --------------------------------------------------------------------
    private List<DialogueScriptableData> LoadAndParseAllScriptableObjects(string resourcePath)
    {
        // Dùng Resources.LoadAll<ScriptableObject> để tải TẤT CẢ ScriptableObject trong folder
        Object[] loaded = Resources.LoadAll(resourcePath, typeof(ScriptableObject));

        List<DialogueScriptableData> results = new List<DialogueScriptableData>();

        foreach (Object obj in loaded)
        {
            ScriptableObject so = obj as ScriptableObject;
            if (so != null)
            {
                // Chuyển sang cấu trúc serializable
                DialogueScriptableData data = ConvertScriptableObjectToData(so);
                results.Add(data);
            }
        }
        return results;
    }

    // --------------------------------------------------------------------
    // (C) CHUYỂN MỘT ScriptableObject (DSDialogueContainerSO, DSDialogueSO, DSDialogueGroupSO, …) -> Data
    // --------------------------------------------------------------------
    private DialogueScriptableData ConvertScriptableObjectToData(ScriptableObject scriptableObj)
    {
        DialogueScriptableData data = new DialogueScriptableData
        {
            objectName = scriptableObj.name,
            objectType = scriptableObj.GetType().Name
        };

        // DSDialogueContainerSO
        if (scriptableObj is DS.ScriptableObjects.DSDialogueContainerSO container)
        {
            data.containerData = new DSDialogueContainerData
            {
                fileName = container.FileName,
                ungroupedDialogues = ExtractDialogues(container.UngroupedDialogues),
                groupedDialogues = ExtractGroupedDialogues(container.DialogueGroups)
            };
        }
        // DSDialogueSO
        else if (scriptableObj is DS.ScriptableObjects.DSDialogueSO dialogueSo)
        {
            data.dialogueData = new DSDialogueData
            {
                dialogueName = dialogueSo.DialogueName,
                text = dialogueSo.Text,
                isStartingDialogue = dialogueSo.IsStartingDialogue,
                dialogueType = dialogueSo.DialogueType.ToString(),
                choices = ExtractChoices(dialogueSo.Choices)
            };
        }
        // DSDialogueGroupSO
        else if (scriptableObj is DS.ScriptableObjects.DSDialogueGroupSO groupSo)
        {
            data.groupData = new DSDialogueGroupData
            {
                groupName = groupSo.GroupName
            };
        }

        // Tuỳ ý xử lý thêm nếu còn loại scriptable object khác

        return data;
    }

    private List<DSDialogueData> ExtractDialogues(List<DS.ScriptableObjects.DSDialogueSO> dialogues)
    {
        List<DSDialogueData> list = new List<DSDialogueData>();
        if (dialogues == null) return list;

        foreach (var d in dialogues)
        {
            DSDialogueData dData = new DSDialogueData
            {
                dialogueName = d.DialogueName,
                text = d.Text,
                isStartingDialogue = d.IsStartingDialogue,
                dialogueType = d.DialogueType.ToString(),
                choices = ExtractChoices(d.Choices)
            };
            list.Add(dData);
        }
        return list;
    }

    private List<GroupedDialogueData> ExtractGroupedDialogues(
        SerializableDictionary<DS.ScriptableObjects.DSDialogueGroupSO, List<DS.ScriptableObjects.DSDialogueSO>> dict
    )
    {
        List<GroupedDialogueData> result = new List<GroupedDialogueData>();
        if (dict == null) return result;

        foreach (var kvp in dict)
        {
            DS.ScriptableObjects.DSDialogueGroupSO groupSo = kvp.Key;
            List<DS.ScriptableObjects.DSDialogueSO> dialogues = kvp.Value;

            GroupedDialogueData gd = new GroupedDialogueData
            {
                groupName = (groupSo != null) ? groupSo.GroupName : "(NULL_GROUP)",
                dialogues = ExtractDialogues(dialogues)
            };
            result.Add(gd);
        }
        return result;
    }

    private List<ChoiceData> ExtractChoices(List<DS.Data.DSDialogueChoiceData> choices)
    {
        List<ChoiceData> cList = new List<ChoiceData>();
        if (choices == null) return cList;
        foreach (var c in choices)
        {
            cList.Add(new ChoiceData
            {
                text = c.Text,
                nextDialogueName = (c.NextDialogue != null) ? c.NextDialogue.DialogueName : "null"
            });
        }
        return cList;
    }

    // --------------------------------------------------------------------
    // (D) KHAI BÁO CÁC LỚP DỮ LIỆU SERIALIZABLE
    // --------------------------------------------------------------------
    [System.Serializable]
    public class PlayerData
    {
        public string session_id;
        public PlayerDetails details;
    }

    [System.Serializable]
    public class PlayerDetails
    {
        public int score;
        public float x;
        public float y;
        public float z;
        public int level;
        public int FirstTimeQuest;
        public float HealthSlider;
        public int Medal;
        public int bonus_medal;
        public string student_id;
        public string fullname;
        public int gold;
        public string CharacterName;
        public int gender;

        // Thêm để lưu JSON DialogSystem
        public string dialoguesJson;
        public QuestSaveData questdata;
        public InventoryData inventory;

        // Nếu bạn có QuestMap, Inventory, v.v... tuỳ bạn thêm



        public PlayerDetails(
            float x, float y, float z,
            int firstTimeQuest,
            float healthSlider,
            string student_id,
            string fullname,
            int gold, 
            int Medal,
        int bonus_medal,
            string CharacterName, int gender)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.FirstTimeQuest = firstTimeQuest;
            this.HealthSlider = healthSlider;
            this.student_id = student_id;
            this.fullname = fullname;
            this.gold = gold;
            this.Medal = Medal;
            this.bonus_medal = bonus_medal;
            this.CharacterName = CharacterName;
            this.gender = gender;
        }
    }

    [System.Serializable]
    public class ResponseData
    {
        public int code;
        public string desc;
    }

    // -------------------- CÁC LỚP PHỤ ĐỂ GOM DỮ LIỆU DIALOGUE --------------------
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

        public DSDialogueContainerData containerData;  // dành cho DSDialogueContainerSO
        public DSDialogueData dialogueData;            // dành cho DSDialogueSO
        public DSDialogueGroupData groupData;          // dành cho DSDialogueGroupSO
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

    /// <summary>
    /// Đây là class bạn cần để tránh lỗi "DSDialogueGroupData could not be found".
    /// </summary>
    [System.Serializable]
    public class DSDialogueGroupData
    {
        public string groupName;
    }


    // -------------------- Quest --------------------

    [System.Serializable]
    public class QuestSaveData
    {
        public string session_id;
        public int playerId;
        public List<QuestEntry> quests = new List<QuestEntry>();
    }

    [System.Serializable]
    public class QuestEntry
    {
        public string questId;
        public QuestData questData;
    }

    // -------------------- Inventory --------------------

    [System.Serializable]
    public class InventoryItemData
    {
        public string itemName;
        public int quantity;
    }

    [System.Serializable]
    public class InventoryData
    {
        public List<InventoryItemData> items = new List<InventoryItemData>();
    }
}
