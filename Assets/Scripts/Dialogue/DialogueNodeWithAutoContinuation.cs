using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace RPGPlatformer.Dialogue
{
    public class DialogueNodeWithAutoContinuation : DialogueNode
    {
        protected override void DrawOutputContainer()
        {
            Port continuationOutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output,
                    Port.Capacity.Single, typeof(Node));
            continuationOutputPort.portName = "Continuation";

            outputContainer.Add(continuationOutputPort);
        }
    }
}