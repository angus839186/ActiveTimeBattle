using System;
using System.Threading;
using ActiveTimeBattle.Domain;
using Cysharp.Threading.Tasks;

namespace ActiveTimeBattle.Application
{
    public sealed class RunFlowController : IRunFlowController
    {
        private readonly GameStateMachine _stateMachine;
        private readonly ISceneFlowService _sceneFlowService;
        private bool _isTransitioning;

        public RunFlowController(
            GameStateMachine stateMachine,
            ISceneFlowService sceneFlowService)
        {
            _stateMachine = stateMachine;
            _sceneFlowService = sceneFlowService;
        }

        public RunSession CurrentSession { get; private set; }

        public async UniTask EnterHubAsync(CancellationToken cancellationToken)
        {
            if (_isTransitioning)
            {
                return;
            }

            _isTransitioning = true;

            try
            {
                _stateMachine.ChangeState(GameFlowState.ReturnToHub);
                CurrentSession = null;
                await _sceneFlowService.LoadSceneAsync("01_Hub", cancellationToken);
                _stateMachine.ChangeState(GameFlowState.Hub);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public async UniTask StartNewRunAsync(CancellationToken cancellationToken)
        {
            if (_isTransitioning)
            {
                return;
            }

            _isTransitioning = true;

            try
            {
                _stateMachine.ChangeState(GameFlowState.RunGeneration);
                CurrentSession = new RunSession(CreateSeed());
                await _sceneFlowService.LoadSceneAsync("02_Exploration", cancellationToken);
                _stateMachine.ChangeState(GameFlowState.Exploration);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public async UniTask EnterCombatAsync(CancellationToken cancellationToken)
        {
            if (_isTransitioning || CurrentSession == null)
            {
                return;
            }

            _isTransitioning = true;

            try
            {
                await _sceneFlowService.LoadSceneAsync("03_Combat", cancellationToken);
                _stateMachine.ChangeState(GameFlowState.Combat);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public async UniTask ReturnToExplorationAsync(
            CancellationToken cancellationToken)
        {
            if (_isTransitioning || CurrentSession == null)
            {
                return;
            }

            _isTransitioning = true;

            try
            {
                CurrentSession.CompleteCurrentNode();
                await _sceneFlowService.LoadSceneAsync(
                    "02_Exploration",
                    cancellationToken);
                _stateMachine.ChangeState(GameFlowState.Exploration);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private static int CreateSeed()
        {
            return HashCode.Combine(
                DateTime.UtcNow.Ticks,
                Guid.NewGuid().GetHashCode());
        }
    }
}
