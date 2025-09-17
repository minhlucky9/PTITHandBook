using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DS.Windows
{
    using DS.windows;
    using Elements;
    using Enumerations;

    /*--------------------------------------------------------------------------------------------------------------------*/

    /*DSSearchWindow là cửa sổ menu để tạo ra các Node và Group*/

    /*ISearchWindowProvider cung cấp 2 phương thức là CreateSearchTree và OnSelectEntry để khởi tạo và quản lí window menu*/

    /*--------------------------------------------------------------------------------------------------------------------*/


    public class DSSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DSGraphView graphView;
        private Texture2D indentationIcon;

        public void Initialize(DSGraphView dsGraphView)
        {
            graphView = dsGraphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }


        /*--------------------------------------------------------------------------------------------------------------------*/

        /*Tác dụng của CreateSearchTree: Populates the search window menu */

        /*SearchTreeGroupEntry đóng vai trò là các thành phần trong menu */

        /*SearchTreeEntry đóng vai trò là các tùy chọn của thành phần trong menu đó */

        /*Biến userData sẽ phục vụ cho phương thức OnSelectEntry để kiểm tra xem tùy chọn nào đã được chọn */

        /*--------------------------------------------------------------------------------------------------------------------*/



        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements")),

                new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1),

                new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
                {
                    userData = DSDialogType.SingleChoice,
                    level = 2
                },

                new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon))
                {
                    userData = DSDialogType.MultipleChoice,
                    level = 2
                },

                new SearchTreeGroupEntry(new GUIContent("Dialogue Groups"), 1),

                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    userData = new Group(),
                    level = 2
                }
            };

            return searchTreeEntries;
        }

        /*--------------------------------------------------------------------------------------------------------------------*/

        /*Tác dụng của OnSelectEntry: What to do when a certain menu is pressed */

        /*localMousePosition có tác dụng giúp cho các Nodes và Group được tạo ở vị trí con trỏ chuột tại thời điểm khỏi tạo */

        /*--------------------------------------------------------------------------------------------------------------------*/

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case DSDialogType.SingleChoice:
                    {
                        DSSingleChoiceNode singleChoiceNode = (DSSingleChoiceNode)graphView.CreateNode("DialogueName", DSDialogType.SingleChoice, localMousePosition);

                        graphView.AddElement(singleChoiceNode);

                        return true;
                    }

                case DSDialogType.MultipleChoice:
                    {
                        DSMultipleChoiceNode multipleChoiceNode = (DSMultipleChoiceNode)graphView.CreateNode("DialogueName", DSDialogType.MultipleChoice, localMousePosition);

                        graphView.AddElement(multipleChoiceNode);

                        return true;
                    }

                case Group _:
                    {
                       graphView.CreateGroup("DialogueGroup", localMousePosition);
                    
                      return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }
    }
}