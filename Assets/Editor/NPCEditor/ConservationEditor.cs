
﻿using Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public class ConservationEditor : NodeEditor
{
    public static NPCConservationSO modifyConservation;

    bool creatingAnswer = false;
    Rect temporaryAnswer;
    bool blocking = false;






    private void OnEnable()
    {
        EditorUtility.SetDirty(modifyConservation);
    }

    private void OnDisable()
    {
        if (modifyConservation != null)
        {
            EditorUtility.SetDirty(modifyConservation);
            AssetDatabase.SaveAssets(); // Lưu dữ liệu vào file
        }
        EditorUtility.ClearDirty(modifyConservation);
    }


    public static void ShowEditor(NPCConservationSO conservation)
    {
        modifyConservation = conservation;
        ConservationEditor editor = GetWindow<ConservationEditor>(conservation.name);
        editor.Init();
    }

    public override void Init()
    {
        base.Init();
        SetupNodeData();
    }

    private void SetupNodeData()
    {
        nodes.Clear();
        for (int i = 0; i < modifyConservation.dialogs.Count; i++)
        {
            Vector2 nodePos = modifyConservation.dialogs[i].nodePosition;
            nodes.Add(new Rect(nodePos.x, nodePos.y, 200, 100));
        }
        names = GetDialogIds();
        blocking = false;
    }

    public override void OnGUI()
    {
        if (blocking) return;

        base.OnGUI();
        //
        switch (Event.current.keyCode)
        {
            case KeyCode.Delete:
                // ...
                DeleteDialog(selectedNode);
                return;
            default:
                break;
        }
    }

    public override void CreateMenu(ref GenericMenu menu, Vector2 mousePosition)
    {
        menu.AddItem(new GUIContent("Dialog"), false, () => OnCreateDialog(mousePosition));
        menu.AddItem(new GUIContent("Answer"), false, () => OnCreateAnswer(mousePosition));
    }

    private void OnCreateAnswer(Vector2 mousePosition)
    {
        creatingAnswer = true;
        temporaryAnswer = new Rect(mousePosition.x, mousePosition.y, 200, 100);
        //enable line
        isConnect = true;
        selectedRect = temporaryAnswer;

    }

    private void OnCreateDialog(Vector2 mousePosition)
    {
        Interaction.DialogConservation dialog = new Interaction.DialogConservation();

        dialog.dialogId = GetNotCreatedId();
        dialog.nodePosition = mousePosition;
        //
        modifyConservation.dialogs.Add(dialog);
        //
        SetupNodeData();
    }

    public override void DrawNodes()
    {
        base.DrawNodes();
        //
        if (creatingAnswer)
        {
            temporaryAnswer = GUILayout.Window(nodes.Count + 1, temporaryAnswer, DrawAnswer, "Answer");
        }

    }

    void DrawAnswer(int id)
    {
        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseDown)
        {
            HideTemporaryAnswer();
        }
        //
        EditorGUILayout.Separator();
        EditorStyles.label.wordWrap = true;
        EditorGUILayout.LabelField(" ");
        EditorStyles.label.wordWrap = false;
        //
        GUI.DragWindow();
    }

    void HideTemporaryAnswer()
    {
        creatingAnswer = false;
        isConnect = false;
        Repaint();
    }

    public override void DrawNodeContent(int i)
    {
        if (i >= modifyConservation.dialogs.Count) return;

        EditorGUILayout.Separator();

        // GUIStyle cho TextArea
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true; // Cho phép tự động xuống dòng

        // Lấy nội dung hiện tại của dialog
        string oldMessage = modifyConservation.dialogs[i].message;

        // Tính chiều rộng tối đa hợp lý (giới hạn tối đa 400px)
        float maxWidth = 800;
        float minWidth = 400;
        float contentWidth = Mathf.Clamp(textAreaStyle.CalcSize(new GUIContent(oldMessage)).x + 20, minWidth, maxWidth);

        // Tính chiều cao dựa trên nội dung với chiều rộng tính toán được
        float textHeight = textAreaStyle.CalcHeight(new GUIContent(oldMessage), contentWidth);

        // Hiển thị TextArea với kích thước động
        string newMessage = EditorGUILayout.TextArea(oldMessage, textAreaStyle,
            GUILayout.Width(contentWidth), GUILayout.Height(textHeight));

        // Nếu nội dung thay đổi, cập nhật dữ liệu
        if (newMessage != oldMessage)
        {
            modifyConservation.dialogs[i].message = newMessage;
            EditorUtility.SetDirty(modifyConservation);
        }

        // Cập nhật kích thước node dựa trên nội dung
        nodes[i] = new Rect(nodes[i].x, nodes[i].y, contentWidth, Mathf.Max(textHeight + 20, 40));
    }




    public override void OnDragNode(int i)
    {
        if (i >= modifyConservation.dialogs.Count) return;

        base.OnDragNode(i);
        modifyConservation.dialogs[i].nodePosition = new Vector2(nodes[i].x, nodes[i].y);
    }

    public override void OnMouseDownNode(int id)
    {
        base.OnMouseDownNode(id);
        HideTemporaryAnswer();
    }

    public override void OnMouseDownMenu()
    {
        base.OnMouseDownMenu();
        HideTemporaryAnswer();
    }

    private string[] GetDialogIds()
    {
        string[] ids = new string[modifyConservation.dialogs.Count];
        for (int i = 0; i < modifyConservation.dialogs.Count; i++)
        {
            ids[i] = modifyConservation.dialogs[i].dialogId;
        }

        return ids;
    }

    public string GetNotCreatedId()
    {
        int id = 0;
        while (true)
        {
            bool exist = false;
            for (int i = 0; i < modifyConservation.dialogs.Count; i++)
            {
                if (modifyConservation.dialogs[i].dialogId.Equals("dialog" + (id + 1)))
                {
                    id++;
                    exist = true;
                    break;
                }
            }

            if (!exist)
            {
                break;
            }
        }

        return "dialog" + (id + 1);

    }

    private void DeleteDialog(int id)
    {
        blocking = true;

        if (id >= 0 && id < modifyConservation.dialogs.Count)
        {
            selectedNode = -1;
            modifyConservation.dialogs.RemoveAt(id);
        }
        SetupNodeData();
        Repaint();
    }





}

