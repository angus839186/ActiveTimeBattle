using System.Threading;
using Cysharp.Threading.Tasks;

namespace ActiveTimeBattle.Application
{
    public interface ISceneFlowService
    {
        UniTask LoadSceneAsync(string sceneName, CancellationToken cancellationToken);
    }
}
