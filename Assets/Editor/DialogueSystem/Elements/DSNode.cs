using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DS.Elements
{
    using Data.Save;
    using Enumerations;
    using UnityEngine.UIElements;
    using Ultilities;
    using DS.windows;
    using System;
    using System.Linq;
    using Interaction;

    public class DSNode : Node
    {
        public string ID { get; set; }  

        public string DialogName { get; set; }

        public List<DSChoiceSaveData> Choices { get; set; }

        public string Text { get; set; }

        public Sprite Icon { get; set; }

        public DSDialogType DialogueType { get; set; }

        public DialogExecuteFunction ExecutedFunction { get; set; } = DialogExecuteFunction.None;

        public DialogExecuteFunction NextExecutedFunction { get; set; } = DialogExecuteFunction.None;

        public DSGroup Group { get; set; }

        protected DSGraphView graphView;

        private Color defaultBackgroundColor;

        public virtual void Initialize(string nodeName, DSGraphView dSGraphView,Vector2 position)
        {
            ID = Guid.NewGuid().ToString();

            DialogName = nodeName;

            Choices = new List<DSChoiceSaveData>();

            Text = "Dialogue text.";

            graphView = dSGraphView;

            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            SetPosition(new Rect(position,Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");

            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        #region Override

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Port", actionEvent => DisconnectInputPort());

            evt.menu.AppendAction("Disconnect Output Port", actionEvent => DisconnectOutputPort());

            base.BuildContextualMenu(evt);
        }

        #endregion

        public virtual void Draw()
        {
            /* TITLE CONTAINER */

            /*--------------------------------------------------------------------------------------------------------------------------------------------*/

            /* Callback trong dialogNameTexField được gọi mỗi khi giá trị của TextField thay đổi.*/

            /*--------------------------------------------------------------------------------------------------------------------------------------------*/

            TextField dialogNameTexField = DSElementsUltilities.CreateTextField(DialogName, null, callback =>
            {
                TextField target = (TextField) callback.target;

                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogName))
                    {
                        ++graphView.NameErrorAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogName))
                    {
                        --graphView.NameErrorAmount;
                    }
                }

                if (Group == null)
                {
                    graphView.RemoveUnGroupNode(this);

                    /*Nếu muốn có WhiteSpaces cho Nodes thì thay DialogName bằng callback.newValue*/
                    DialogName = target.value;

                    graphView.AddUnGroupNode(this);

                    return;
                } 
                
                DSGroup currentGroup = Group;

                graphView.RemoveGroupedNode(this, Group);

                DialogName = callback.newValue;

                graphView.AddGroupedNode(this, currentGroup);

            });

            dialogNameTexField.AddClasses(
                "ds-node__textfield",
                "ds-node__filename-textfield",
                "ds-node__textfield__hidden"
                );

            titleContainer.Insert(0, dialogNameTexField);

            /* INPUT CONTAINER */

            Port inputPort = this.CreatePort("Dialog Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);

            /* EXTENSION CONTAINER */

            VisualElement customdatacontainer = new VisualElement();

            customdatacontainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = DSElementsUltilities.CreateFoldout("Dialogue Text");

            TextField textTextField = DSElementsUltilities.CreateTextArea(Text, null, callback => Text = callback.newValue);

            textTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__qute-textfield"
                );
        
            textFoldout.Add(textTextField);

            customdatacontainer.Add(textFoldout);

            extensionContainer.Add(customdatacontainer);

            EnumField executedFunctionField = new EnumField("Node Function", ExecutedFunction);
            executedFunctionField.Init(ExecutedFunction);
            executedFunctionField.RegisterValueChangedCallback(evt =>
            {
                ExecutedFunction = (DialogExecuteFunction)evt.newValue;
            });

            executedFunctionField.style.marginTop = 4;
            executedFunctionField.style.minWidth = 150;

            EnumField NextExecutedFunctionField = new EnumField("Next Node Function", NextExecutedFunction);
            NextExecutedFunctionField.Init(NextExecutedFunction);
            NextExecutedFunctionField.RegisterValueChangedCallback(evt =>
            {
                NextExecutedFunction = (DialogExecuteFunction)evt.newValue;
            });

            NextExecutedFunctionField.style.marginTop = 4;
            NextExecutedFunctionField.style.minWidth = 150;

            // Gắn vào container phù hợp
            extensionContainer.Add(executedFunctionField);
            extensionContainer.Add(NextExecutedFunctionField);


        }

        public void DisconnectAllPorts()
        {
            DisconnectInputPort();

            DisconnectOutputPort();
        }

        private void DisconnectInputPort()
        {
            DisconnectPort(inputContainer);
        }

        private void DisconnectOutputPort()
        {
            DisconnectPort(outputContainer);
        }

        #region  Định nghĩa port.conected và port.connection

        /*--------------------------------------------------------------------------------------------------------------------------------------------*/

        /*

        1. port.connected

        ✅ Kiểu dữ liệu: bool (Boolean – true/false)

        ✅ Ý nghĩa:

        - Cho biết Port đó có đang kết nối với cổng khác không.

        ✅ Trả về:

        - true nếu có ít nhất một kết nối

        - false nếu chưa được kết nối với bất kỳ Port nào

        2. port.connections

        ✅ Kiểu dữ liệu: IEnumerable<Edge>

        ✅ Ý nghĩa:

        - Trả về tập hợp tất cả các Edge (đường kết nối) được nối vào Port này.

        - Mỗi Edge đại diện cho một kết nối giữa 2 Port.

        ✅ Dùng để:

        - Duyệt qua các kết nối, ví dụ để xóa, sửa, hiển thị.

        - Lấy Port đối diện bằng cách xem Edge.input và Edge.output.

        */
        /*--------------------------------------------------------------------------------------------------------------------------------------------*/

        #endregion

        private void DisconnectPort(VisualElement container)
        {
            foreach(Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();

            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }


        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
    }
}

