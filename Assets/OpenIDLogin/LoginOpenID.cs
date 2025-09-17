using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static GlobalResponseData_Login;

public class LoginOpenID : MonoBehaviour
{
    string access_token = "";
    string refresh_token = "";
    string id_token = "";
    TokenClass tokenInfo = null;
    public string clientID = "";
    public string token_url = "";
    public string auth_url = "";
    public string data_url = "";
    public string logout_url = "";

    [Header("Buttons")]
    public Button loginBtn;
    public Button continueBtn;
    public Button changeAccountBtn;

    [Header("UI")]
    [SerializeField] private GameObject SuccessUI;
    [SerializeField] private GameObject FailureUI;
    [SerializeField] private GameObject NoInternetUI;
    [SerializeField] private GameObject FailSlinkUI;
    public PlayerDataLoader playerDataLoader;

    private void Start()
    { 

        if (PlayerPrefs.HasKey("refresh_token"))
        {
            refresh_token = PlayerPrefs.GetString("refresh_token");
        }
        
        loginBtn.onClick.AddListener(async delegate
        {
            string token = await GetLoginTokenAsync();
            StartCoroutine(GetProtectedData(data_url, token));
        });

        //continueBtn.onClick.AddListener(async delegate
        //{
        //    string token = RefreshAccessToken();
        //    if (token == null)
        //    {
        //        token = await GetLoginTokenAsync();
        //    }
        //    StartCoroutine(GetProtectedData(data_url, token));
        //});

        changeAccountBtn.onClick.AddListener(async delegate
        {
            string token = await GetChangeAuthenticationTokenAsync();
            StartCoroutine(GetProtectedData(data_url, token));
        });
    }

    

    public async Task<string> GetChangeAuthenticationTokenAsync()
    {

        string redirectURI = string.Format("http://{0}:{1}/", "localhost", GetRandomUnusedPort());
        string RedirectURIWithoutLastSlash = redirectURI.TrimEnd('/');

        string code = await GetCodeForChangeAuthentication(redirectURI, RedirectURIWithoutLastSlash);


        Dictionary<string, string> postParameters = new Dictionary<string, string>()
            {
            { "grant_type", "authorization_code"},
                { "code", code},
                { "client_id", clientID},
                { "redirect_uri", RedirectURIWithoutLastSlash }
        };
        string data = PostRequest(postParameters, token_url);
       
        TokenClass token = JsonUtility.FromJson<TokenClass>(data);

        access_token = token.access_token;
        refresh_token = token.refresh_token;
        id_token = token.id_token;

        tokenInfo = token;
        //Debug.Log("access token is: " + access_token);
        //Debug.Log("id token is: " + tokenInfo.id_token);
        //
        PlayerPrefs.SetString("refresh_token", refresh_token);
        PlayerPrefs.SetString("id_token", id_token);
        PlayerPrefs.Save();
        return access_token;
    }

    private async Task<string> GetCodeForChangeAuthentication(string redirectURI, string RedirectURIWithoutLastSlash)
    {

        string code = null;

        HttpListener http = new HttpListener();
        http.Prefixes.Add(redirectURI);
        http.Start();

        string authorizationRequest = $"{auth_url}?client_id={clientID}&response_type=code&scope=openid%20profile%20email&redirect_uri={RedirectURIWithoutLastSlash}&state=abc123xyz";

        string logoutRequest = $"{logout_url}?id_token_hint={id_token}&post_logout_redirect_uri={RedirectURIWithoutLastSlash}&client_id={clientID}";

        Application.runInBackground = true;
        // Opens request in the browser.
        Application.OpenURL(logoutRequest);

        try
        {
            var context = await http.GetContextAsync();
            var response = context.Response;

            var html = $"<html><head><meta http-equiv='refresh' content='0, {authorizationRequest}' ></head><body> <script></script> Logout successfully, Please return to the game!</body></html>";
            string responseString = string.Format(html);
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length);

            context = await http.GetContextAsync();
            response = context.Response;
 
            html = $"<html><head><meta http-equiv='refresh'></head><body>Login successfully, Please return to the game! <script> alert(\"Login successfully, Please return to the game!\"); window.close();</script></body></html>";
            responseString = string.Format(html);
            buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            responseOutput = response.OutputStream;
            responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });


            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                string debug = context.Request.QueryString.Get("error");

            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                var debug = context.Request.QueryString;
                var debug3 = context.Request.RawUrl;
            }
            // extracts the code
            code = context.Request.QueryString.Get("code");

            string tryCode = context.Request.QueryString["code"];

            Debug.Log("code is: " + code);

        }
        catch (Exception ex)
        {
            string g = ex.Message;
        }

        //wait for login
        //code = await GetCodeForAuthentication(redirectURI, RedirectURIWithoutLastSlash);

        Application.runInBackground = false;

        return code;
    }

    string RefreshAccessToken()
    {
        Dictionary<string, string> postParameters = new Dictionary<string, string>()
            {
            { "grant_type", "refresh_token"},
                { "refresh_token", refresh_token},
                { "client_id", clientID}
        };
        string data = PostRequest(postParameters, token_url);
        Debug.Log("text by server is: " + data);

        TokenClass token = JsonUtility.FromJson<TokenClass>(data);

        access_token = token.access_token;
        refresh_token = token.refresh_token;
        id_token = token.id_token;

        tokenInfo = token;
        Debug.Log("access token is: " + access_token);
        Debug.Log("id token is: " + tokenInfo.id_token);
        //
        PlayerPrefs.SetString("refresh_token", refresh_token);
        PlayerPrefs.SetString("id_token", id_token);
        PlayerPrefs.Save();
        return access_token;
    }

    string PostRequest(Dictionary<string, string> postParameters, string url)
    {
        UnityWebRequest request = new UnityWebRequest();
        request.url = $"{url}";

        request.method = UnityWebRequest.kHttpVerbPOST;
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        string postData = "";

        foreach (string key in postParameters.Keys)
            postData += UnityWebRequest.EscapeURL(key) + "=" + UnityWebRequest.EscapeURL(postParameters[key]) + "&";

        byte[] data = Encoding.ASCII.GetBytes(postData);

        request.uploadHandler = new UploadHandlerRaw(data);

        request.timeout = 60;

        request.SendWebRequest();

        while (!request.isDone)
        {
            //Debug.Log(request.downloadProgress);
        }

        return request.downloadHandler.text;
    }

    public async Task<string> GetLoginTokenAsync()
    {

        string redirectURI = string.Format("http://{0}:{1}/", "localhost", GetRandomUnusedPort());
        string RedirectURIWithoutLastSlash = redirectURI.TrimEnd('/');

        string code = await GetCodeForAuthentication(redirectURI, RedirectURIWithoutLastSlash);


        Dictionary<string, string> postParameters = new Dictionary<string, string>() 
            {
            { "grant_type", "authorization_code"},
                { "code", code},
                { "client_id", clientID},
                { "redirect_uri", RedirectURIWithoutLastSlash }
        };
        string data = PostRequest(postParameters, token_url);
        Debug.Log("text by server is: " + data);

        TokenClass token = JsonUtility.FromJson<TokenClass>(data);

        access_token = token.access_token;
        refresh_token = token.refresh_token;
        id_token = token.id_token;

        tokenInfo = token;
        Debug.Log("access token is: " + access_token);
        Debug.Log("id token í: " + tokenInfo.id_token);
        //
        PlayerPrefs.SetString("refresh_token", refresh_token);
        PlayerPrefs.SetString("id_token", id_token);
        PlayerPrefs.Save();
        return access_token;
    }

    public IEnumerator GetProtectedData(string url, string bearerToken)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Set the Authorization header with the Bearer token
            webRequest.SetRequestHeader("Authorization", "Bearer " + bearerToken);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + webRequest.downloadHandler.text);

                JSONObject json = new JSONObject(webRequest.downloadHandler.text);
                JSONObject data = json.GetField("data");

                StudentInfo studentInfo = new StudentInfo(data.GetField("ma").ToString(), 
                    data.GetField("ten").ToString(), 
                    data.GetField("email").ToString(), 
                    data.GetField("soDienThoai").ToString());

                Debug.Log(studentInfo.ma + " " + studentInfo.ten + " " + studentInfo.soDienThoai + " " + studentInfo.email);
                //api ben Hieu

                string uri = "http://games.ptit.edu.vn:8080/DemoBackend3D_API/user/login";
                WWWForm form = new WWWForm();
                form.AddField("student_id", studentInfo.ma.Replace("\"", ""));
                form.AddField("fullname", studentInfo.ten.Replace("\"", ""));
                form.AddField("email", studentInfo.email.Replace("\"", ""));
                form.AddField("tel", studentInfo.soDienThoai.Replace("\"", ""));
                form.AddField("password", studentInfo.soDienThoai.Replace("\"", ""));
                using (UnityWebRequest request = UnityWebRequest.Post(uri, form))
                {
                    yield return request.SendWebRequest();
                    if (request.isNetworkError || request.isHttpError)
                    {
                        //    outputArea.text = request.error;
                    }
                    else
                    {
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

                            StartCoroutine(CheckSaveData());
                        }
                        else if (responseData.code == 400)
                        {
                            FailureUI.SetActive(true);

                        }

                    }
                }


            }
            else
            {
                Debug.Log("Không tìm thấy thông tin sinh viên!");
                Debug.LogError("Error: " + webRequest.error);
                FailSlinkUI.SetActive(true);
            }

        }

    }

    private async Task<string> GetCodeForAuthentication(string redirectURI, string RedirectURIWithoutLastSlash)
    {

        string code = null;

        HttpListener http = new HttpListener();
        http.Prefixes.Add(redirectURI);
        http.Start();

        string myID = "ptit-connect";

        string authorizationRequest = $"{auth_url}?client_id={clientID}&response_type=code&scope=openid%20profile%20email&redirect_uri={RedirectURIWithoutLastSlash}&state=abc123xyz";

        Application.runInBackground = true;
        // Opens request in the browser.
        Application.OpenURL(authorizationRequest);

        try
        {
            var context = await http.GetContextAsync();
            var response = context.Response;

            var html = $"<html><head><meta http-equiv='refresh'></head><body> Login successfully, Please return to the game! <script> alert(\"Login successfully, Please return to the game!\"); window.location.replace(\"stsv://\"); window.close();</script></body></html>";


            string responseString = string.Format(html);
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                
                Console.WriteLine("HTTP server stopped.");
            });


            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                string debug = context.Request.QueryString.Get("error");

            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {

                var debug = context.Request.QueryString;

                var debug3 = context.Request.RawUrl;


            }
            // extracts the code
            code = context.Request.QueryString.Get("code");

            string tryCode = context.Request.QueryString["code"];

            Debug.Log("code is: " + code);
        }
        catch (Exception ex)
        {
            string g = ex.Message;
        }

        Application.runInBackground = false;

        return code;
    }

    public static int GetRandomUnusedPort()
    {
        TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
        try
        {
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            return port;
        }
        finally
        {
            listener.Stop(); // Release the port
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

public class TokenClass
{
    public string access_token;
    public int expires_in;
    public string refresh_token;
    public int refresh_expires_in;
    public string token_type;
    public string id_token;
    public string scope;
}

public class StudentInfo
{
    public string ma;
    public string ten;
    public string email;
    public string soDienThoai;

    public StudentInfo(string _ma, string _ten, string _email, string _soDienThoai)
    {
        ma = _ma; 
        ten = _ten; 
        email = _email;
        soDienThoai = _soDienThoai;
    }
}