using Interaction;
using System;
using UnityEngine;

namespace DS.Data.Save
{
    [Serializable]
    public class DSChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }

        [field: SerializeField] public DialogExecuteFunction ExecutedFunction { get; set; }

        [field: SerializeField] public DialogExecuteFunction NextExecutedFunction { get; set; }

        [field: SerializeField] public Sprite Icon { get; set; }
    }
}