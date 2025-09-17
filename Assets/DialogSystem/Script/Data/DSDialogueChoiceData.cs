using System;
using UnityEngine;

namespace DS.Data
{
    using Interaction;
    using ScriptableObjects;

    [Serializable]
    public class DSDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DSDialogueSO NextDialogue { get; set; }

        [field: SerializeField] public DialogExecuteFunction ExecutedFunction { get; set; }

        [field: SerializeField] public DialogExecuteFunction NextExecutedFunction { get; set; }

        [field: SerializeField] public Sprite Icon { get; set; }
    }
}