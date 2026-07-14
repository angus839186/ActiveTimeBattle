using UnityEngine;

public class TestExploreNode : ExploreEventNode
{

    protected override void OnInteract()
    {
        if (GameFlowController.Instance == null)
        {
            Debug.LogWarning("BattleNode: GameFlowController not found.");
            return;
        }

        CompleteNode();
        Debug.Log($"Interacted with {name}");
    }
}