using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordMinigame : MonoBehaviour
{
    public static PasswordMinigame instance;

    [Header("Settings")]
    [SerializeField] private int maxAttempts = 3;
    [SerializeField] private float feedbackDuration = 2f;

    // UI Elements được truyền vào từ Init
    public UIAnimationController passwordContainer;
    private TMP_InputField passwordInputField;
    private TextMeshProUGUI feedbackText;
    private Button checkButton;
    private TextMeshProUGUI titleText;

    private string correctPassword;
    private int currentAttempts = 0;
    private bool isCompleted = false;

    private System.Action onComplete;
    private System.Action onFail;

    

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Khởi tạo minigame mật mã với tất cả UI elements được truyền vào (dành cho 3 chữ số)
    /// </summary>
    /// 

    /// <summary>
    /// Khởi tạo minigame mật mã với tất cả UI elements được truyền vào (dành cho 3 chữ số)
    /// </summary>
    public void InitForInterger(UIAnimationController container,
                     TMP_InputField inputField,
                     Button button,
                     TextMeshProUGUI title,
                     TextMeshProUGUI feedbackText,
                     string password,
                     string titleMessage = "Nhập mật mã",
                     System.Action onCompleteCallback = null,
                     System.Action onFailCallback = null)
    {
        if (passwordInputField != null)
            passwordInputField.interactable = true;
        if (checkButton != null)
            checkButton.interactable = true;

        // Validate inputs
        if (container == null)
        {
            Debug.LogError("Password container is null!");
            return;
        }

        if (inputField == null)
        {
            Debug.LogError("Input field is null!");
            return;
        }

        if (button == null)
        {
            Debug.LogError("Check button is null!");
            return;
        }

        // Assign UI elements
        passwordContainer = container;
        passwordInputField = inputField;
        checkButton = button;
        titleText = title;
        this.feedbackText = feedbackText;


        // Setup game state
        correctPassword = password;
        onComplete = onCompleteCallback;
        onFail = onFailCallback;
        currentAttempts = 0;
        isCompleted = false;
        passwordInputField.characterLimit = 3;
        passwordInputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
        SetupUI(titleMessage);
        Show();
    }

    public void Init(UIAnimationController container,
                     TMP_InputField inputField,
                     Button button,
                     TextMeshProUGUI title,
                     TextMeshProUGUI feedbackText,
                     string password,
                     string titleMessage = "Nhập mật mã",
                     System.Action onCompleteCallback = null,
                     System.Action onFailCallback = null)
    {
        if (passwordInputField != null)
            passwordInputField.interactable = true;
        if (checkButton != null)
            checkButton.interactable = true;

        // Validate inputs
        if (container == null)
        {
            Debug.LogError("Password container is null!");
            return;
        }

        if (inputField == null)
        {
            Debug.LogError("Input field is null!");
            return;
        }

        if (button == null)
        {
            Debug.LogError("Check button is null!");
            return;
        }

        // Assign UI elements
        passwordContainer = container;
        passwordInputField = inputField;
        checkButton = button;
        titleText = title;
        this.feedbackText = feedbackText;

        // Setup game state
        correctPassword = password;
        onComplete = onCompleteCallback;
        onFail = onFailCallback;
        currentAttempts = 0;
        isCompleted = false;
        SetupUI(titleMessage);
        Show();
    }

    /// <summary>
    /// Overload Init method - tự động tìm UI elements trong container (backward compatible)
    /// </summary>
    public void Init(UIAnimationController container,
                     string password,
                     string titleMessage = "Nhập mật mã",
                     System.Action onCompleteCallback = null,
                     System.Action onFailCallback = null)
    {
        if (container == null)
        {
            Debug.LogError("Password container is null!");
            return;
        }

        // Tự động tìm UI elements
        TMP_InputField inputField = container.GetComponentInChildren<TMP_InputField>();
        Button button = container.GetComponentInChildren<Button>();

        TextMeshProUGUI[] texts = container.GetComponentsInChildren<TextMeshProUGUI>();
        TextMeshProUGUI title = null;
        TextMeshProUGUI feedback = null;

        foreach (var text in texts)
        {
            if (title == null && text.gameObject.name.ToLower().Contains("title"))
            {
                title = text;
            }
            else if (feedback == null && (text.gameObject.name.ToLower().Contains("feedback") ||
                                         text.gameObject.name.ToLower().Contains("result")))
            {
                feedback = text;
            }
        }

        // Gọi Init chính với các elements đã tìm được
        InitForInterger(container, inputField, button, title, feedback, password, titleMessage, onCompleteCallback, onFailCallback);
    }

    /// <summary>
    /// Cài đặt UI
    /// </summary>
    private void SetupUI(string titleMessage)
    {
        // Setup title
        if (titleText != null)
        {
            titleText.text = titleMessage;
        }

        // Setup input field
        if (passwordInputField != null)
        {
            passwordInputField.text = "";
           // passwordInputField.contentType = TMP_InputField.ContentType.Password;
            passwordInputField.onSubmit.RemoveAllListeners();
            passwordInputField.onSubmit.AddListener(OnPasswordSubmit);
            passwordInputField.onEndEdit.RemoveAllListeners();
            passwordInputField.onEndEdit.AddListener(OnPasswordEndEdit);
            passwordInputField.interactable = true;
            passwordInputField.Select();
            passwordInputField.ActivateInputField();
        }

        // Setup button
        if (checkButton != null)
        {
            checkButton.onClick.RemoveAllListeners();
            checkButton.onClick.AddListener(OnCheckButtonClick);
            checkButton.interactable = true;

            // Cập nhật text của button nếu có
            TextMeshProUGUI buttonText = checkButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null && !buttonText.text.ToLower().Contains("kiểm"))
            {
                buttonText.text = "Kiểm tra";
            }
        }

        // Setup feedback text
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
            feedbackText.text = "";
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút kiểm tra
    /// </summary>
    private void OnCheckButtonClick()
    {
        CheckPassword();
    }

    /// <summary>
    /// Xử lý khi nhấn Enter trong input field
    /// </summary>
    private void OnPasswordSubmit(string text)
    {
        CheckPassword();
    }

    /// <summary>
    /// Xử lý khi kết thúc edit (để tránh submit 2 lần)
    /// </summary>
    private void OnPasswordEndEdit(string text)
    {
        // Re-select input field nếu chưa complete
        if (!isCompleted && passwordInputField != null)
        {
            passwordInputField.Select();
            passwordInputField.ActivateInputField();
        }
    }

    /// <summary>
    /// Kiểm tra mật khẩu
    /// </summary>
    private void CheckPassword()
    {
        if (isCompleted) return;

        if (passwordInputField == null) return;

        string enteredPassword = passwordInputField.text.Trim();

        if (string.IsNullOrEmpty(enteredPassword))
        {
            ShowFeedback("Vui lòng nhập mật mã!", Color.yellow);
            return;
        }

        currentAttempts++;

        if (enteredPassword == correctPassword)
        {
            // Đúng mật mã
            ShowFeedback("Mật mã chính xác!", Color.green);
            isCompleted = true;

            // Disable input
            if (passwordInputField != null)
                passwordInputField.interactable = false;
            if (checkButton != null)
                checkButton.interactable = false;

            StartCoroutine(CompleteAfterDelay());
        }
        else
        {
            // Sai mật mã
            if (currentAttempts >= maxAttempts)
            {
                ShowFeedback($"Sai mật mã! Bạn đã hết {maxAttempts} lần thử.", Color.red);
                isCompleted = true;

                // Disable input
                if (passwordInputField != null)
                    passwordInputField.interactable = false;
                if (checkButton != null)
                    checkButton.interactable = false;

                StartCoroutine(FailAfterDelay());
            }
            else
            {
                int remainingAttempts = maxAttempts - currentAttempts;
                ShowFeedback($"Sai mật mã! Còn {remainingAttempts} lần thử.", Color.red);

                // Clear input để thử lại
                if (passwordInputField != null)
                {
                    passwordInputField.text = "";
                    passwordInputField.Select();
                    passwordInputField.ActivateInputField();
                }
            }
        }
    }

    /// <summary>
    /// Hiển thị feedback
    /// </summary>
    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = message;
            feedbackText.color = color;

            StopCoroutine(nameof(HideFeedbackAfterDelay));
            StartCoroutine(HideFeedbackAfterDelay());
        }
        else
        {
            Debug.Log($"Password Feedback: {message}");
        }
    }

    /// <summary>
    /// Ẩn feedback sau một khoảng thời gian
    /// </summary>
    private IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);

        if (feedbackText != null && !isCompleted)
            feedbackText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Hoàn thành minigame sau delay
    /// </summary>
    private IEnumerator CompleteAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);

        Debug.Log("Password Minigame Completed!");
        Hide();
        onComplete?.Invoke();
    }

    /// <summary>
    /// Thất bại minigame sau delay
    /// </summary>
    private IEnumerator FailAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        Debug.Log("Password Minigame Failed!");
        Hide();
        onFail?.Invoke();
    }

    /// <summary>
    /// Hiển thị minigame
    /// </summary>
    public void Show()
    {
        if (passwordContainer != null)
        {
            passwordContainer.Activate();

            // Focus vào input field
            if (passwordInputField != null)
            {
                passwordInputField.Select();
                passwordInputField.ActivateInputField();
            }
        }
    }

    /// <summary>
    /// Ẩn minigame
    /// </summary>
    public void Hide()
    {
       
            passwordContainer.Deactivate();
    }

    /// <summary>
    /// Reset minigame với các elements hiện tại
    /// </summary>
    public void Reset()
    {
        currentAttempts = 0;
        isCompleted = false;

        if (passwordInputField != null)
        {
            passwordInputField.text = "";
            passwordInputField.interactable = true;
        }

        if (checkButton != null)
            checkButton.interactable = true;

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
            feedbackText.text = "";
        }
    }

    /// <summary>
    /// Thiết lập mật khẩu mới cho container hiện tại
    /// </summary>
    public void SetPassword(string newPassword)
    {
        correctPassword = newPassword;
        Reset();
    }

    /// <summary>
    /// Thiết lập số lần thử tối đa
    /// </summary>
    public void SetMaxAttempts(int attempts)
    {
        maxAttempts = Mathf.Max(1, attempts);
    }

    /// <summary>
    /// Lấy số lần thử còn lại
    /// </summary>
    public int GetRemainingAttempts()
    {
        return Mathf.Max(0, maxAttempts - currentAttempts);
    }

    /// <summary>
    /// Kiểm tra xem minigame đã hoàn thành chưa
    /// </summary>
    public bool IsCompleted()
    {
        return isCompleted;
    }

    /// <summary>
    /// Lấy container hiện tại
    /// </summary>
    public UIAnimationController GetCurrentContainer()
    {
        return passwordContainer;
    }

    /// <summary>
    /// Lấy các UI elements hiện tại
    /// </summary>
    public (TMP_InputField inputField, Button button, TextMeshProUGUI title, TextMeshProUGUI feedback) GetCurrentUIElements()
    {
        return (passwordInputField, checkButton, titleText, feedbackText);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}