using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [Header("Assign 8 Panels ở đây (theo thứ tự)")]
    public GameObject[] panels;  // mảng 8 panel tương ứng với 8 button

    [Header("Assign 8 Button ở đây (theo thứ tự)")]
    public Button[] buttons;     // mảng 8 button

    public string[] panelNames; // mảng tên panel tương ứng với từng button

    [Header("TextMeshProUGUI để hiển thị tên Panel")]
    public TextMeshProUGUI panelNameText;
    public TextMeshProUGUI goldText;

    private void Awake()
    {
        // Đảm bảo mảng đã được set đủ phần tử
        if (panels.Length != buttons.Length)
        {
            Debug.LogError("Số lượng panels và buttons phải bằng nhau!");
            return;
        }

        // Gán listener cho từng button
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;  // bắt giá trị i vào biến cục bộ để tránh lỗi closure
            buttons[i].onClick.AddListener(() => ShowOnlyPanel(index));
        }

        // [Tuỳ chọn] Hiển thị panel đầu tiên khi start
        ShowOnlyPanel(0);
    }

    private void Start()
    {
        PlayerInventory.instance.OnGoldChanged += UpdateGoldText;
    }

    public void UpdateGoldText(int changeAmount, int goldAmount)
    {
        goldText.text = goldAmount.ToString();
    }

    /// <summary>
    /// Hiển thị đúng panel tại vị trí index, ẩn tất cả panel còn lại.
    /// </summary>
    public void ShowOnlyPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }

        if (panelNameText != null)
        {
           
            panelNameText.text = panelNames[index];
        }
    }
}
