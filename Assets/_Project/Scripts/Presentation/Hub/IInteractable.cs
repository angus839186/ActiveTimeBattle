using System.Threading;
using ActiveTimeBattle.Application;
using Cysharp.Threading.Tasks;

namespace ActiveTimeBattle.Presentation.Hub
{
    public interface IInteractable
    {
        string DisplayName { get; }

        string InteractionPrompt { get; }

        bool CanInteract(HubContext context);

        UniTask InteractAsync(
            HubContext context,
            CancellationToken cancellationToken);
    }
}
