using PlayerController;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Interaction;
using System.Collections;
using static GlobalResponseData_Login; // để dùng InventoryItem, UsableItemSO, QuestItemSO

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager instance;

    [Header("UI chính")]
    public TMP_Text goldText;
    public TMP_Text goldText2;         
    public TMP_Text countText;              // hiển thị “số lượng/8”
    public Button usableTabBtn;             // nút chuyển sang đồ dùng
    public Button questTabBtn;              // nút chuyển sang nhiệm vụ
    public GameObject itemPrefab;           // đang dùng cho slot (không đổi)
    public Transform inventoryContainer;    // chứa 8 slot sẵn có
    public Button closeBtn;

    [Header("Detail Prefab")]
    public GameObject detailUIPrefab;       // prefab InventoryItemDetailUI
    public Transform detailUIParent;        // parent để hiển thị detail

    [Header("Tab Indicators")]
    public GameObject usableTabIndicator;   // ví dụ: underline, highlight cho Usable
    public GameObject questTabIndicator;

    UIAnimationController uiAnimation;
    private bool showingUsable = true;      // đang hiển thị Usable hay Quest

    private void Awake()
    {
        instance = this;
        uiAnimation = GetComponent<UIAnimationController>();

        closeBtn.onClick.AddListener(() =>
            StartCoroutine(CloseWindow())
        );
    }

    private void Start()
    {
        // cập nhật số vàng
        PlayerInventory.instance.OnGoldChanged += UpdateGoldText;
        goldText.text = PlayerInventory.instance.gold.ToString();
        goldText2.text = PlayerInventory.instance.gold.ToString();
        // cập nhật inventory
        PlayerInventory.instance.OnInventoryUpdated += _ => RefreshUI();

        // tab buttons
        usableTabBtn.onClick.AddListener(() =>
        {
            showingUsable = true;
            RefreshUI();
            UpdateTabIndicators();
        });
        questTabBtn.onClick.AddListener(() =>
        {
            showingUsable = false;
            RefreshUI();
            UpdateTabIndicators();
        });

        // khởi tạo lần đầu
        RefreshUI();
        UpdateTabIndicators();
    }

    public void UpdateGoldText(int changeAmount, int goldAmount)
    {
        goldText.text = goldAmount.ToString();
        goldText2.text = goldAmount.ToString();
    }

    /// <summary>
    /// Bật/tắt indicator dựa trên tab đang active
    /// </summary>
    private void UpdateTabIndicators()
    {
        if (usableTabIndicator != null)
            usableTabIndicator.SetActive(showingUsable);
        if (questTabIndicator != null)
            questTabIndicator.SetActive(!showingUsable);
    }

    /// <summary>
    /// Gọi lại mỗi khi mở hoặc inventory thay đổi
    /// </summary>
    public void RefreshUI()
    {
        // 1. Xoá bất cứ detail UI cũ
        foreach (Transform t in detailUIParent) Destroy(t.gameObject);

        // 2. Lấy danh sách cặp (item, index gốc trong PlayerInventory)
        var all = PlayerInventory.instance.inventoryItems
                   .Select((inv, idx) => new { inv, idx })
                   .Where(x => !x.inv.IsEmpty &&
                              (showingUsable && x.inv.item is UsableItemSO ||
                               !showingUsable && x.inv.item is QuestItemSO))
                   .ToList();

        // 3. Cập nhật số lượng “đã dùng/8”
        int slotCount = inventoryContainer.childCount; // giả sử = 8
        countText.text = $"{all.Count}/{slotCount}";

        // 4. Lặp qua từng slot trong giao diện
        for (int i = 0; i < slotCount; i++)
        {
            var slotUI = inventoryContainer.GetChild(i)
                             .GetComponent<InventoryItemUI>();
            slotUI.ResetInventorySlot();

            if (i < all.Count)
            {
                // truyền vào đúng InventoryItem và vị trí gốc
                slotUI.InitInventorySlot(all[i].inv, all[i].idx);
            }
        }


    }

    /// <summary>
    /// Mở detail preview khi người dùng click slot
    /// </summary>
    public void ShowItemDetail(InventoryItem item, int slotIndex)
    {
        foreach (Transform child in detailUIParent)
            Destroy(child.gameObject);


        var go = Instantiate(detailUIPrefab, detailUIParent);
        var detailUI = go.GetComponent<InventoryItemDetailUI>();
        detailUI.InitDetail(item, slotIndex);
    }

    private IEnumerator OpenWindow()
    {
        PlayerManager.instance.DeactivateController();
        PlayerManager.instance.isInteract = true;
        yield return new WaitForSeconds(0.7f);
        uiAnimation.Activate();
        RefreshUI();
        
    }

    private IEnumerator CloseWindow()
    {
        uiAnimation.UpdateObjectChange();
        uiAnimation.Deactivate();
        PlayerManager.instance.isInteract = false;
        yield return new WaitForSeconds(0.7f);
        PlayerManager.instance.ActivateController();
    }

    public void OpenInventory()
    {
        StartCoroutine(OpenWindow());
    }

/*

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.I) && !PlayerManager.instance.isInteract)
            StartCoroutine(OpenWindow());
    }
*/
}
