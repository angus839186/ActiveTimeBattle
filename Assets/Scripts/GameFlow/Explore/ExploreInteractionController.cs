using UnityEngine;
using UnityEngine.InputSystem;

public class ExploreInteractionController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private InputAction interactAction;
    private IExploreInteractable currentInteractable;

    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = GetComponentInParent<PlayerInput>();
        }

        interactAction = playerInput.actions["Interact"];
    }

    private void OnEnable()
    {
        interactAction.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (currentInteractable == null)
        {
            Debug.Log("No interactable target.");
            return;
        }

        currentInteractable.Interact();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IExploreInteractable interactable))
        {
            currentInteractable = interactable;
            Debug.Log($"Enter interact range: {other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IExploreInteractable interactable) &&
            interactable == currentInteractable)
        {
            currentInteractable = null;
            Debug.Log($"Exit interact range: {other.name}");
        }
    }
}