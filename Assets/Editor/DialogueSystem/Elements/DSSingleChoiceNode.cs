using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


 namespace DS.Elements
{
    using Data.Save;
    using Enumerations;
    using Ultilities;
    using windows;

    public class DSSingleChoiceNode : DSNode
    {
        public override void Initialize(string nodeName, DSGraphView dSGraphView, Vector2 position)
        {
            base.Initialize(nodeName, dSGraphView, position);

            DialogueType = DSDialogType.SingleChoice;

            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = "Next Dialogue"
            };

            Choices.Add(choiceData);

        }
        public override void Draw()
        {
            base.Draw();
          
            /* OUTPUT CONTAINER */
            foreach (DSChoiceSaveData choice in Choices)
            {
                Port ChoicePort = this.CreatePort(choice.Text);

                ChoicePort.userData = choice;

                ChoicePort.portName = choice.Text;

                outputContainer.Add(ChoicePort);


            }
            RefreshExpandedState();
        }

        
    }


}

