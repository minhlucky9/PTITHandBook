using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace DS.Elements
{
    using Data.Save;
    using Enumerations;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine.UIElements;
    using UnityEditor.UIElements;

    using Ultilities;
    using windows;
    using System.Net;

    public class DSMultipleChoiceNode : DSNode
    {
    
        public override void Initialize(string nodeName, DSGraphView dSGraphView, Vector2 position)
        {
            base.Initialize(nodeName, dSGraphView, position);

            DialogueType = DSDialogType.MultipleChoice;

            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = "New Choice",
                Icon = null,
            };

            Choices.Add(choiceData);
        }
        public override void Draw()
        {
            base.Draw();

            /* MAIN CONTAINER */
            Button addChoiceButton = DSElementsUltilities.CreateButton("Add choice", () =>
            {

                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = "New Choice",
                    Icon = null,
                };
                          
                Choices.Add(choiceData);

                Port choicePort = CreateChoicePort(choiceData);

                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);

            /* OUTPUT CONTAINER */
            foreach (DSChoiceSaveData choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }

        private Port CreateChoicePort(object userData)
        {
            Port ChoicePort = this.CreatePort();

            ChoicePort.userData = userData;

            DSChoiceSaveData ChoiceData = (DSChoiceSaveData) userData;

            ChoicePort.portName = "";

            Button DeleteChoiceButton = DSElementsUltilities.CreateButton("X", () =>
            {
                if(Choices.Count == 1)
                {
                    return;
                }

                if(ChoicePort.connected)
                {
                    graphView.DeleteElements(ChoicePort.connections);
                }

                Choices.Remove(ChoiceData);

                graphView.RemoveElement(ChoicePort);
            });

            TextField choicetextField = DSElementsUltilities.CreateTextField(ChoiceData.Text, null, callback =>
            {
                ChoiceData.Text = callback.newValue;
            });

            choicetextField.style.flexDirection = FlexDirection.Column; // Dòng này sửa lỗi inputContainer và outputContainer dính vào nhau 

            choicetextField.AddClasses(
                "ds-node__textfield",
                "ds-node__choice-textfield",
                "ds-node__textfield__hidden"
                );
        
            ChoicePort.Add(choicetextField);

          
            ObjectField iconField = new ObjectField();
            iconField.objectType = typeof(Sprite);
            iconField.value = ChoiceData.Icon;
            iconField.RegisterValueChangedCallback(evt =>
            {
                ChoiceData.Icon = (Sprite)evt.newValue;
            });
            iconField.style.minWidth = 32;
            iconField.style.maxWidth = 132;
            ChoicePort.Add(iconField);

            ChoicePort.Add(DeleteChoiceButton);

            return ChoicePort;
        }

    }
}

