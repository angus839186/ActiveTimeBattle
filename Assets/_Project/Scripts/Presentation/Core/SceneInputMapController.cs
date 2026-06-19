using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveTimeBattle.Presentation.Core
{
    public sealed class SceneInputMapController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Hub";

        private InputActionMap _activeMap;

        private void OnEnable()
        {
            InputActionAsset actions = inputActions != null
                ? inputActions
                : InputSystem.actions;
            if (actions == null)
            {
                Debug.LogError("找不到 Project-wide Input Action Asset。", this);
                return;
            }

            actions.Disable();
            _activeMap = actions.FindActionMap(actionMapName, true);
            _activeMap.Enable();
        }

        private void OnDisable()
        {
            _activeMap?.Disable();
        }
    }
}
