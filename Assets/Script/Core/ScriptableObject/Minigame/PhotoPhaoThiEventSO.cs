using UnityEngine;
using Interaction.Minigame;

[CreateAssetMenu(menuName = "Scriptable Objects/Minigame/Photo Phao Thi Event Data")]
public class PhotoPhaoThiEventSO : MinigameDataSO
{
    [Tooltip("Giá vàng để mua phao thi")]
    public int goldCost = 150;

    [Tooltip("Item ID cần thêm khi mua phao thi")]
    public string rewardItemId;

    public override void Init(GameObject targetNPC)
    {
        base.Init(targetNPC);
        // Khởi tạo Photo Phao Thi quest
        PhotoPhaoThiQuestManager.instance.InitPhotoPhaoThiQuest(targetNPC, this);
    }
}
