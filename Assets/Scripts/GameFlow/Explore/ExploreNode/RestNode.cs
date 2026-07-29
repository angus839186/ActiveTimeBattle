using UnityEngine;

public class RestNode : ExploreEventNode
{
    protected override void OnInteract()
    {
        Debug.Log($"Rest used: {NodeId}");
        CompleteNode();
    }
}