using UnityEngine;

public class RewardNode : ExploreNode
{
    protected override void OnInteract()
    {
        Debug.Log($"Reward claimed: {NodeId}");
        CompleteNode();
    }
}