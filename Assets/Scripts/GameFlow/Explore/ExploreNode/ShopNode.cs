using UnityEngine;

public class ShopNode : ExploreEventNode
{
    protected override void OnInteract()
    {
        Debug.Log($"Shop opened: {NodeId}");
        CompleteNode();
    }
}