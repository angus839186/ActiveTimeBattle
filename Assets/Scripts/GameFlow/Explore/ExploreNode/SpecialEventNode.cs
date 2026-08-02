using UnityEngine;

public class SpecialEventNode : ExploreNode
{
    protected override void OnInteract()
    {
        Debug.Log($"Special event completed: {NodeId}");
        CompleteNode();
    }
}