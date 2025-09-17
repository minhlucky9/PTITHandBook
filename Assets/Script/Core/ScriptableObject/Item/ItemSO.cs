using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interaction
{
    public class ItemSO : ScriptableObject
    {
        public string itemId;
        public GameObject prefabs;
        public Sprite itemImage;
        public string itemName;
        [TextArea(2, 4)]
        public string itemDescription;
        public bool isStackable;
        public int maxStackSize;

        public virtual void UseItem() { }

        private void OnValidate()
        {
#if UNITY_EDITOR
            itemId = this.name;
#endif
        }
    }
}

