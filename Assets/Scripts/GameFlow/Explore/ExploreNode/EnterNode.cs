using UnityEngine;

public class EnterNode : ExploreEventNode
{
    protected override void OnActivated()
    {
        if (!IsCompleted)
        {
            Debug.Log($"Enter node completed: {NodeId}");
            CompleteNode();
        }
    }

    protected override void OnInteract()
    {
    }
}