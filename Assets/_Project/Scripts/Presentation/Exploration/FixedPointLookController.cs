using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveTimeBattle.Presentation.Exploration
{
    public sealed class FixedPointLookController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField, Min(0.01f)] private float sensitivity = 0.08f;
        [SerializeField] private float minimumYaw = -45f;
        [SerializeField] private float maximumYaw = 45f;
        [SerializeField] private float minimumPitch = -20f;
        [SerializeField] private float maximumPitch = 25f;

        private InputAction _lookAction;
        private float _yaw;
        private float _pitch;

        private void Awake()
        {
            InputActionAsset actions = inputActions != null
                ? inputActions
                : InputSystem.actions;
            _lookAction = actions.FindAction("Exploration/Look", true);
            Vector3 initialRotation = transform.localEulerAngles;
            _yaw = NormalizeAngle(initialRotation.y);
            _pitch = NormalizeAngle(initialRotation.x);
        }

        private void OnEnable()
        {
            _lookAction.performed += OnLookPerformed;
        }

        private void OnDisable()
        {
            _lookAction.performed -= OnLookPerformed;
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            Vector2 delta = context.ReadValue<Vector2>() * sensitivity;
            _yaw = Mathf.Clamp(_yaw + delta.x, minimumYaw, maximumYaw);
            _pitch = Mathf.Clamp(
                _pitch - delta.y,
                minimumPitch,
                maximumPitch);
            transform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
