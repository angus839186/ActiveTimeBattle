using UnityEngine;

public class RestNode : ExploreNode
{
    protected override void OnInteract()
    {
        Debug.Log($"Rest used: {NodeId}");
        CompleteNode();
    }
}