using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interaction
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Quest/Quest Info")]
    public class QuestInfoSO : ScriptableObject
    {
        [field: SerializeField] public string id { get; private set; }

        [Header("General")]
        public string displayName;

        [Header("Requirements")]
        public int levelRequirement;
        public QuestInfoSO[] questPrerequisites;
       

        [Header("Steps")]
        public List<QuestStep> questSteps;

        [Header("Rewards")]
        public int goldReward;
        public int experienceReward;
        public int bonusMedal = 0;
        public List<ItemReward> itemRewards;

        [Header("UI")]
        public Sprite questIcon;              // Ảnh minh họa quest
        public bool isAdvancedQuest = false;
        public enum QuestType
        {
            Game,
            Quiz,
            Collect,
            HelpFriend,
            Passive    // đôi lúc có những quest không cần StopInteract()
            
        }

        // Trong QuestInfoSO:
        public QuestType questType;

        // ensure the id is always the name of the Scriptable Object asset
        private void OnValidate()
        {
#if UNITY_EDITOR
            id = this.name;
#endif
        }
    }
    [System.Serializable]
    public class QuestStep
    {
        public string stepId;
        public StepMissionType missionType;
        [TextArea(2,4)]
        public string stepName;
        [TextArea(4,8)]
        public string stepDescription;
        public string npcId;
        public NPCState npcStatus;
        public NPCConservationSO conservation;
        public string nextStep;
    }

    [System.Serializable]
    public class ItemReward
    {
        public ItemSO item;
        public int quantity;
    }

    public enum StepMissionType
    {
        TALKING,
        MINIGAME
    }
}


