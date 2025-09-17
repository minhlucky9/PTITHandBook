using Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace NPCQuestSystem.Editor
{
    public class NPCQuestEditor : EditorWindow
    {
        QuestInfoSO[] allQuests;
        NPCInfoSO[] allNPCs;

        private int selectedNPC = 0;
        private string npcName = "";
        private string npcRole = "";

        [MenuItem("Tools/NPC Quest Editor")]
        public static void ShowWindow()
        {
            GetWindow<NPCQuestEditor>("NPC Quest Editor");
        }

        private void OnEnable()
        {
            // loads all QuestInfo Scriptable Objects under the Assets/Resources/Quests folder
            allQuests = Resources.LoadAll<QuestInfoSO>("Quests");
            allNPCs = Resources.LoadAll<NPCInfoSO>("NPC");
        }

        private void OnDisable()
        {
        }

        private void OnGUI()
        {
            string[] arr = new string[3] { "Option1", "Option2", "Option3" };
            GUILayout.Label("Create NPC data", EditorStyles.boldLabel);
            // Create a dropdown button
            selectedNPC = EditorGUILayout.Popup("Select a NPC", selectedNPC, GetDropDownNPC(allNPCs));
            //
            EditorGUILayout.Separator();
            GUILayout.Label("NPC information", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Name", allNPCs[selectedNPC].npcName);
            EditorGUILayout.LabelField("Role", allNPCs[selectedNPC].npcRole);
            EditorGUILayout.LabelField("Normal Conservation", allNPCs[selectedNPC].normalConservation.name);
            //
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create a new NPC", GUILayout.Width(200)))
            {
                Debug.Log("Create Clicked");
                ConservationEditor.ShowEditor(allNPCs[selectedNPC].normalConservation);
            }
            if (GUILayout.Button("Modify", GUILayout.Width(100)))
            {
                Debug.Log("Modify Clicked");
                ModifyNPCEditorWindow.ShowWindow(allNPCs[selectedNPC]);
            }
            EditorGUILayout.EndHorizontal();
        }

        private string[] GetDropDownNPC(NPCInfoSO[] npcs)
        {
            string[] res = new string[npcs.Length];
            for (int i = 0; i < npcs.Length; i++)
            {
                res[i] = npcs[i].npcId;
            }
            return res;
        }
    }
}

#endif