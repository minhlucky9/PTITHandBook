using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using static GlobalResponseData_Login;

public class PostMethod_LeaderBoard : MonoBehaviour
{
    public TMP_InputField outputArea; // TextMeshPro element to display status messages

    // UI Text elements for the top 5 players
    public TextMeshProUGUI[] fullNameTexts;
    public TextMeshProUGUI[] scoreTexts;
    public TextMeshProUGUI[] student_idTexts;
    public TextMeshProUGUI[] MedalTexts;

    public GameObject leaderboardEntryPrefab;    // Prefab có gắn LeaderboardEntryUI
    public Transform leaderboardContentParent;   // GameObject cha (VerticalLayoutGroup)

    private string apiUrl = "http://games.ptit.edu.vn:8080/DemoBackend3D_API/player/getLeaderboard";

    void Start()
    {
        //if (outputArea == null)
        //{
        //    outputArea = GameObject.Find("OutputArea").GetComponent<TMP_InputField>();
        //}
        LoadLeaderboardData();
    }

    public void LoadLeaderboardData()
    {
        StartCoroutine(LoadLeaderboardData_Coroutine());
    }

    IEnumerator LoadLeaderboardData_Coroutine()
    {
        //outputArea.text = "Loading leaderboard data...";

        // Create session data object
        SessionData sessionData = new SessionData
        {
            session_id = GlobalResponseData.session_id
        };

        // Convert session data to JSON
        string jsonData = JsonUtility.ToJson(sessionData);

        // Create UnityWebRequest for POST
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for response
        yield return request.SendWebRequest();

        // Handle errors or successful response
        if (request.isNetworkError || request.isHttpError)
        {
            //outputArea.text = request.error;
            Debug.Log(request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            LeaderboardResponse responseData = JsonUtility.FromJson<LeaderboardResponse>(jsonResponse);

            if (responseData.code == 200)
            {
                //outputArea.text = "Top 5 Players Loaded Successfully!";
                Debug.Log("Top 5 Players Loaded Successfully!");
                // Sort the leaderboard by score in descending order and take the top 5 players
                List<LeaderboardPlayer> topPlayers = responseData.leaderboard_player.Take(30).ToList();

                foreach (Transform child in leaderboardContentParent)
                    Destroy(child.gameObject);

                for (int i = 0; i < topPlayers.Count; i++)
                {
                    var player = topPlayers[i];
                    GameObject go = Instantiate(leaderboardEntryPrefab, leaderboardContentParent);
                    LeaderboardEntryUI entryUI = go.GetComponent<LeaderboardEntryUI>();
                    entryUI.SetData(
                        order: i + 1,                                 // Thứ tự 1→30
                        progress: player.bonus_quest.ToString(),           // Tiến trình
                        playerName: player.fullname,                      // Tên
                        studentID: player.student_id,                    // Mã SV
                        coin: player.score.ToString()               // Coin
                    );
                }
            }
            else
            {
                //outputArea.text = $"Error {responseData.code}: {responseData.desc}";
            }
        }
    }

    [System.Serializable]
    public class SessionData
    {
        public string session_id;
    }

    [System.Serializable]
    public class LeaderboardResponse
    {
        public int code;
        public string desc;
        public List<LeaderboardPlayer> leaderboard_player;
    }

    [System.Serializable]
    public class LeaderboardPlayer
    {
        public int ID;
        public string name;
        public string last_login;
        public string fullname;
        public string student_id;
        public int score;
        public int quest;
        public int bonus_quest;
        public Details_state details_state; // This should match the response
    }

    [System.Serializable]
    public class Details_state
    {
        public int Medal;
    }

}
