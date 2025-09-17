using TMPro;
using UnityEngine;

public class LeaderboardEntryUI : MonoBehaviour
{
    [Header("Tham chiếu các Text trong Prefab")]
    public TextMeshProUGUI orderText;       // Thứ tự
    public TextMeshProUGUI progressText;    // Tiến trình (Medal)
    public TextMeshProUGUI playerNameText;  // Tên người chơi
    public TextMeshProUGUI studentIDText;   // Mã sinh viên
    public TextMeshProUGUI coinText;        // Coin (Điểm)

    /// <summary>
    /// Gán dữ liệu vào các Text
    /// </summary>
    public void SetData(int order, string progress, string playerName, string studentID, string coin)
    {
        orderText.text = order.ToString();
        progressText.text = progress;
        playerNameText.text = playerName;
        studentIDText.text = studentID;
        coinText.text = coin;
    }
}
