using UnityEngine;

public class TestExploreNode : ExploreNode
{

    protected override void OnInteract()
    {
        if (GameFlowController.Instance == null)
        {
            Debug.LogWarning("testExploreNode: GameFlowController not found.");
            return;
        }

        CompleteNode();
        Debug.Log($"Interacted with {name}");
    }
}