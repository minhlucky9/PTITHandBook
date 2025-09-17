using UnityEngine;

public class AnyInputSwitcher : MonoBehaviour
{
    [Header("Assign your UI panels here")]
    public GameObject currentUI;    // UI đang hiển thị
    public GameObject nextUI;       // UI muốn chuyển đến

    private bool hasSwitched = false;

    void Update()
    {
        // Nếu chưa chuyển UI và người chơi ấn phím hoặc click chuột
        if (!hasSwitched && (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            SwitchUI();
            hasSwitched = true; // chỉ switch một lần, bỏ nếu muốn cho switch nhiều lần
        }
    }

    private void SwitchUI()
    {
        if (currentUI != null) currentUI.SetActive(false);
        if (nextUI != null) nextUI.SetActive(true);
    }
}
