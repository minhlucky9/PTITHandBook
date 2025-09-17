using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DragAndDropMinigame : MonoBehaviour
{
    public static DragAndDropMinigame instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab; // Prefab cho DragAndDropSlot
    [SerializeField] private GameObject itemPrefab; // Prefab cho DragAndDropItem

    [Header("Containers")]
    [SerializeField] private Transform dragContainer;
    [SerializeField] private Transform dropContainer;
    [SerializeField] public UIAnimationController MiniGame;
    [SerializeField] private Button closeBtn;

    [Header("Settings")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int columns = 4;
    [SerializeField] private float slotSize = 100f;
    [SerializeField] private float spacing = 10f;

    private List<GameObject> dragSlots = new List<GameObject>();
    private List<GameObject> dropSlots = new List<GameObject>();
    private List<GameObject> items = new List<GameObject>();

    private int correctCount = 0;
    [HideInInspector] public int totalPieces = 0;
    private System.Action onComplete;

    private HashSet<string> correctlyPlacedItems = new HashSet<string>(); // Track items đã đặt đúng

    private void Awake()
    {
      instance = this;
       
    }

    /// <summary>
    /// Khởi tạo minigame kéo thả với texture truyền vào
    /// </summary>
    public void Init(Texture2D puzzleTexture, System.Action onCompleteCallback = null, System.Action quitAction = null)
    {
        onComplete = onCompleteCallback;
        closeBtn.onClick.AddListener(delegate
        {
            quitAction?.Invoke();
        });

        Clear();

        totalPieces = rows * columns;
        CreateSlots();
        CreatePuzzlePieces(puzzleTexture);
        ShufflePieces();
        MiniGame.Activate();
    }

    /// <summary>
    /// Tạo các slot cho drag và drop container
    /// </summary>
    private void CreateSlots()
    {
        // Tạo slots cho drag container
        for (int i = 0; i < totalPieces; i++)
        {
            GameObject slot = Instantiate(slotPrefab, dragContainer);
            slot.name = $"DragSlot_{i}";

            InventorySlot slotScript = slot.GetComponent<InventorySlot>();
            if (slotScript == null) slotScript = slot.AddComponent<InventorySlot>();
            slotScript.isTarget = false;
            slotScript.slotID = (i*2).ToString(); // Ô gốc không có ID
            slotScript.gameManager = TraceQuestManager.instance;

            RectTransform rectTransform = slot.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(slotSize, slotSize);

            int row = i / columns;
            int col = i % columns;
            float xPos = col * (slotSize + spacing);
            float yPos = -row * (slotSize + spacing);
            rectTransform.anchoredPosition = new Vector2(xPos, yPos);

            dragSlots.Add(slot);
        }

        // Tạo slots cho drop container
        for (int i = 0; i < totalPieces; i++)
        {
            GameObject slot = Instantiate(slotPrefab, dropContainer);
            slot.name = $"DropSlot_{i}";

            InventorySlot slotScript = slot.GetComponent<InventorySlot>();
            if (slotScript == null) slotScript = slot.AddComponent<InventorySlot>();
            slotScript.isTarget = true;
            slotScript.slotID = i.ToString(); // Set ID cho ô đích
            slotScript.gameManager = TraceQuestManager.instance;

            RectTransform rectTransform = slot.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(slotSize, slotSize);

            int row = i / columns;
            int col = i % columns;
            float xPos = col * (slotSize + spacing);
            float yPos = -row * (slotSize + spacing);
            rectTransform.anchoredPosition = new Vector2(xPos, yPos);

            dropSlots.Add(slot);
        }
    }

    /// <summary>
    /// Tạo các mảnh ghép từ texture
    /// </summary>
    private void CreatePuzzlePieces(Texture2D puzzleTexture)
    {
        if (puzzleTexture == null)
        {
            Debug.LogWarning("Puzzle texture is null, creating colored pieces instead");
            CreateColoredPieces();
            return;
        }

        int pieceWidth = puzzleTexture.width / columns;
        int pieceHeight = puzzleTexture.height / rows;

        for (int i = 0; i < totalPieces; i++)
        {
            int row = i / columns;
            int col = i % columns;

            // Tạo item
            GameObject item = Instantiate(itemPrefab, dragSlots[i].transform);
            item.name = $"DragAndDropItem_{i}";

            // Setup DraggableItem
            DraggableItem draggable = item.GetComponent<DraggableItem>();
            if (draggable == null) draggable = item.AddComponent<DraggableItem>();
            draggable.itemID = i.ToString();

            // Setup Image
            Image image = item.GetComponent<Image>();
            if (image == null) image = item.AddComponent<Image>();
            draggable.image = image;

            // Cắt sprite từ texture
            Rect spriteRect = new Rect(
                col * pieceWidth,
                (rows - 1 - row) * pieceHeight, // Flip Y
                pieceWidth,
                pieceHeight
            );

            Sprite pieceSprite = Sprite.Create(puzzleTexture, spriteRect, new Vector2(0.5f, 0.5f));
            image.sprite = pieceSprite;
            image.color = Color.white;

            // Setup RectTransform
            RectTransform rectTransform = item.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(slotSize - 5, slotSize - 5); // Nhỏ hơn slot một chút
            rectTransform.anchoredPosition = Vector2.zero;

            items.Add(item);
        }
    }

    /// <summary>
    /// Tạo các mảnh ghép có màu sắc (fallback khi không có texture)
    /// </summary>
    private void CreateColoredPieces()
    {
        for (int i = 0; i < totalPieces; i++)
        {
            GameObject item = Instantiate(itemPrefab, dragSlots[i].transform);
            item.name = $"DragAndDropItem_{i}";

            // Setup DraggableItem
            DraggableItem draggable = item.GetComponent<DraggableItem>();
            if (draggable == null) draggable = item.AddComponent<DraggableItem>();
            draggable.itemID = i.ToString();

            // Setup Image với màu
            Image image = item.GetComponent<Image>();
            if (image == null) image = item.AddComponent<Image>();
            draggable.image = image;

            float hue = (i * 0.618f) % 1f;
            image.color = Color.HSVToRGB(hue, 0.6f, 0.9f);

            // Thêm text hiển thị số
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(item.transform, false);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = (i + 1).ToString();
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Setup RectTransform
            RectTransform rectTransform = item.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(slotSize - 5, slotSize - 5);
            rectTransform.anchoredPosition = Vector2.zero;

            items.Add(item);
        }
    }

    /// <summary>
    /// Xáo trộn vị trí các mảnh ghép
    /// </summary>
    private void ShufflePieces()
    {
        List<GameObject> tempItems = new List<GameObject>(items);

        for (int i = 0; i < dragSlots.Count; i++)
        {
            int randomIndex = Random.Range(0, tempItems.Count);
            GameObject item = tempItems[randomIndex];
            tempItems.RemoveAt(randomIndex);

            item.transform.SetParent(dragSlots[i].transform);
            item.transform.localPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// Được gọi khi một item được thả đúng vị trí
    /// </summary>
    public void OnCorrectPlacement(string itemID)
    {
        // Chỉ đếm nếu item chưa được track là đã đặt đúng
        if (!correctlyPlacedItems.Contains(itemID))
        {
            correctlyPlacedItems.Add(itemID);
            correctCount = correctlyPlacedItems.Count;

            Debug.Log($"Correct placement: {itemID}, Total: {correctCount}/{totalPieces}");

            if (correctCount >= totalPieces)
            {
                Debug.Log("Drag and Drop Minigame Completed!");
                onComplete?.Invoke();
            }
        }
    }


    /// <summary>
    /// Được gọi khi một item bị di chuyển khỏi vị trí đúng
    /// </summary>
    public void OnItemRemovedFromCorrectSlot(string itemID)
    {
        if (correctlyPlacedItems.Contains(itemID))
        {
            correctlyPlacedItems.Remove(itemID);
            correctCount = correctlyPlacedItems.Count;
            Debug.Log($"Item removed: {itemID}, Total: {correctCount}/{totalPieces}");
        }
    }

    /// <summary>
    /// Reset minigame
    /// </summary>
    public void Reset()
    {
        correctCount = 0;
        correctlyPlacedItems.Clear(); // Clear tracking list

        // Đưa tất cả items về drag container
        for (int i = 0; i < items.Count && i < dragSlots.Count; i++)
        {
            items[i].transform.SetParent(dragSlots[i].transform);
            items[i].transform.localPosition = Vector2.zero;

            DraggableItem draggable = items[i].GetComponent<DraggableItem>();
            if (draggable != null)
                draggable.enabled = true;
        }

        ShufflePieces();
    }

    /// <summary>
    /// Xóa tất cả objects đã tạo
    /// </summary>
    public void Clear()
    {
        foreach (GameObject slot in dragSlots)
            if (slot != null) DestroyImmediate(slot);

        foreach (GameObject slot in dropSlots)
            if (slot != null) DestroyImmediate(slot);

        foreach (GameObject item in items)
            if (item != null) DestroyImmediate(item);

        dragSlots.Clear();
        dropSlots.Clear();
        items.Clear();
        correctCount = 0;
        correctlyPlacedItems.Clear();
    }

    private void OnDestroy()
    {
        Clear();
    }
}