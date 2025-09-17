using System.Collections.Generic;
using UnityEngine;

namespace DS.ScriptableObjects
{
    using Data;
    using Enumerations;
    using Interaction;

    public class DSDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField][field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<DSDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public DSDialogType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }
        [field: SerializeField] public Sprite Icon { get; set; }

        [field: SerializeField] public DialogExecuteFunction ExecutedFunction { get; set; }
        [field: SerializeField] public DialogExecuteFunction NextExecutedFunction { get; set; }

        public void Initialize(string dialogueName, string text, List<DSDialogueChoiceData> choices,Sprite icon, DSDialogType dialogueType, bool isStartingDialogue, DialogExecuteFunction executedFunction, DialogExecuteFunction NextexecutedFunction)
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            Icon = icon; 
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
            NextExecutedFunction = NextexecutedFunction;
            ExecutedFunction = executedFunction;
        }
    }
}