using System.Threading;
using ActiveTimeBattle.Application;
using ActiveTimeBattle.Domain;
using ActiveTimeBattle.Domain.Narrative;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ActiveTimeBattle.Presentation.Core
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        public static GameBootstrapper Instance { get; private set; }

        public IRunFlowController RunFlowController { get; private set; }

        public GameStateMachine StateMachine { get; private set; }

        public NarrativeContext NarrativeContext { get; private set; }

        public CancellationToken LifetimeToken =>
            _lifetimeCancellation?.Token ?? destroyCancellationToken;

        private CancellationTokenSource _lifetimeCancellation;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _lifetimeCancellation = new CancellationTokenSource();
            StateMachine = new GameStateMachine();
            NarrativeContext = new NarrativeContext();
            StateMachine.StateChanged += OnGameStateChanged;
            RunFlowController = new RunFlowController(
                StateMachine,
                new UnitySceneFlowService());
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene().name == "00_Bootstrap")
            {
                RunFlowController.EnterHubAsync(_lifetimeCancellation.Token).Forget();
            }
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            _lifetimeCancellation.Cancel();
            _lifetimeCancellation.Dispose();
            StateMachine.StateChanged -= OnGameStateChanged;
            Instance = null;
        }

        private void OnGameStateChanged(GameFlowState state)
        {
            if (state == GameFlowState.ReturnToHub)
            {
                NarrativeContext.Clear();
            }
        }
    }
}
