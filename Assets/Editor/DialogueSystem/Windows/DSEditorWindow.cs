using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace DS.windows
{
    using DS.Utilities;
    using System;
    using Ultilities;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor.UIElements;

    #region Vì sao các phương thức trong DSGraphView được sử dụng một cách liên tục 

    /*--------------------------------------------------------------------------------------------------------------------*/

    /*  rootVisualElement là gốc của hệ thống UI trong EditorWindow, đóng vai trò như một container chứa tất cả các thành phần UI. */

    /*  graphView (là một instance của DSGraphView) được thêm vào rootVisualElement, khiến nó trở thành một phần của giao diện và duy trì trạng thái trong suốt vòng đời của cửa sổ.

       Điều đó giúp chúng ta sử dụng các phương thức trong DSGraphView một cách liên tục */

    /*--------------------------------------------------------------------------------------------------------------------*/

    #endregion

    #region Vì sao DSGraphView vẫn tồn tại?

    /*--------------------------------------------------------------------------------------------------------------------*/

    /*  🔥 Vì sao DSGraphView vẫn tồn tại? */

    /*  rootVisualElement là một phần của Unity UI System, hoạt động độc lập với vòng đời của script. */

    /*  Khi cửa sổ vẫn mở, rootVisualElement giữ nguyên các phần tử bên trong nó, nghĩa là DSGraphView vẫn tồn tại và có thể phản hồi sự kiện. */

    /*  Chỉ khi đóng cửa sổ, Unity reload script, hoặc thay đổi trạng thái Play, OnEnable() mới được gọi lại và DSGraphView mới được tạo lại. */

    /*--------------------------------------------------------------------------------------------------------------------*/

    #endregion

    public class DSEditorWindow : EditorWindow
    {
        private DSGraphView graphView;

        private readonly string defaultFileName = "DialogFileName";

        private static TextField filenameTextField;

        private Button saveButton;

        private Button miniMapButton;

        [SerializeField]

        StyleSheet style;

        [MenuItem("Window/DS/Dialog Graph")]

        public static void ShowExample()
        {
             GetWindow<DSEditorWindow>("Dialog Graph");
       
        }

      private void OnEnable()
        {
            AddGraphView();

            AddToolBar();

            AddStyles();
        }

        #region Các phương thức Add

        private void AddGraphView()
        {
            graphView = new DSGraphView(this);

            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }

        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();

            /*Nếu muốn có WhiteSpaces cho ToolBars thì chỉ cần bỏ callback đi là xong*/
            filenameTextField = DSElementsUltilities.CreateTextField(defaultFileName, "File name:", callback =>
            {
                filenameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();    
            });


            saveButton = DSElementsUltilities.CreateButton("Save", () => Save());

            Button loadButton = DSElementsUltilities.CreateButton("Load", () => Load());

            Button reloadButton = DSElementsUltilities.CreateButton("Reload", () => Reload());

            Button clearButton = DSElementsUltilities.CreateButton("Clear", () => Clear());

            Button resetButton = DSElementsUltilities.CreateButton("Reset", () => ResetGraph());

            miniMapButton = DSElementsUltilities.CreateButton("Minimap", () => ToggleMiniMap());

            toolbar.Add(filenameTextField);

            toolbar.Add(saveButton);

            toolbar.Add(clearButton);

            toolbar.Add(loadButton);

            toolbar.Add(reloadButton);

            toolbar.Add(resetButton);

            toolbar.Add(miniMapButton);

            toolbar.AddStyleSheets("DialogSystem/DSToolBarStyle.uss");

            rootVisualElement.Add(toolbar);
        }

      

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogSystem/DSVariables.uss");
           

        }

        #endregion

        #region Các phương thức Ultility

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }

        #endregion


        private void Save()
        {
            if (string.IsNullOrEmpty(filenameTextField.value))
            {
                EditorUtility.DisplayDialog("Invalid file name.", "Please ensure the file name you've typed in is valid.", "Roger!");
                return;
            }

            DSIOUtility.Initialize(graphView, filenameTextField.value);
            DSIOUtility.Save();
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        private void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Editor/DialogueSystem/Graphs", "asset");

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            Clear();

            DSIOUtility.Initialize(graphView, Path.GetFileNameWithoutExtension(filePath));
            DSIOUtility.Load();
        }

        private void ResetGraph()
        {
            Clear();

            UpdateFileName(defaultFileName);
        }

        public static void UpdateFileName(string newFileName)
        {
            filenameTextField.value = newFileName;
        }

        private void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();

            miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
        }

      
        private void Reload()
        {
           string filename = filenameTextField.value?.Trim() + "Graph";

            if (string.IsNullOrEmpty(filename))
            {
                
                return;
            }

         
            Clear();

            DSIOUtility.Initialize(graphView, filename);

            DSIOUtility.Load();
        }


    }
}

