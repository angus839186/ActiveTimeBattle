using UnityEngine;

public class SpecialEventNode : ExploreEventNode
{
    protected override void OnInteract()
    {
        Debug.Log($"Special event completed: {NodeId}");
        CompleteNode();
    }
}