using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BattleInputController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private BattleSceneController battleSceneController;


    private InputAction skillQAction;
    private InputAction skillWAction;
    private InputAction skillEAction;
    private InputAction skillRAction;
    private InputAction defenseAction;
    private InputAction directionUpAction;
    private InputAction directionDownAction;
    private InputAction directionLeftAction;
    private InputAction directionRightAction;

    private readonly List<BattleDirection> directionBuffer = new List<BattleDirection>();

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
        directionUpAction = playerInput.actions["DirectionUp"];
        directionDownAction = playerInput.actions["DirectionDown"];
        directionLeftAction = playerInput.actions["DirectionLeft"];
        directionRightAction = playerInput.actions["DirectionRight"];
        skillQAction = playerInput.actions["SkillQ"];
        skillWAction = playerInput.actions["SkillW"];
        skillEAction = playerInput.actions["SkillE"];
        skillRAction = playerInput.actions["SkillR"];
        defenseAction = playerInput.actions["Defense"];
    }

    private void OnEnable()
    {
        directionUpAction.performed += OnDirectionUp;
        directionDownAction.performed += OnDirectionDown;
        directionLeftAction.performed += OnDirectionLeft;
        directionRightAction.performed += OnDirectionRight;
        skillQAction.performed += OnSkillQ;
        skillWAction.performed += OnSkillW;
        skillEAction.performed += OnSkillE;
        skillRAction.performed += OnSkillR;
        defenseAction.performed += OnDefense;
        battleSceneController.DefenseQTEStarted += ClearDirectionBuffer;
    }

    private void OnDisable()
    {
        directionUpAction.performed -= OnDirectionUp;
        directionDownAction.performed -= OnDirectionDown;
        directionLeftAction.performed -= OnDirectionLeft;
        directionRightAction.performed -= OnDirectionRight;
        skillQAction.performed -= OnSkillQ;
        skillWAction.performed -= OnSkillW;
        skillEAction.performed -= OnSkillE;
        skillRAction.performed -= OnSkillR;
        defenseAction.performed -= OnDefense;
        battleSceneController.DefenseQTEStarted -= ClearDirectionBuffer;
    }

    private void OnSkillQ(InputAction.CallbackContext context) => TryUseSkill(BattleSkillSlot.Q);
    private void OnSkillW(InputAction.CallbackContext context) => TryUseSkill(BattleSkillSlot.W);
    private void OnSkillE(InputAction.CallbackContext context) => TryUseSkill(BattleSkillSlot.E);
    private void OnSkillR(InputAction.CallbackContext context) => TryUseSkill(BattleSkillSlot.R);
    private void OnDefense(InputAction.CallbackContext context) => battleSceneController.TryDefense();

    private void OnDirectionUp(InputAction.CallbackContext context) => AddDirection(BattleDirection.Up);
    private void OnDirectionDown(InputAction.CallbackContext context) => AddDirection(BattleDirection.Down);
    private void OnDirectionLeft(InputAction.CallbackContext context) => AddDirection(BattleDirection.Left);
    private void OnDirectionRight(InputAction.CallbackContext context) => AddDirection(BattleDirection.Right);

    private void AddDirection(BattleDirection direction)
    {
        if (!battleSceneController.CanAcceptSkillInput)
        {
            return;
        }

        directionBuffer.Add(direction);

        if (!CanMatchAnyAssignedSkill())
        {
            Debug.Log($"Invalid direction sequence: {GetInputBufferText()}");
            ClearDirectionBuffer();
            return;
        }

        battleSceneController.SetCurrentInputText(GetInputBufferText());
        Debug.Log($"Input Buffer: {GetInputBufferText()}");
    }

    private void TryUseSkill(BattleSkillSlot slot)
    {
        if (!battleSceneController.CanAcceptSkillInput)
        {
            ClearDirectionBuffer();
            return;
        }
        BattleSkillDefinition skill = battleSceneController.GetSkill(slot);
        string fullInputText = directionBuffer.Count > 0
    ? $"{GetInputBufferText()}+{slot}"
    : slot.ToString();

        if (skill == null)
        {
            Debug.LogWarning($"No skill assigned to slot: {slot}");
            ClearDirectionBuffer();
            return;
        }

        if (!IsDirectionSequenceMatched(skill.DirectionSequence))
        {
            Debug.Log($"Wrong combo: {fullInputText}");
            ClearDirectionBuffer();
            return;
        }

        Debug.Log($"Use Skill: {skill.SkillName}, Input: {fullInputText}");
        battleSceneController.UseSkill(skill);
        ClearDirectionBuffer();
    }
    private bool IsDirectionSequenceMatched(BattleDirection[] requiredSequence)
    {
        if (requiredSequence == null || requiredSequence.Length != directionBuffer.Count)
        {
            return false;
        }

        for (int i = 0; i < requiredSequence.Length; i++)
        {
            if (requiredSequence[i] != directionBuffer[i])
            {
                return false;
            }
        }

        return true;
    }
    private string GetInputBufferText()
    {
        if (directionBuffer.Count == 0)
        {
            return string.Empty;
        }

        List<string> texts = new List<string>();

        foreach (BattleDirection direction in directionBuffer)
        {
            texts.Add(GetDirectionText(direction));
        }

        return string.Join("+", texts);
    }

    private string GetDirectionText(BattleDirection direction)
    {
        switch (direction)
        {
            case BattleDirection.Up:
                return "↑";
            case BattleDirection.Down:
                return "↓";
            case BattleDirection.Left:
                return "←";
            case BattleDirection.Right:
                return "→";
            default:
                return "?";
        }
    }
    private bool CanMatchAnyAssignedSkill()
    {
        return IsDirectionPrefix(battleSceneController.GetSkill(BattleSkillSlot.Q)) ||
               IsDirectionPrefix(battleSceneController.GetSkill(BattleSkillSlot.W)) ||
               IsDirectionPrefix(battleSceneController.GetSkill(BattleSkillSlot.E)) ||
               IsDirectionPrefix(battleSceneController.GetSkill(BattleSkillSlot.R));
    }

    private bool IsDirectionPrefix(BattleSkillDefinition skill)
    {
        if (skill == null || skill.DirectionSequence == null)
        {
            return false;
        }

        BattleDirection[] requiredSequence = skill.DirectionSequence;

        if (directionBuffer.Count > requiredSequence.Length)
        {
            return false;
        }

        for (int i = 0; i < directionBuffer.Count; i++)
        {
            if (directionBuffer[i] != requiredSequence[i])
            {
                return false;
            }
        }

        return true;
    }
    private void ClearDirectionBuffer()
    {
        directionBuffer.Clear();
        battleSceneController.SetCurrentInputText(string.Empty);
    }
}