using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class PostMethod_SignUp : MonoBehaviour
{
    TMP_InputField outputArea;
   public TMP_InputField student_id;
    public TMP_InputField fullname;
    public TMP_InputField password;
    public TMP_InputField email;
     public TMP_InputField tel;
    public TextMeshProUGUI Respone;
    public GameObject FailedRespone;
    [SerializeField] private GameObject NoInternetUI; // UI thông báo không có kết nối internet
    [SerializeField] private GameObject SuccessOrFailedUI;
    void Start()
    {
        outputArea = GameObject.Find("OutputArea").GetComponent<TMP_InputField>();
        GameObject.Find("PostButton ").GetComponent<Button>().onClick.AddListener(PostData);
    }

   public void PostData() => StartCoroutine(PostData_Coroutine());

    IEnumerator PostData_Coroutine()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            outputArea.text = "Không có kết nối internet.";
            NoInternetUI.SetActive(true); // Hiển thị UI thông báo không có internet
            yield break; // Dừng coroutine
        }
        outputArea.text = "Loading...";
        string uri = "http://games.ptit.edu.vn:8080/DemoBackend3D_API/user/signup";
        WWWForm form = new WWWForm();
        form.AddField("student_id", student_id.text + "(guest)");
        form.AddField("fullname", fullname.text);
        form.AddField("password", password.text);
        form.AddField("email", email.text);
        form.AddField("tel", tel.text);

        using (UnityWebRequest request = UnityWebRequest.Post(uri, form))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                outputArea.text = request.error;
            }
            else
            {
                // Assuming the response is a JSON string
                string jsonResponse = request.downloadHandler.text;

                // Parse JSON response
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(jsonResponse);

                // Handle response based on the code
                if (responseData.code == 200)
                {
                    SuccessOrFailedUI.SetActive(true);
                    outputArea.text = "Đăng ký thành công!";
                    Respone.text = "Đăng ký thành công!";
                }
                else if (responseData.code == 400)
                {
                    SuccessOrFailedUI.SetActive(true);
                    outputArea.text = $"Error {responseData.code}: {responseData.desc}";
                    Respone.text = $"Mã sinh viên bị trùng, hãy thử lại";
                  //  FailedRespone.SetActive(false);
                }
                else
                {
                    outputArea.text = $"Unexpected response code: {responseData.code}";
                    Respone.text = $"Unexpected response code: {responseData.code}";
                }
            }
        }
    }

    [System.Serializable]
    public class ResponseData
    {
        public int code;
        public string desc;  // Optional description for error messages
    }
}
