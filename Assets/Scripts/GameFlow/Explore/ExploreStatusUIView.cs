using System.Linq;
using TMPro;
using UnityEngine;

public class ExploreStatusUIView : MonoBehaviour
{
    [SerializeField] private ExploreInteractionController interactionController;

    [SerializeField] private TMP_Text currentRoomText;
    [SerializeField] private TMP_Text currentRoomTypeText;
    [SerializeField] private TMP_Text currentInteractableText;
    [SerializeField] private TMP_Text completedNodesText;

    private void Update()
    {
        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;

        if (runSession == null)
        {
            SetEmpty();
            return;
        }

        RoomData currentRoom = runSession.ExploreMap != null
            ? runSession.ExploreMap.GetRoom(runSession.CurrentExploreRoomId)
            : null;

        currentRoomText.text = $"Room: {runSession.CurrentExploreRoomId}";

        currentRoomTypeText.text = currentRoom != null
            ? $"Type: {currentRoom.NodeType}"
            : "Type: None";

        currentInteractableText.text = interactionController != null
            ? $"Interactable: {interactionController.CurrentInteractableName}"
            : "Interactable: None";

        completedNodesText.text = runSession.CompletedNodes.Count > 0
            ? $"Completed Nodes:\n{string.Join("\n", runSession.CompletedNodes)}"
            : "Completed Nodes: None";
    }

    private void SetEmpty()
    {
        currentRoomText.text = "Room: None";
        currentRoomTypeText.text = "Type: None";
        currentInteractableText.text = "Interactable: None";
        completedNodesText.text = "Completed Nodes: None";
    }
}