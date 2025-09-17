using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Data.Save
{
    using Enumerations;
    using Interaction;

    [Serializable]
    public class DSNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<DSChoiceSaveData> Choices { get; set; }

        [field: SerializeField] public Sprite Icon { get; set; }

        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public DSDialogType DialogueType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }

        [field: SerializeField] public DialogExecuteFunction ExecutedFunction { get; set; }

        [field: SerializeField] public DialogExecuteFunction NextExecutedFunction { get; set; }
    }
}