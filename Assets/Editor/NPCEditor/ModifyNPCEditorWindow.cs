using Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace NPCQuestSystem.Editor
{
    public class ModifyNPCEditorWindow : EditorWindow
    {
        public static NPCInfoSO modifyNPC;
        public static void ShowWindow(NPCInfoSO npcInfo)
        {
            modifyNPC = npcInfo;
            // Create and show a new editor window
            GetWindow<ModifyNPCEditorWindow>("Modify NPC Information");
        }

        private void OnEnable()
        {
            EditorUtility.SetDirty(modifyNPC);
            this.minSize = new Vector2(300, 200);
            this.maxSize = new Vector2(300, 200);
        }

        private void OnDisable()
        {
            EditorUtility.ClearDirty(modifyNPC);
        }

        private void OnGUI()
        {
            GUILayout.Label("Update NPC Information", EditorStyles.boldLabel);

            modifyNPC.npcName = EditorGUILayout.TextField("Name", modifyNPC.npcName);
            modifyNPC.npcRole = EditorGUILayout.TextField("Role", modifyNPC.npcRole);
            EditorGUILayout.Separator();

            if (GUILayout.Button("Submit", GUILayout.Height(30)))
            {
                Close(); // Closes the current window
            }
        }
    }
}

#endif