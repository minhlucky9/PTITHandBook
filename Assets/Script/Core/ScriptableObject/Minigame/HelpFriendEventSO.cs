using System;
using UnityEngine;

namespace Interaction.Minigame
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Minigame/Help Friend Event Data")]
    public class HelpFriendEventSO : MinigameDataSO
    {
        [Tooltip("Item ID cần có trong inventory để hoàn thành quest")]
        public string requiredItemId;

        public override void Init(GameObject targetNPC)
        {
            base.Init(targetNPC);
            // Khởi tạo quest
            HelpFriendQuestManager.instance.InitHelpFriendQuest(targetNPC, this);
        }
    }
}
