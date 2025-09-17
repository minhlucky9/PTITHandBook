using Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDialogueHandler 
{
    NPCInfoSO NpcInfo { get; }
    DialogConservation PausedDialog { get; set; }
    bool IsConversationPaused { get; set; }
}
