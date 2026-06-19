using System.Threading;
using ActiveTimeBattle.Application;
using ActiveTimeBattle.Presentation.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ActiveTimeBattle.Presentation.Hub
{
    public sealed class StartRunInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "深淵入口";
        [SerializeField] private string interactionPrompt = "開始新的冒險";

        public string DisplayName => displayName;

        public string InteractionPrompt => interactionPrompt;

        public bool CanInteract(HubContext context)
        {
            return !context.IsBusy
                && GameBootstrapper.Instance != null;
        }

        public async UniTask InteractAsync(
            HubContext context,
            CancellationToken cancellationToken)
        {
            GameBootstrapper bootstrapper = GameBootstrapper.Instance;
            if (bootstrapper == null)
            {
                return;
            }

            await bootstrapper.RunFlowController
                .StartNewRunAsync(bootstrapper.LifetimeToken);
        }
    }
}
