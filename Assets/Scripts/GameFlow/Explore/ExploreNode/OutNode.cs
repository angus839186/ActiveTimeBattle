using UnityEngine;

public class OutNode : ExploreEventNode
{
    protected override void OnInteract()
    {
        Debug.Log($"Exit reached: {NodeId}");
        CompleteNode();
    }
}