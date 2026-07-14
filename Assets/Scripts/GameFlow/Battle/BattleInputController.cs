using UnityEngine;
using UnityEngine.InputSystem;

public class BattleInputController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private BattleSceneController battleSceneController;

    private InputAction skillQAction;
    private InputAction skillWAction;
    private InputAction skillEAction;
    private InputAction skillRAction;
    private InputAction defenseAction;

    private void Awake()
    {
        if (battleSceneController == null)
        {
            battleSceneController = FindFirstObjectByType<BattleSceneController>();
        }
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        skillQAction = playerInput.actions["SkillQ"];
        skillWAction = playerInput.actions["SkillW"];
        skillEAction = playerInput.actions["SkillE"];
        skillRAction = playerInput.actions["SkillR"];
        defenseAction = playerInput.actions["Defense"];
    }

    private void OnEnable()
    {
        skillQAction.performed += OnSkillQ;
        skillWAction.performed += OnSkillW;
        skillEAction.performed += OnSkillE;
        skillRAction.performed += OnSkillR;
        defenseAction.performed += OnDefense;
    }

    private void OnDisable()
    {
        skillQAction.performed -= OnSkillQ;
        skillWAction.performed -= OnSkillW;
        skillEAction.performed -= OnSkillE;
        skillRAction.performed -= OnSkillR;
        defenseAction.performed -= OnDefense;
    }

    private void OnSkillQ(InputAction.CallbackContext context) => battleSceneController.DealDamageToEnemy(10);
    private void OnSkillW(InputAction.CallbackContext context) => battleSceneController.DealDamageToEnemy(15);
    private void OnSkillE(InputAction.CallbackContext context) => battleSceneController.DealDamageToEnemy(20);
    private void OnSkillR(InputAction.CallbackContext context) => battleSceneController.DealDamageToEnemy(30);
    private void OnDefense(InputAction.CallbackContext context) => Debug.Log("Battle Defense");
}