using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [Tooltip(
      "– Để trống nếu đây là ô gốc (DragBackGround)\n" +
      "– Gán ID (vd: \"A\", \"B\"…) nếu đây là ô đích (DropBackGround)"
    )]
    public string slotID = "";
    public bool isTarget = false;
    [HideInInspector] public TraceQuestManager gameManager;

    public void OnDrop(PointerEventData eventData)
    {
        var droppedGO = eventData.pointerDrag;
        if (droppedGO == null) return;

        var draggable = droppedGO.GetComponent<DraggableItem>();
        if (draggable == null) return;

        // --- 1) Nếu là ô gốc (slotID trống) chỉ snap qua để người chơi có thể trả về ---
        if (isTarget == false)
        {
            draggable.parentAfterDrag = transform;
            DragAndDropMinigame.instance.OnItemRemovedFromCorrectSlot(draggable.itemID);
            return;
        }

        // --- 2) Nếu đã có item rồi, ignore ---
        if (transform.childCount > 0)
        {
            draggable.parentAfterDrag = draggable.originalParent;
            return;
        }
           

        // --- 3) Đây là ô đích: kiểm tra đúng/sai ---
        if (draggable.itemID == slotID)
        {
            // --- 3.1) Snap ngay lập tức vào ô đích ---
            draggable.parentAfterDrag = transform;
            droppedGO.transform.SetParent(transform);
            droppedGO.transform.localPosition = Vector3.zero;
            droppedGO.transform.localRotation = Quaternion.identity;

            // --- 3.2) Khóa không cho kéo nữa (nếu muốn) ---
           // draggable.enabled = false;

            // --- 3.3) Thông báo đúng về GameManager ---
            gameManager.OnCorrectDrop(draggable);
        }
        else
        {
            // --- 4) Thả sai: snap vào ô này, nhưng không khóa, người chơi có thể kéo tiếp ---
            draggable.parentAfterDrag = transform;
            gameManager.OnWrongDrop(draggable);
        }
    }
}
