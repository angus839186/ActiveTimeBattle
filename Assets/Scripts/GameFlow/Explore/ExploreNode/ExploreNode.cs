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
public class ExploreNode : MonoBehaviour, IExploreInteractable
{
    [SerializeField] private string nodeId;
    [SerializeField] private RoomController roomController;

    [SerializeField] private ExploreNodeType nodeType;

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private ExploreNodeMaterial[] materialEntries;
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
    public void Activate(string nodeId, ExploreNodeType nodeType, RoomController roomController)
    {
        gameObject.SetActive(true);
        Initialize(nodeId, nodeType, roomController);
        ApplyMaterial();
        OnActivated();
    }

    public void Initialize(string nodeId, ExploreNodeType nodeType, RoomController roomController)
    {
        this.nodeId = nodeId;
        this.nodeType = nodeType;
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

    protected virtual void OnInteract()
    {
        switch (NodeType)
        {
            case ExploreNodeType.Battle:
            case ExploreNodeType.EliteBattle:
                StartBattle();
                break;

            case ExploreNodeType.Enter:
                CompleteNode();
                break;

            case ExploreNodeType.Out:
                Debug.Log($"Exit reached: {NodeId}");
                CompleteNode();
                break;

            case ExploreNodeType.Reward:
                Debug.Log($"Reward claimed: {NodeId}");
                CompleteNode();
                break;

            case ExploreNodeType.Shop:
                Debug.Log($"Shop opened: {NodeId}");
                CompleteNode();
                break;

            case ExploreNodeType.Rest:
                Debug.Log($"Rest used: {NodeId}");
                CompleteNode();
                break;

            case ExploreNodeType.SpecialEvent:
                Debug.Log($"Special event completed: {NodeId}");
                CompleteNode();
                break;
        }
    }

    private void StartBattle()
    {
        if (GameFlowController.Instance == null)
        {
            Debug.LogWarning("ExploreNode: GameFlowController not found.");
            return;
        }

        RunSession runSession = GameFlowController.Instance.CurrentRunSession;

        if (runSession != null)
        {
            runSession.StartPendingBattle(RoomController.RoomId, NodeId);

            ExplorePlayerController player = FindFirstObjectByType<ExplorePlayerController>();

            if (player != null)
            {
                runSession.SetExploreReturnPosition(player.transform.position);
            }
        }

        Debug.Log($"Start battle from node: {NodeId}, Type: {NodeType}");
        GameFlowController.Instance.ChangeToBattle();
    }

    private void ApplyMaterial()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer == null)
        {
            Debug.LogWarning("ExploreNode: Target renderer is not assigned.");
            return;
        }

        Material material = GetMaterial(NodeType);

        if (material == null)
        {
            Debug.LogWarning($"ExploreNode: Material not assigned for type: {NodeType}");
            return;
        }

        targetRenderer.material = material;
    }

    private Material GetMaterial(ExploreNodeType nodeType)
    {
        if (materialEntries == null)
        {
            return null;
        }

        foreach (ExploreNodeMaterial entry in materialEntries)
        {
            if (entry != null && entry.NodeType == nodeType)
            {
                return entry.Material;
            }
        }

        return null;
    }
}