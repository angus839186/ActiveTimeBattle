using System;
using System.Collections.Generic;
using System.Threading;
using ActiveTimeBattle.Domain.Narrative;
using ActiveTimeBattle.Presentation.Core;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveTimeBattle.Presentation.Narrative
{
    public sealed class DialogueRunner : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text choiceText;

        private InputActionMap _dialogueMap;
        private InputAction _advanceAction;
        private InputAction _selectChoiceAction;
        private InputAction _cancelAction;
        private bool _advanceRequested;
        private bool _cancelRequested;
        private int _choiceDelta;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            InputActionAsset actions = inputActions != null
                ? inputActions
                : InputSystem.actions;
            _dialogueMap = actions.FindActionMap("Dialogue", true);
            _advanceAction = _dialogueMap.FindAction("Advance", true);
            _selectChoiceAction =
                _dialogueMap.FindAction("SelectChoice", true);
            _cancelAction = _dialogueMap.FindAction("Cancel", true);
            panel.SetActive(false);
        }

        public async UniTask PlayAsync(
            DialogueData dialogue,
            CancellationToken cancellationToken)
        {
            if (_isPlaying || dialogue == null || dialogue.Lines.Length == 0)
            {
                return;
            }

            _isPlaying = true;
            InputActionAsset actions = inputActions != null
                ? inputActions
                : InputSystem.actions;
            List<InputActionMap> previouslyEnabled = new List<InputActionMap>();
            foreach (InputActionMap map in actions.actionMaps)
            {
                if (map.enabled)
                {
                    previouslyEnabled.Add(map);
                }
            }

            using CancellationTokenSource linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    destroyCancellationToken);

            try
            {
                actions.Disable();
                _dialogueMap.Enable();
                Subscribe();
                panel.SetActive(true);

                foreach (DialogueLineData line in dialogue.Lines)
                {
                    bool shouldContinue = await ShowLineAsync(
                        line,
                        linkedCancellation.Token);
                    if (!shouldContinue)
                    {
                        break;
                    }
                }
            }
            finally
            {
                Unsubscribe();
                panel.SetActive(false);
                _dialogueMap.Disable();
                foreach (InputActionMap map in previouslyEnabled)
                {
                    map.Enable();
                }

                _isPlaying = false;
            }
        }

        private async UniTask<bool> ShowLineAsync(
            DialogueLineData line,
            CancellationToken cancellationToken)
        {
            _advanceRequested = false;
            _cancelRequested = false;
            _choiceDelta = 0;
            int selectedChoice = 0;
            speakerText.text = line.Speaker;
            bodyText.text = line.Body;
            RefreshChoices(line, selectedChoice);

            while (!_advanceRequested && !_cancelRequested)
            {
                if (_choiceDelta != 0 && line.Choices.Length > 0)
                {
                    selectedChoice =
                        (selectedChoice + _choiceDelta + line.Choices.Length)
                        % line.Choices.Length;
                    _choiceDelta = 0;
                    RefreshChoices(line, selectedChoice);
                }

                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    cancellationToken);
            }

            if (_cancelRequested)
            {
                return false;
            }

            if (line.Choices.Length > 0
                && GameBootstrapper.Instance != null)
            {
                GameBootstrapper.Instance.NarrativeContext.SetFlag(
                    line.Choices[selectedChoice].SetFlag);
            }

            return true;
        }

        private void RefreshChoices(
            DialogueLineData line,
            int selectedChoice)
        {
            if (line.Choices.Length == 0)
            {
                choiceText.text = "Enter / Space 繼續";
                return;
            }

            string[] choices = new string[line.Choices.Length];
            for (int index = 0; index < choices.Length; index++)
            {
                choices[index] =
                    $"{(index == selectedChoice ? ">" : " ")} "
                    + line.Choices[index].Text;
            }

            choiceText.text = string.Join("\n", choices);
        }

        private void Subscribe()
        {
            _advanceAction.performed += OnAdvance;
            _selectChoiceAction.performed += OnSelectChoice;
            _cancelAction.performed += OnCancel;
        }

        private void Unsubscribe()
        {
            _advanceAction.performed -= OnAdvance;
            _selectChoiceAction.performed -= OnSelectChoice;
            _cancelAction.performed -= OnCancel;
        }

        private void OnAdvance(InputAction.CallbackContext context)
        {
            _advanceRequested = true;
        }

        private void OnSelectChoice(InputAction.CallbackContext context)
        {
            float vertical = context.ReadValue<Vector2>().y;
            if (!Mathf.Approximately(vertical, 0f))
            {
                _choiceDelta = vertical > 0f ? -1 : 1;
            }
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            _cancelRequested = true;
        }
    }
}
