using UnityEngine;

public class ShopNode : ExploreNode
{
    protected override void OnInteract()
    {
        Debug.Log($"Shop opened: {NodeId}");
        CompleteNode();
    }
}