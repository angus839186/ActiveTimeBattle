using UnityEngine;

public class RewardNode : ExploreEventNode
{
    protected override void OnInteract()
    {
        Debug.Log($"Reward claimed: {NodeId}");
        CompleteNode();
    }
}