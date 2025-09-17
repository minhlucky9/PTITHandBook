
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interaction.Minigame
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Minigame/Loot Event Data")]
    public class LootEventSO : MinigameDataSO
    {
        public List<LootObject> lootObjects;
        public int numberOfLoot;
        public override void Init(GameObject targetGameObject)
        {
            base.Init(targetGameObject);
            //
            GameObject parent = new GameObject();
            for(int i = 0; i < lootObjects.Count; i++)
            {
                Instantiate(lootObjects[i].prefabs, parent.transform);   
            }

            CollectQuestManager.instance.InitCollectQuest(targetGameObject, parent, this);
        }
    }

    [Serializable]
    public class LootObject
    {
        public GameObject prefabs;
    }
}
