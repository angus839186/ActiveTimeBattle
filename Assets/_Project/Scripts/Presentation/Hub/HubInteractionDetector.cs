using System.Threading;
using ActiveTimeBattle.Application;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveTimeBattle.Presentation.Hub
{
    public sealed class HubInteractionDetector : MonoBehaviour
    {
        [SerializeField] private Camera raycastCamera;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private InteractionPromptView promptView;
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField] private float interactionDistance = 3f;

        private readonly HubContext _context = new HubContext();
        private InputAction _interactAction;
        private IInteractable _currentTarget;
        private CancellationTokenSource _interactionCancellation;

        private void Awake()
        {
            InputActionAsset actions = inputActions != null
                ? inputActions
                : InputSystem.actions;
            _interactAction = actions.FindAction("Hub/Interact", true);
            _interactionCancellation = new CancellationTokenSource();
        }

        private void OnEnable()
        {
            _interactAction.performed += OnInteractPerformed;
        }

        private void OnDisable()
        {
            _interactAction.performed -= OnInteractPerformed;
            promptView.Hide();
        }

        private void OnDestroy()
        {
            _interactionCancellation.Cancel();
            _interactionCancellation.Dispose();
        }

        private void Update()
        {
            if (_context.IsBusy)
            {
                return;
            }

            if (TryGetInteractable(out IInteractable interactable))
            {
                if (!ReferenceEquals(_currentTarget, interactable))
                {
                    _currentTarget = interactable;
                    promptView.Show(
                        interactable.DisplayName,
                        interactable.InteractionPrompt);
                }

                return;
            }

            ClearTarget();
        }

        private void OnInteractPerformed(InputAction.CallbackContext callbackContext)
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            if (_context.IsBusy
                || !TryGetInteractable(out IInteractable target))
            {
                return false;
            }

            _currentTarget = target;
            InteractAsync(target, _interactionCancellation.Token).Forget();
            return true;
        }

        private async UniTask InteractAsync(
            IInteractable target,
            CancellationToken cancellationToken)
        {
            _context.SetBusy(true);

            try
            {
                await target.InteractAsync(_context, cancellationToken);
            }
            finally
            {
                _context.SetBusy(false);
                if (this != null)
                {
                    ClearTarget();
                }
            }
        }

        private void ClearTarget()
        {
            if (_currentTarget == null)
            {
                return;
            }

            _currentTarget = null;
            promptView.Hide();
        }

        private bool TryGetInteractable(out IInteractable interactable)
        {
            interactable = null;
            Ray ray = raycastCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f));
            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    interactionDistance,
                    interactableLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            MonoBehaviour[] behaviours =
                hit.collider.GetComponentsInParent<MonoBehaviour>(true);
            for (int index = 0; index < behaviours.Length; index++)
            {
                if (behaviours[index] is IInteractable candidate
                    && candidate.CanInteract(_context))
                {
                    interactable = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
