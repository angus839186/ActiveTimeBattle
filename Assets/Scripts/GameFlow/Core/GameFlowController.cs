using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    public string CurrentStateName => stateMachine.CurrentStateName;

    [SerializeField] private StateMachine stateMachine;
    [SerializeField] private InputModeController inputModeController;
    [SerializeField] private CameraModeController cameraModeController;
    [SerializeField] private PlayerControlModeController playerControlModeController;

    public RunSession CurrentRunSession => context?.RunSession;

    private GameFlowContext context;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (stateMachine == null)
        {
            stateMachine = GetComponent<StateMachine>();
        }

        context = new GameFlowContext(
            inputModeController,
            cameraModeController,
            playerControlModeController,
            new SceneFlowController(),
            new RunSession());
    }

    private void Start()
    {
        ChangeToLobby();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ChangeToLobby()
    {
        stateMachine.ChangeState(new LobbyState(context));
    }

    public void ChangeToExplore()
    {
        stateMachine.ChangeState(new ExploreState(context));
    }

    public void ChangeToBattle()
    {
        stateMachine.ChangeState(new BattleState(context));
    }
    public void StartNewRun(string selectedClassId)
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);

        context.RunSession.StartRun(selectedClassId, seed);
        ChangeToExplore();
    }

    public void StartNewRun(PlayerClassDefinition selectedClass)
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);

        context.RunSession.StartRun(selectedClass, seed);
        ChangeToExplore();
    }
}