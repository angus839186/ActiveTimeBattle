using UnityEngine;

public enum ExploreNodeType
{
    Enter,
    Out,
    Reward,
    Shop,
    Rest,
    Battle,
    EliteBattle,
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

    protected RoomController RoomController => roomController;

    protected virtual void Start()
    {
        RestoreNodeState();
    }

    protected virtual void RestoreNodeState()
    {
        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;

        if (runSession != null && runSession.IsNodeCompleted(nodeId))
        {
            IsCompleted = true;
            gameObject.SetActive(false);
        }
    }
    public void Activate(string nodeId, RoomController roomController)
    {
        gameObject.SetActive(true);
        Initialize(nodeId, roomController);
        OnActivated();
    }

    public void Initialize(string nodeId, RoomController roomController)
    {
        this.nodeId = nodeId;
        this.roomController = roomController;

        IsCompleted = false;
        RestoreNodeState();
    }

    protected virtual void OnActivated()
    {
    }
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
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;

        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;

        if (runSession != null)
        {
            runSession.CompleteNode(nodeId);
        }

        if (roomController == null)
        {
            Debug.LogWarning($"{name}: RoomController is not assigned.");
            gameObject.SetActive(false);
            return;
        }

        roomController.CompleteRoom();
        gameObject.SetActive(false);
    }

    protected abstract void OnInteract();
}