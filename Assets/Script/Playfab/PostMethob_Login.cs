using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using static GlobalResponseData_Login;

public class PostMethod_Login : MonoBehaviour
{
    TMP_InputField outputArea;
    public TMP_InputField MSV;
    public TMP_InputField Password;
    // public TextMeshProUGUI Respone;
    [SerializeField] private GameObject SuccessUI;
    [SerializeField] private GameObject FailureUI;
    [SerializeField] private GameObject NoInternetUI; 

    public PlayerDataLoader playerDataLoader;

    void Start()
    {
        outputArea = GameObject.Find("OutputArea").GetComponent<TMP_InputField>();
        //  GameObject.Find("PostButton ").GetComponent<Button>().onClick.AddListener(PostData_Login);
    }

    public void PostData_Login() => StartCoroutine(PostData_Coroutine());

    IEnumerator PostData_Coroutine()
    {
        // Ki?m tra k?t n?i internet
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            outputArea.text = "Không có kết nối internet.";
            NoInternetUI.SetActive(true); // Hi?n th? UI thông báo không có internet
            yield break; // D?ng coroutine
        }

        if (MSV.text == "" || Password.text == "")
        {
            FailureUI.SetActive(true);
            yield break; // D?ng coroutine
        }

        outputArea.text = "Loading...";
        string uri = "http://games.ptit.edu.vn:8080/DemoBackend3D_API/user/loginGuest";
        WWWForm form = new WWWForm();
        form.AddField("student_id", MSV.text + "(guest)");
        form.AddField("password", Password.text);

        using (UnityWebRequest request = UnityWebRequest.Post(uri, form))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                outputArea.text = request.error;
                FailureUI.SetActive(true); // Hi?n th? UI thông báo không có internet
                yield break; // D?ng coroutine
            }
            else
            {
                Debug.Log("Student ID: " + MSV.text);
                Debug.Log("Password: " + Password.text);

                // Assuming the response is a JSON string
                string jsonResponse = request.downloadHandler.text;
                // Parse JSON response
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(jsonResponse);
                if (responseData.code == 200)
                {
                    Debug.Log(responseData.code);
                    GlobalResponseData.code = responseData.code;
                    GlobalResponseData.user_id = responseData.user_id;
                    GlobalResponseData.fullname = responseData.fullname;
                    GlobalResponseData.student_id = responseData.student_id;
                    GlobalResponseData.session_id = responseData.session_id;
                    GlobalResponseData.role = responseData.role;

                    // Display the parsed data
                    outputArea.text = $"Code: {responseData.code}\n" +
                                      $"User ID: {responseData.user_id}\n" +
                                      $"Student ID: {responseData.student_id}\n" +
                                      $"Full Name: {responseData.fullname}\n" +
                                      $"Session ID: {responseData.session_id}\n" +
                                      $"Role: {responseData.role}";
                   

                  

                    StartCoroutine(CheckSaveData());
                }
                else if (responseData.code == 400)
                {
                    FailureUI.SetActive(true);
                    // Respone.text = $"??ng nh?p th?t b?i. Hãy ki?m tra l?i mã sinh viên ho?c m?t kh?u";
                }

            }
        }
    }

    public IEnumerator CheckSaveData()
    {
        yield return new WaitForSeconds(1f);

        yield return playerDataLoader.LoadPlayerData_Coroutine();


        Debug.Log(GlobalResponseData.FirstTimeQuest);

        if (GlobalResponseData.FirstTimeQuest == 0)
        {
            AsyncLoader.Instance.ApplyToggle(true);
        }
        else
        {
            AsyncLoader.Instance.ApplyToggle(false);
        }

        SuccessUI.SetActive(true);
    }

    [System.Serializable]
    public class ResponseData
    {
        public int code;
        public int user_id;
        public string student_id;
        public string fullname;
        public string session_id;
        public int role;
    }
}
