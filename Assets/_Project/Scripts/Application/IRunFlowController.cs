using System.Threading;
using ActiveTimeBattle.Domain;
using Cysharp.Threading.Tasks;

namespace ActiveTimeBattle.Application
{
    public interface IRunFlowController
    {
        RunSession CurrentSession { get; }

        UniTask EnterHubAsync(CancellationToken cancellationToken);

        UniTask EnterCombatAsync(CancellationToken cancellationToken);

        UniTask ReturnToExplorationAsync(CancellationToken cancellationToken);

        UniTask StartNewRunAsync(CancellationToken cancellationToken);
    }
}
