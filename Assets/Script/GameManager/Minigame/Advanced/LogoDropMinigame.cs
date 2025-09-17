using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LogoDropMinigame : MonoBehaviour
{
    public static LogoDropMinigame instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject itemPrefab;

    [Header("Containers")]
    [SerializeField] private Transform dragContainer;
    [SerializeField] private Transform dropContainer;
    [SerializeField] public UIAnimationController Container;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI slotNameText;
    [SerializeField] private TextMeshProUGUI slotDescriptionText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private int maxAttempts = 3;
    [SerializeField] private float itemSize = 100f;
    [SerializeField] private float spacing = 10f;

    private List<GameObject> dragSlots = new List<GameObject>();
    private GameObject dropSlot;
    private List<GameObject> items = new List<GameObject>();

    private string correctItemID;
    private string correctName;
    private string correctDescription;
    private string wrongDescription = "Sai rồi! Hãy thử lại.";

    private int currentAttempts = 0;
    private DraggableItem currentDroppedItem;
    private System.Action onComplete;
    private System.Action onFail;

    [System.Serializable]
    public class LogoItem
    {
        public string id;
        public string name;
        public string description;
        public Sprite sprite;
        public Color color = Color.white;
    }

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Khởi tạo minigame với danh sách logos
    /// </summary>
    public void Init(List<LogoItem> logoItems, string correctID, System.Action onCompleteCallback = null, System.Action onFailCallback = null)
    {
        if (logoItems == null || logoItems.Count == 0)
        {
            Debug.LogError("Logo items list is empty!");
            return;
        }

        onComplete = onCompleteCallback;
        onFail = onFailCallback;
        correctItemID = correctID;
        currentAttempts = 0;

        // Tìm thông tin của item đúng
        LogoItem correctItem = logoItems.Find(x => x.id == correctID);
        if (correctItem != null)
        {
            correctName = correctItem.name;
            correctDescription = correctItem.description;
        }

        Clear();
        CreateSlots(logoItems.Count);
        CreateLogoItems(logoItems);

        Container.Activate();

        // Ẩn UI ban đầu
        if (slotNameText != null) slotNameText.gameObject.SetActive(false);
        if (slotDescriptionText != null) slotDescriptionText.gameObject.SetActive(false);
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(false);
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
        }

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener( delegate {
            onFailCallback?.Invoke();
        });
    }

    /// <summary>
    /// Tạo các slot
    /// </summary>
    private void CreateSlots(int itemCount)
    {
        // Tạo các drag slots
        int cols = Mathf.CeilToInt(Mathf.Sqrt(itemCount));
        int rows = Mathf.CeilToInt((float)itemCount / cols);

        for (int i = 0; i < itemCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, dragContainer);
            slot.name = $"LogoDragSlot_{i}";

            InventorySlot slotScript = slot.GetComponent<InventorySlot>();
            if (slotScript == null) slotScript = slot.AddComponent<InventorySlot>();
            slotScript.slotID = ""; // Ô gốc
            slotScript.gameManager = TraceQuestManager.instance;

            RectTransform rectTransform = slot.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(itemSize, itemSize);

            int row = i / cols;
            int col = i % cols;
            float xPos = col * (itemSize + spacing);
            float yPos = -row * (itemSize + spacing);
            rectTransform.anchoredPosition = new Vector2(xPos, yPos);

            dragSlots.Add(slot);
        }

        // Tạo 1 drop slot
        dropSlot = Instantiate(slotPrefab, dropContainer);
        dropSlot.name = "LogoDropSlot";

        InventorySlot dropSlotScript = dropSlot.GetComponent<InventorySlot>();
        if (dropSlotScript == null) dropSlotScript = dropSlot.AddComponent<InventorySlot>();
        dropSlotScript.slotID = "TARGET"; // ID đặc biệt cho ô đích
        dropSlotScript.gameManager = TraceQuestManager.instance;

        // Override OnDrop behavior cho logo minigame
        MonoBehaviour.DestroyImmediate(dropSlotScript);
        LogoDropSlot logoDropSlot = dropSlot.AddComponent<LogoDropSlot>();
        logoDropSlot.Initialize(this);

        RectTransform dropRectTransform = dropSlot.GetComponent<RectTransform>();
        dropRectTransform.anchorMax = Vector2.one;
        dropRectTransform.anchorMin = Vector2.zero;
        dropRectTransform.offsetMax = Vector2.zero;
        dropRectTransform.offsetMin = Vector2.zero;
        dropRectTransform.anchoredPosition = Vector2.zero;

        // Thêm outline để dễ nhìn
        Outline outline = dropSlot.AddComponent<Outline>();
        outline.effectColor = Color.yellow;
        outline.effectDistance = new Vector2(2, 2);
    }

    /// <summary>
    /// Tạo các logo items
    /// </summary>
    private void CreateLogoItems(List<LogoItem> logoItems)
    {
        for (int i = 0; i < logoItems.Count && i < dragSlots.Count; i++)
        {
            GameObject item = Instantiate(itemPrefab, dragSlots[i].transform);
            item.name = $"LogoItem_{logoItems[i].id}";

            // Setup DraggableItem
            DraggableItem draggable = item.GetComponent<DraggableItem>();
            if (draggable == null) draggable = item.AddComponent<DraggableItem>();
            draggable.itemID = logoItems[i].id;

            // Setup Image
            Image image = item.GetComponent<Image>();
            if (image == null) image = item.AddComponent<Image>();
            draggable.image = image;

            if (logoItems[i].sprite != null)
            {
                image.sprite = logoItems[i].sprite;
            }
            image.color = logoItems[i].color;

            // Nếu không có sprite, hiển thị text
            if (logoItems[i].sprite == null)
            {
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(item.transform, false);

                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = logoItems[i].name;
                text.fontSize = 16;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.black;

                RectTransform textRect = text.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }

            // Setup RectTransform
            RectTransform rectTransform = item.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(itemSize - 5, itemSize - 5);
            rectTransform.anchoredPosition = Vector2.zero;

            items.Add(item);
        }
    }

    /// <summary>
    /// Xử lý khi item được thả vào drop slot
    /// </summary>
    public void OnItemDropped(DraggableItem item)
    {
        currentDroppedItem = item;
        currentAttempts++;
        item.gameObject.SetActive(false);
        dropSlot.GetComponent<Image>().sprite = item.GetComponent<Image>().sprite;

        bool isCorrect = (item.itemID == correctItemID);

        // Hiển thị UI feedback
        if (slotNameText != null)
        {
            slotNameText.gameObject.SetActive(true);
            slotNameText.text = isCorrect ? correctName : "Sai rồi!";
            slotNameText.color = isCorrect ? Color.green : Color.red;
        }

        if (slotDescriptionText != null)
        {
            slotDescriptionText.gameObject.SetActive(true);
            slotDescriptionText.text = isCorrect ? correctDescription : wrongDescription;
            slotDescriptionText.color = Color.white;
        }

        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(isCorrect);
        }

        if (isCorrect)
        {
            // Khóa item không cho kéo nữa
            item.enabled = false;
            FreezeDraggable();
        }
        else
        {
            // Nếu sai và hết lượt thử
            if (currentAttempts >= maxAttempts)
            {
                StartCoroutine(FailAfterDelay());
            }
            else
            {
                // Hiển thị số lần thử còn lại
                if (slotDescriptionText != null)
                {
                    slotDescriptionText.text = $"{wrongDescription}\nCòn {maxAttempts - currentAttempts} lần thử.";
                }
            }
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút xác nhận
    /// </summary>
    private void OnConfirmButtonClick()
    {
        Debug.Log("Logo Drop Minigame Completed!");
        onComplete?.Invoke();
    }

    /// <summary>
    /// Xử lý khi thất bại
    /// </summary>
    private IEnumerator FailAfterDelay()
    {
        if (slotDescriptionText != null)
        {
            slotDescriptionText.text = "Bạn đã hết lượt thử! Thử lại lần sau.";
            slotDescriptionText.color = Color.red;
        }

        yield return new WaitForSeconds(2f);

        Debug.Log("Logo Drop Minigame Failed!");
        onFail?.Invoke();
    }

    /// <summary>
    /// Reset minigame
    /// </summary>
    public void Reset()
    {
        currentAttempts = 0;
        currentDroppedItem = null;

        // Ẩn UI
        if (slotNameText != null) slotNameText.gameObject.SetActive(false);
        if (slotDescriptionText != null) slotDescriptionText.gameObject.SetActive(false);
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);

        // Reset items về vị trí ban đầu
        for (int i = 0; i < items.Count && i < dragSlots.Count; i++)
        {
            items[i].transform.SetParent(dragSlots[i].transform);
            items[i].transform.localPosition = Vector2.zero;

            DraggableItem draggable = items[i].GetComponent<DraggableItem>();
            if (draggable != null)
                draggable.enabled = true;
        }
    }

    void FreezeDraggable()
    {
        for (int i = 0; i < items.Count; i++)
        {
            DraggableItem draggable = items[i].GetComponent<DraggableItem>();
            if (draggable != null)
                draggable.enabled = false;
        }
    }

    /// <summary>
    /// Xóa tất cả objects
    /// </summary>
    public void Clear()
    {
        foreach (GameObject slot in dragSlots)
            if (slot != null) DestroyImmediate(slot);

        if (dropSlot != null) DestroyImmediate(dropSlot);

        foreach (GameObject item in items)
            if (item != null) DestroyImmediate(item);

        dragSlots.Clear();
        items.Clear();
        dropSlot = null;
        currentDroppedItem = null;
    }

    private void OnDestroy()
    {
        Clear();
    }
}

/// <summary>
/// Custom drop slot cho Logo minigame
/// </summary>
public class LogoDropSlot : MonoBehaviour, IDropHandler
{
    private LogoDropMinigame minigame;

    public void Initialize(LogoDropMinigame game)
    {
        minigame = game;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var droppedGO = eventData.pointerDrag;
        if (droppedGO == null) return;

        var draggable = droppedGO.GetComponent<DraggableItem>();
        if (draggable == null) return;

        // Snap vào vị trí
        draggable.parentAfterDrag = transform;
        droppedGO.transform.SetParent(transform);
        droppedGO.transform.localPosition = Vector3.zero;

        // Thông báo cho minigame
        if (minigame != null)
            minigame.OnItemDropped(draggable);
    }
}