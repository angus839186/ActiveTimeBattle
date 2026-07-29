using UnityEngine;
using UnityEngine.InputSystem;

public class ExplorePlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private Transform cameraTransform;

    private InputAction moveAction;
    private Vector2 moveInput;
    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        moveAction = playerInput.actions["Move"];
    }

    private void OnEnable()
    {
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
    }

    private void Update()
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 move = cameraRight * moveInput.x + cameraForward * moveInput.y;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    public void TeleportTo(Transform targetPoint)
    {
        if (targetPoint == null)
        {
            Debug.LogWarning("ExplorePlayerController: Target point is null.");
            return;
        }

        characterController.enabled = false;
        transform.position = targetPoint.position;
        characterController.enabled = true;
    }

    public void TeleportTo(Vector3 position)
    {
        characterController.enabled = false;
        transform.position = position;
        characterController.enabled = true;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}