using System.Threading;
using System.Threading.Tasks;
using ActiveTimeBattle.Application;
using ActiveTimeBattle.Domain;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace ActiveTimeBattle.Tests
{
    public sealed class RunFlowControllerTests
    {
        [Test]
        public async Task StartNewRunCreatesSessionAndEntersExploration()
        {
            GameStateMachine stateMachine = new GameStateMachine();
            RecordingSceneFlowService sceneFlow = new RecordingSceneFlowService();
            RunFlowController controller =
                new RunFlowController(stateMachine, sceneFlow);

            await controller.StartNewRunAsync(CancellationToken.None);

            Assert.That(controller.CurrentSession, Is.Not.Null);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(GameFlowState.Exploration));
            Assert.That(sceneFlow.LastSceneName, Is.EqualTo("02_Exploration"));
        }

        [Test]
        public async Task EnterHubClearsCurrentRunSession()
        {
            GameStateMachine stateMachine = new GameStateMachine();
            RecordingSceneFlowService sceneFlow = new RecordingSceneFlowService();
            RunFlowController controller =
                new RunFlowController(stateMachine, sceneFlow);
            await controller.StartNewRunAsync(CancellationToken.None);

            await controller.EnterHubAsync(CancellationToken.None);

            Assert.That(controller.CurrentSession, Is.Null);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(GameFlowState.Hub));
            Assert.That(sceneFlow.LastSceneName, Is.EqualTo("01_Hub"));
        }

        [Test]
        public async Task StartingTwoRunsCreatesDifferentSessions()
        {
            GameStateMachine stateMachine = new GameStateMachine();
            RecordingSceneFlowService sceneFlow = new RecordingSceneFlowService();
            RunFlowController controller =
                new RunFlowController(stateMachine, sceneFlow);

            await controller.StartNewRunAsync(CancellationToken.None);
            RunSession firstSession = controller.CurrentSession;
            await controller.EnterHubAsync(CancellationToken.None);
            await controller.StartNewRunAsync(CancellationToken.None);

            Assert.That(controller.CurrentSession, Is.Not.SameAs(firstSession));
            Assert.That(controller.CurrentSession.Seed, Is.Not.EqualTo(firstSession.Seed));
        }

        [Test]
        public async Task CombatFlowReturnsToExplorationWithCurrentRun()
        {
            GameStateMachine stateMachine = new GameStateMachine();
            RecordingSceneFlowService sceneFlow = new RecordingSceneFlowService();
            RunFlowController controller =
                new RunFlowController(stateMachine, sceneFlow);
            await controller.StartNewRunAsync(CancellationToken.None);
            RunSession session = controller.CurrentSession;

            await controller.EnterCombatAsync(CancellationToken.None);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(GameFlowState.Combat));
            Assert.That(sceneFlow.LastSceneName, Is.EqualTo("03_Combat"));

            await controller.ReturnToExplorationAsync(CancellationToken.None);
            Assert.That(
                stateMachine.CurrentState,
                Is.EqualTo(GameFlowState.Exploration));
            Assert.That(sceneFlow.LastSceneName, Is.EqualTo("02_Exploration"));
            Assert.That(controller.CurrentSession, Is.SameAs(session));
            Assert.That(session.IsCurrentNodeCompleted, Is.True);
        }

        [Test]
        public async Task PlayerHealthPersistsWhenReturningFromCombat()
        {
            GameStateMachine stateMachine = new GameStateMachine();
            RecordingSceneFlowService sceneFlow = new RecordingSceneFlowService();
            RunFlowController controller =
                new RunFlowController(stateMachine, sceneFlow);
            await controller.StartNewRunAsync(CancellationToken.None);
            RunSession session = controller.CurrentSession;
            session.Player.ApplyDamage(35);

            await controller.EnterCombatAsync(CancellationToken.None);
            await controller.ReturnToExplorationAsync(CancellationToken.None);

            Assert.That(controller.CurrentSession, Is.SameAs(session));
            Assert.That(session.Player.CurrentHealth, Is.EqualTo(65));
        }

        private sealed class RecordingSceneFlowService : ISceneFlowService
        {
            public string LastSceneName { get; private set; }

            public UniTask LoadSceneAsync(
                string sceneName,
                CancellationToken cancellationToken)
            {
                LastSceneName = sceneName;
                return UniTask.CompletedTask;
            }
        }
    }
}
