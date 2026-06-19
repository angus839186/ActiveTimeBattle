using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveTimeBattle.Presentation.Hub
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float lookSensitivity = 0.08f;
        [SerializeField] private float gravity = -20f;

        private CharacterController _characterController;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private float _pitch;
        private float _verticalVelocity;
        private int _lookWarmupFrames;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            InputActionAsset actions = inputActions != null
                ? inputActions
                : InputSystem.actions;
            _moveAction = actions.FindAction("Hub/Move", true);
            _lookAction = actions.FindAction("Hub/Look", true);
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _lookWarmupFrames = 2;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            UpdateLook();
            UpdateMovement();
        }

        private void UpdateLook()
        {
            if (!UnityEngine.Application.isFocused || _lookWarmupFrames-- > 0)
            {
                return;
            }

            Vector2 rawLook = _lookAction.ReadValue<Vector2>();
            if (rawLook.sqrMagnitude > 10000f)
            {
                return;
            }

            Vector2 look =
                Vector2.ClampMagnitude(rawLook, 50f) * lookSensitivity;
            transform.Rotate(Vector3.up, look.x);

            _pitch = Mathf.Clamp(_pitch - look.y, -85f, 85f);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateMovement()
        {
            Vector2 move = _moveAction.ReadValue<Vector2>();
            Vector3 planarVelocity =
                (transform.right * move.x + transform.forward * move.y) * moveSpeed;

            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            _verticalVelocity += gravity * Time.deltaTime;
            planarVelocity.y = _verticalVelocity;
            _characterController.Move(planarVelocity * Time.deltaTime);
        }
    }
}
