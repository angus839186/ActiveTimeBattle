using System.Threading;
using ActiveTimeBattle.Application;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace ActiveTimeBattle.Presentation.Core
{
    public sealed class UnitySceneFlowService : ISceneFlowService
    {
        public async UniTask LoadSceneAsync(
            string sceneName,
            CancellationToken cancellationToken)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)
                .ToUniTask(cancellationToken: cancellationToken);
        }
    }
}
