using DS.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.windows
{
    using System;
    using DS.Windows;
    using Data.Error;
    using Enumerations;
    using Ultilities;
    using UnityEngine.Rendering;
    using static Unity.Burst.Intrinsics.X86.Avx;
    using DS.Data.Save;
    using Interaction;

    public class DSGraphView : GraphView
    {
        private DSEditorWindow editorWindow;

        private DSSearchWindow searchWindow;

        #region Định nghĩa

        /*--------------------------------------------------------------------------------------------------------------------------------------------*/

        /*ungroupedNodes là một dictionary dùng để chứa các Nodes giống nhau bên ngoài Group*/

        /*groupedNodes là một dictionary dùng để chứa các Nodes giống nhau bên trong Group*/

        /*groups là một dictionary dùng để chứa các Group giống nhau*/

        /*3 Dictionary trên giúp chúng ta phát hiện ra được tên Nodes và Group nào đang trùng nhau*/

        /*--------------------------------------------------------------------------------------------------------------------------------------------*/

        #endregion

        private SerializableDictionary<string, DSNodeErrorData> ungroupedNodes;

        private SerializableDictionary<string, DSGroupErrorData> groups;

        private SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>> groupedNodes;

        private int nameErrorAmount;

        private MiniMap miniMap;


        public int NameErrorAmount
        {
            get
            {
                return nameErrorAmount;
            }

            set
            {
                nameErrorAmount = value;

                if (nameErrorAmount == 0)
                {
                    editorWindow.EnableSaving();
                }

                if (nameErrorAmount == 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }

       public DSGraphView(DSEditorWindow dSEditorWindow)
        {
            editorWindow = dSEditorWindow;

            ungroupedNodes = new SerializableDictionary<string, DSNodeErrorData>();

            groups = new SerializableDictionary<string, DSGroupErrorData>();

            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>>();

            AddManipulator();

            AddSearchWindow();

            AddMiniMap();

            AddGridBackGround();

            OnElementDeleted();

            OngroupElementsAdded();

            OngroupElementsRemoved();

            OnGroupRenamed();

            OnGraphViewChanged();

            AddStyles();

            AddMiniMapStyles();
        }


        #region Override Method
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();


            /*UQueryState là một dạng truy vấn giúp tìm kiếm và lọc các phần tử trong UI (các VisualElement)*/
            ports.ForEach(port =>
            {
                if(startPort == port)
                {
                    return;
                }
                if (startPort.node == port.node)
                {
                    return;
                }
                if (startPort.direction == port.direction)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }
        #endregion


        
        public DSNode CreateNode(string nodeName, DSDialogType DialogueType,Vector2 position, bool shoulDraw = true)
        {

            /*--------------------------------------------------------------------------------------------------------------------------------------------*/

            /*Hãy nhớ Type trong C# không phải là nội dung của class mà là metadata (siêu dữ liệu) về class đó. Nó chứa thông tin như:

             - Tên class
             - Namespace
             -  Assembly chứa class
             -  Class cha (Base Type)
             - Các phương thức, thuộc tính, constructor, interface mà class đó kế thừa
            */

           /*--------------------------------------------------------------------------------------------------------------------------------------------*/


            Type nodetype = Type.GetType($"DS.Elements.DS{DialogueType}Node");

            /*Activator.CreateInstance yêu cầu kiểu dữ liệu là Type */
            DSNode node =(DSNode) Activator.CreateInstance(nodetype); 

            node.Initialize(nodeName, this, position);

            if(shoulDraw)
            {
                node.Draw();
            }

  

            AddUnGroupNode(node);

            return node;
           
        }
  
        private void AddGridBackGround()
        {
            GridBackground gridBackground = new GridBackground();

            gridBackground.StretchToParentSize();

            Insert(0, gridBackground);  
        }
        
        
        private void AddStyles()
        {
            this.AddStyleSheets(
                "DialogSystem/DSGraphViewStyles.uss",
                "DialogSystem/DSNodeStyles.uss"
                );
           

        }
     

        private void AddManipulator()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", DSDialogType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", DSDialogType.MultipleChoice));

            this.AddManipulator(CreateGroupContextualMenu());


        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, DSDialogType DialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                   menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode("DialogueName", DialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );  

            return contextualMenuManipulator;
        }


        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                   menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("Dialog Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );

            return contextualMenuManipulator;
        }

        public DSGroup CreateGroup(string title, Vector2 localMousePosition)
        {
            DSGroup group = new DSGroup(title, localMousePosition);

            AddGroup(group);

            #region Vấn đề mã A-1 : Lí do vì sao phải thêm dòng lệnh AddElement(group);
            /*--------------------------------------------------------------------------------------------------------------------------------------------*/

            /*Lí do vì sao phải thêm dòng lệnh AddElement(group); */

            /* Phân tích 
            Trong phương thức CreateGroup, bạn đang lặp qua selection để thêm các Node được chọn vào Group.Tuy nhiên, vấn đề nằm ở chỗ Group chưa được thêm vào GraphView trước khi các Node được thêm vào nó.

            Cụ thể, bạn đang làm theo thứ tự sau:

            1.Tạo Group mới

            2.Duyệt qua danh sách các Node được chọn(selection)

            3.Thêm từng Node vào Group

            Nhưng vấn đề là GraphView chưa nhận biết Group này, vì nó chưa được chính thức thêm vào. Điều này dẫn đến việc:

            GraphView chưa "lắng nghe" sự kiện của Group, do đó nó không gọi các callback cần thiết để chuyển đổi Node từ "ungrouped" sang "grouped".

            Khi bạn gọi group.AddElement(node), GraphView không nhận biết Group này nên nó không kích hoạt các sự kiện cần thiết để cập nhật groupedNodes.
            */

            /*--------------------------------------------------------------------------------------------------------------------------------------------*/

            #endregion
       
            #region Câu hỏi về vấn đề mã A-1: Chẳng phải group đã được cho vào GraphView thông qua IManipulator hay sao

            /*--------------------------------------------------------------------------------------------------------------------------------------------*/

            /* Đúng là trong phương thức CreateGroupContextualMenu, bạn đã thêm Group vào GraphView bằng cách gọi AddElement(CreateGroup(...)). 
             
               Điều đó có nghĩa là Group thực sự sẽ được thêm vào GraphView sau khi CreateGroup kết thúc.

               Tuy nhiên, vấn đề nằm ở thứ tự thực thi của sự kiện và cách GraphView xử lý các callback.

           Giải thích sâu hơn 

               1. Khi nào Group thực sự được thêm vào GraphView?

               Khi bạn gọi CreateGroup(...), phương thức này chỉ tạo ra Group và thêm các Node vào nó.

               Sau khi CreateGroup kết thúc, phương thức CreateGroupContextualMenu sẽ gọi AddElement(group).

               ⚠️ Nhưng tại thời điểm vòng lặp foreach trong CreateGroup chạy, Group vẫn chưa được GraphView nhận diện.

               GraphView không "nghe" sự kiện thêm Node vào Group, vì nó chưa biết đến sự tồn tại của Group.

               Callback elementsAddedToGroup chỉ được kích hoạt nếu Group đã là một phần của GraphView trước khi các Node được thêm vào.

               2. Điều gì sẽ xảy ra nếu Group chưa có trong GraphView khi thêm Node?
  
               Khi bạn gọi group.AddElement(node), Group chưa có trong GraphView, nên callback elementsAddedToGroup không được gọi.

               Điều này khiến Node không được di chuyển từ ungroupedNodes sang groupedNodes, vì sự kiện elementsAddedToGroup chưa được kích hoạt.


            /*--------------------------------------------------------------------------------------------------------------------------------------------*/

            #endregion

            AddElement(group);

            foreach(GraphElement graphElement in selection)
            {
                if(! (graphElement is DSNode))
                {
                    continue;
                }

                DSNode node = (DSNode) graphElement;

                group.AddElement(node);
            }
           
            return group;   
        }
   
        private void AddSearchWindow()
        {
            if(searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();                  
            }

            searchWindow.Initialize(this);

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow); 
            
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 wordMousePosition = mousePosition;

            if(isSearchWindow)
            {
                wordMousePosition -= editorWindow.position.position;
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(wordMousePosition);

            return localMousePosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            NameErrorAmount = 0;
        }

        private void AddMiniMap()
        {
            miniMap = new MiniMap()
            {
                anchored = true
            };

            miniMap.SetPosition(new Rect(15, 50, 200, 180));

            Add(miniMap);

            miniMap.visible = false;
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
        }

        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }


        #region Các phương thức lặp lại 

        public void AddUnGroupNode(DSNode node)
        {
            string nodename = node.DialogName.ToLower();

            if (!ungroupedNodes.ContainsKey(nodename))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                ungroupedNodes.Add(nodename, nodeErrorData);

                return;
            }

            List<DSNode> ungroupNodesList = ungroupedNodes[nodename].Nodes;

            ungroupNodesList.Add(node);

            Color errorcolor = ungroupedNodes[nodename].ErrorData.Color;

            node.SetErrorStyle(errorcolor);

            if (ungroupNodesList.Count == 2)
            {
                ++nameErrorAmount;

                ungroupNodesList[0].SetErrorStyle(errorcolor);
            }

        }

        public void RemoveUnGroupNode(DSNode node)
        {
            string nodename = node.DialogName.ToLower();



            List<DSNode> ungroupNodesList = ungroupedNodes[nodename].Nodes;

            ungroupNodesList.Remove(node);

            node.ResetStyle();

            if (ungroupNodesList.Count == 1)
            {
                --nameErrorAmount;

                ungroupNodesList[0].ResetStyle();

                return;
            }

            if (ungroupNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodename);
            }
        }

        public void AddGroupedNode(DSNode node, DSGroup group)
        {
            string nodename = node.DialogName.ToLower();

            node.Group = group;

            if (!groupedNodes.ContainsKey(group))
            {
                groupedNodes.Add(group, new SerializableDictionary<string, DSNodeErrorData>());
            }

            if (!groupedNodes[group].ContainsKey(nodename))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                groupedNodes[group].Add(nodename, nodeErrorData);

                return;
            }

            List<DSNode> groupNodesList = groupedNodes[group][nodename].Nodes;

            groupNodesList.Add(node);

            Color errorColor = groupedNodes[group][nodename].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (groupNodesList.Count == 2)
            {
                ++nameErrorAmount;

                groupNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupedNode(DSNode node, Group group)
        {
            string nodeName = node.DialogName.ToLower();

            node.Group = null;

            List<DSNode> groupNodesList = groupedNodes[group][nodeName].Nodes;

            groupNodesList.Remove(node);

            node.ResetStyle();

            if (groupNodesList.Count == 1)
            {
                --nameErrorAmount;

                groupNodesList[0].ResetStyle();

                return;
            }

            if (groupNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }

        private void AddGroup(DSGroup group)
        {
            string groupName = group.title.ToLower();

            if (!groups.ContainsKey(groupName))
            {
                DSGroupErrorData groupErrorData = new DSGroupErrorData();

                groupErrorData.Groups.Add(group);

                groups.Add(groupName, groupErrorData);

                return;
            }

            List<DSGroup> grouplist = groups[groupName].Groups;

            grouplist.Add(group);

            Color errorColor = groups[groupName].DSErrorData.Color;

            group.SetErrorStyle(errorColor);

            if (grouplist.Count == 2)
            {
                ++nameErrorAmount;

                grouplist[0].SetErrorStyle(errorColor);
            }
        }

        private void RemoveGroup(DSGroup group)
        {
            string oldGroupName = group.oldtitle.ToLower();

            List<DSGroup> grouplist = groups[oldGroupName].Groups;

            groups[oldGroupName].Groups.Remove(group);

            group.ResetStyle();

            if (grouplist.Count == 1)
            {
                --nameErrorAmount;

                grouplist[0].ResetStyle();

                return;
            }

            if (grouplist.Count == 1)
            {
                groups.Remove(oldGroupName);
            }
        }

        #endregion

        #region CallBacks
        private void OnElementDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {

                #region  Dòng "Type groupType = typeof(DSGroup);" có ý nghĩa gì ?

                /*--------------------------------------------------------------------------------------------------------------------------------------------*/

                /*
                 
                Dòng Type groupType = typeof(DSGroup); có vai trò lưu lại thông tin kiểu của DSGroup để dùng cho việc so sánh tại runtime, nhằm xác định phần tử trong selection có đúng là một nhóm (group) hay không.

                Các Type còn lại cũng tương tự như vậy

                */
                /*--------------------------------------------------------------------------------------------------------------------------------------------*/

                #endregion

                Type groupType = typeof(DSGroup);

                Type edgeType = typeof(Edge);

                List<DSGroup> grouptoDelete = new List<DSGroup>();

                List<Edge> edgetoDelete = new List<Edge>();

                List<DSNode> nodesToDelete = new List<DSNode>();

                foreach(GraphElement element in selection)
                {
                    if(element is DSNode node)
                    {
                        nodesToDelete.Add(node);

                        continue;
                    }

                    if(element.GetType() == edgeType)
                    {
                        Edge edge = (Edge) element;

                        edgetoDelete.Add(edge);

                        continue;
                    }

                    if(element.GetType() != groupType)
                    {
                        continue;
                    }

                    DSGroup group = (DSGroup) element;
                    
                    grouptoDelete.Add(group);
                }

                foreach(DSGroup group in grouptoDelete)
                {
                    List<DSNode> groupNodes = new List<DSNode>();

                    foreach(GraphElement groupElement in group.containedElements)
                    {
                        if(!(groupElement is DSNode))
                        {
                            continue ;
                        }

                        DSNode node = (DSNode) groupElement;    

                        groupNodes.Add(node);

                    }

                    #region Vấn đề A-2: Dòng group.RemoveElements(groupNodes) có ý nghĩa gì ?

                    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

                    /*
                    Dòng group.RemoveElements(groupNodes); đến từ phương thức của lớp Group trong Unity's GraphView API.

                    Nó đến từ đâu và có tác dụng gì?

                    RemoveElements(IEnumerable < GraphElement > elements) là một phương thức của lớp Group, có chức năng loại bỏ các phần tử(Node) khỏi Group.

                    Khi gọi group.RemoveElements(groupNodes);, nó sẽ:

                    1.Xóa các Node trong danh sách groupNodes khỏi Group.

                    2.Kích hoạt sự kiện elementsRemovedFromGroup, từ đó GraphView có thể xử lý lại danh sách Node.

                    3.Trả các Node bị xóa về trạng thái "ungrouped", nếu bạn đã xử lý sự kiện elementsRemovedFromGroup đúng cách.
                  
                    */
                    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

                    #endregion


                    #region Câu hỏi của vấn đề A-2: Tại sao phải gọi dòng này trước khi RemoveElement(group);
                    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

                    /*
                    ?.Tại sao phải gọi dòng này trước khi RemoveElement(group);?

                    Nếu bạn chỉ gọi RemoveElement(group); mà không xóa các Node trước, có thể xảy ra các vấn đề:

                    1.Các Node vẫn còn trong GraphView nhưng không còn thuộc về Group.

                     - Điều này có thể gây lỗi khi truy cập group.containedElements, vì Group đã bị xóa.

                    2.Không kích hoạt callback elementsRemovedFromGroup.

                     -Nếu không gọi RemoveElements, sự kiện elementsRemovedFromGroup sẽ không chạy, và các Node sẽ không được cập nhật trạng thái.

                    3.Dữ liệu trong groupedNodes có thể bị sai.

                     - Nếu Group bị xóa mà các Node vẫn còn, danh sách groupedNodes có thể chứa thông tin không hợp lệ.

                    Tóm lại

                    Dòng group.RemoveElements(groupNodes); giúp xóa đúng cách các Node khỏi Group trước khi xóa chính Group đó, đảm bảo hệ thống GraphView cập nhật đúng trạng thái của các Node.
                    */

                    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

                    #endregion

                    group.RemoveElements(groupNodes);

                    RemoveGroup(group);

                    RemoveElement(group);
                }

                DeleteElements(edgetoDelete);

                foreach(DSNode node in nodesToDelete)
                {
                    if(node.Group != null)
                    {
                        node.Group.RemoveElement(node);
                    }

                    RemoveUnGroupNode(node);

                    node.DisconnectAllPorts();

                    RemoveElement(node);
                }
            };
        }

        private void OngroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach(GraphElement element in elements)
                {
                    if(!(element is DSNode))
                    {
                        continue;
                    }

                    DSGroup nodeGroup = (DSGroup) group;

                    DSNode node = (DSNode) element;

                    RemoveUnGroupNode(node);

                    AddGroupedNode(node, nodeGroup);
                } 
            };
        }

        private void OngroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }

                    DSNode node = (DSNode)element;

                    RemoveGroupedNode(node, group);

                    AddUnGroupNode(node);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newtitle) =>
            {
                DSGroup dSGroup = (DSGroup) group;

                dSGroup.title = newtitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(dSGroup.title))
                {
                    if (!string.IsNullOrEmpty(dSGroup.oldtitle))
                    {
                        ++nameErrorAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dSGroup.oldtitle))
                    {
                        --nameErrorAmount;
                    }
                }

                RemoveGroup(dSGroup);

                /*Nếu muốn có WhiteSpaces cho tên Group thì thay dSGroup.oldtitle bằng newtitle*/
                dSGroup.oldtitle = dSGroup.title;

                AddGroup(dSGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DSNode nextNode = (DSNode)edge.input.node;

                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;

                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }

                        Edge edge = (Edge)element;

                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;

                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }

        #endregion
    }
}

