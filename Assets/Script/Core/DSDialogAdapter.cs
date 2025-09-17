using UnityEngine;
using DS.ScriptableObjects; // For DSDialogueSO, DSDialogueContainerSO
using System.Collections.Generic;
using System.Linq;
using Interaction; // For DialogConservation, DialogResponse, etc.

public class DSDialogueAdapter : MonoBehaviour
{
   

    // map nodeName -> DSDialogueSO (for quick lookup)
    private Dictionary<string, DSDialogueSO> nodeLookup = new Dictionary<string, DSDialogueSO>();

    public void Initialize(DSDialogueContainerSO container)
    {
        nodeLookup.Clear();
 //       Debug.Log($"Initializing dialogue container with {container.DialogueGroups.Count} groups and {container.UngroupedDialogues.Count} ungrouped dialogues");

        // load group dialogues
        foreach (var kv in container.DialogueGroups)
        {
  //          Debug.Log($"Processing group: {kv.Key.name} with {kv.Value.Count} dialogues");
            foreach (var d in kv.Value)
            {
                nodeLookup[d.DialogueName] = d;
  //              Debug.Log($"Added grouped dialogue: '{d.DialogueName}' with {d.Choices?.Count ?? 0} choices");
                if (d.Choices != null)
                {
                    foreach (var choice in d.Choices)
                    {
     //                   Debug.Log($"  Choice: '{choice.Text}' -> Next: {(choice.NextDialogue != null ? choice.NextDialogue.DialogueName : "null")}");
                        if (choice.NextDialogue != null)
                        {
    //                        Debug.Log($"    Next dialogue exists: {choice.NextDialogue.DialogueName}");
                        }
                    }
                }
            }
        }
        // load ungrouped dialogues
        foreach (var d in container.UngroupedDialogues)
        {
            nodeLookup[d.DialogueName] = d;
     //       Debug.Log($"Added ungrouped dialogue: '{d.DialogueName}' with {d.Choices?.Count ?? 0} choices");
            if (d.Choices != null)
            {
                foreach (var choice in d.Choices)
                {
      //              Debug.Log($"  Choice: '{choice.Text}' -> Next: {(choice.NextDialogue != null ? choice.NextDialogue.DialogueName : "null")}");
                    if (choice.NextDialogue != null)
                    {
      //                  Debug.Log($"    Next dialogue exists: {choice.NextDialogue.DialogueName}");
                    }
                }
            }
        }

        // Verify all next dialogues exist
    //    Debug.Log("Verifying next dialogue connections...");
        foreach (var node in nodeLookup.Values)
        {
            if (node.Choices != null)
            {
                foreach (var choice in node.Choices)
                {
                    if (choice.NextDialogue != null && !nodeLookup.ContainsKey(choice.NextDialogue.DialogueName))
                    {
    //                    Debug.LogError($"Invalid connection: Node '{node.DialogueName}' choice '{choice.Text}' points to non-existent node '{choice.NextDialogue.DialogueName}'");
                    }
                }
            }
        }
    }

    // Convert 1 DSDialogueSO -> 1 DialogConservation
    public DialogConservation ConvertDSDialogueToConservation(DSDialogueSO dsDialogue)
    {
        if (dsDialogue == null) return null;

    //    Debug.Log($"Converting dialogue: '{dsDialogue.DialogueName}' to conservation");
        DialogConservation cons = new DialogConservation();
        cons.dialogId = dsDialogue.DialogueName;
        cons.message = dsDialogue.Text;
        cons.possibleResponses = new List<DialogResponse>();

        if (dsDialogue.Choices != null)
        {
      //      Debug.Log($"Processing {dsDialogue.Choices.Count} choices for dialogue '{dsDialogue.DialogueName}'");
            foreach (var choice in dsDialogue.Choices)
            {
                var resp = new DialogResponse();
                resp.message = choice.Text;
                resp.icon = choice.Icon;
                resp.executedFunction = dsDialogue.ExecutedFunction;
                resp.NextExecutedFunction = dsDialogue.NextExecutedFunction;
                
                // Log the choice and its next dialogue before setting nextDialogId
     //           Debug.Log($"Processing choice: '{choice.Text}'");
     //           Debug.Log($"  NextDialogue is {(choice.NextDialogue != null ? "not null" : "null")}");
                if (choice.NextDialogue != null)
                {
       //             Debug.Log($"  NextDialogue name: '{choice.NextDialogue.DialogueName}'");
      //              Debug.Log($"  NextDialogue exists in lookup: {nodeLookup.ContainsKey(choice.NextDialogue.DialogueName)}");
                }

                // nextDialogId = NextDialogue.DialogueName if not null
                resp.nextDialogId = choice.NextDialogue ? choice.NextDialogue.DialogueName : "";
        //        Debug.Log($"  Set nextDialogId to: '{resp.nextDialogId}'");
                
                cons.possibleResponses.Add(resp);
            }
        }
        return cons;
    }

    // Lấy DSDialogueSO theo tên
    public DSDialogueSO GetNodeByName(string nodeName)
    {
 //       Debug.Log($"Looking for node with name: '{nodeName}'");
//        Debug.Log($"Current nodeLookup count: {nodeLookup.Count}");
        
        if (string.IsNullOrEmpty(nodeName))
        {
 //           Debug.LogError("Attempted to look up node with null or empty name");
            return null;
        }

        if (nodeLookup.TryGetValue(nodeName, out var node))
        {
     //       Debug.Log($"Found node: '{node.DialogueName}'");
    //        Debug.Log($"Node details - Choices count: {node.Choices?.Count ?? 0}");
            if (node.Choices != null)
            {
                foreach (var choice in node.Choices)
                {
      //              Debug.Log($"  Choice: '{choice.Text}' -> Next: {(choice.NextDialogue != null ? choice.NextDialogue.DialogueName : "null")}");
                }
            }
            return node;
        }
        else
        {
    //        Debug.LogError($"Node '{nodeName}' not found in lookup dictionary");
    //        Debug.Log("Available nodes in lookup:");
            foreach (var name in nodeLookup.Keys)
            {
    //            Debug.Log($"- '{name}'");
            }
            return null;
        }
    }

    // Debug methods
    public int GetNodeLookupCount()
    {
        return nodeLookup.Count;
    }

    public void LogNodeNames()
    {
   //     Debug.Log("Available node names:");
        foreach (var name in nodeLookup.Keys)
        {
    //        Debug.Log($"- '{name}'");
        }
    }
}
