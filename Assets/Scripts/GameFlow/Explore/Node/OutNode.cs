using UnityEngine;

public class OutNode : ExploreNode
{
    protected override void OnInteract()
    {
        Debug.Log($"Exit reached: {NodeId}");
        CompleteNode();
    }
}