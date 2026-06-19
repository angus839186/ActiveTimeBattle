using System.Threading;
using ActiveTimeBattle.Application;
using ActiveTimeBattle.Presentation.Hub;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ActiveTimeBattle.Presentation.Narrative
{
    public sealed class DialogueInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "互動物";
        [SerializeField] private string interactionPrompt = "交談";
        [SerializeField] private DialogueData dialogue;
        [SerializeField] private DialogueRunner dialogueRunner;

        public string DisplayName => displayName;

        public string InteractionPrompt => interactionPrompt;

        public bool CanInteract(HubContext context)
        {
            return !context.IsBusy
                && dialogue != null
                && dialogueRunner != null
                && !dialogueRunner.IsPlaying;
        }

        public UniTask InteractAsync(
            HubContext context,
            CancellationToken cancellationToken)
        {
            return dialogueRunner.PlayAsync(dialogue, cancellationToken);
        }
    }
}
