using UnityEngine;

public enum ExploreNodeType
{
    Battle,
    EliteBattle,
    Reward,
    Shop,
    Rest,
    Enter,
    Out,
    SpecialEvent
}
public abstract class ExploreEventNode : MonoBehaviour, IExploreInteractable
{
    [SerializeField] private string nodeId;
    [SerializeField] private RoomController roomController;

    [SerializeField] private ExploreNodeType nodeType;
    public ExploreNodeType NodeType => nodeType;

    public string NodeId => nodeId;
    public bool IsCompleted { get; private set; }

    public void Interact()
    {
        if (IsCompleted)
        {
            return;
        }

        OnInteract();
    }

    protected void CompleteNode()
    {
        IsCompleted = true;
        roomController.CompleteRoom();
    }

    protected abstract void OnInteract();
}