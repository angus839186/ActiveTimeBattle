using System;
using System.Collections.Generic;
using System.Linq;
using ActiveTimeBattle.Application.Combat;
using ActiveTimeBattle.Domain;
using ActiveTimeBattle.Domain.Combat;
using ActiveTimeBattle.Domain.Exploration;
using ActiveTimeBattle.Presentation.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveTimeBattle.Presentation.Combat
{
    public sealed class CombatController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private SkillData skillQ;
        [SerializeField] private SkillData skillW;
        [SerializeField] private SkillData skillE;
        [SerializeField] private SkillData ultimateR;
        [SerializeField] private CombatHudView hudView;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private Renderer enemyRenderer;
        [SerializeField, Min(1)] private int playerHealth = 100;
        [SerializeField, Min(1)] private int enemyHealth = 120;
        [SerializeField, Min(0.1f)] private float enemyActionDuration = 4f;
        [SerializeField, Min(1)] private int enemyDamage = 24;
        [SerializeField, Min(0.1f)] private float qteDuration = 1.2f;
        [SerializeField, Min(0.05f)] private float normalWindow = 0.8f;
        [SerializeField, Min(0.05f)] private float perfectWindow = 0.3f;

        private readonly List<CombatDirection> _recentDirections =
            new List<CombatDirection>();
        private readonly Dictionary<InputAction, CombatDirection> _directionActions =
            new Dictionary<InputAction, CombatDirection>();
        private readonly Dictionary<InputAction, string> _skillActions =
            new Dictionary<InputAction, string>();
        private CombatSession _session;
        private InputAction _defenseAction;
        private bool _isEnding;

        private void Awake()
        {
            if (GameBootstrapper.Instance?
                    .RunFlowController
                    .CurrentSession?
                    .CurrentNode
                    .Type == ExplorationNodeType.Boss)
            {
                enemyHealth = 220;
                enemyDamage = 32;
                enemyActionDuration = 3f;
            }

            InputActionAsset actions = inputActions != null
                ? inputActions
                : InputSystem.actions;
            InputActionMap combatMap = actions.FindActionMap("Combat", true);

            RegisterDirection(
                combatMap.FindAction("DirectionUp", true),
                CombatDirection.Up);
            RegisterDirection(
                combatMap.FindAction("DirectionDown", true),
                CombatDirection.Down);
            RegisterDirection(
                combatMap.FindAction("DirectionLeft", true),
                CombatDirection.Left);
            RegisterDirection(
                combatMap.FindAction("DirectionRight", true),
                CombatDirection.Right);
            RegisterSkill(combatMap.FindAction("SkillQ", true), "q");
            RegisterSkill(combatMap.FindAction("SkillW", true), "w");
            RegisterSkill(combatMap.FindAction("SkillE", true), "e");
            RegisterSkill(combatMap.FindAction("UltimateR", true), "r");
            _defenseAction = combatMap.FindAction("Defense", true);

            SkillData[] configuredSkills =
                { skillQ, skillW, skillE, ultimateR };
            if (configuredSkills.Any(data => data == null))
            {
                throw new InvalidOperationException(
                    "CombatController requires four valid SkillData references.");
            }

            RunSession runSession = GameBootstrapper.Instance?
                .RunFlowController
                .CurrentSession;
            SkillRuntime[] runtimes = configuredSkills
                .Where(
                    data => runSession == null
                        || runSession.HasUnlockedSkill(data.SkillId))
                .Select(data => new SkillRuntime(data.CreateDefinition()))
                .ToArray();
            DefenseQteWindow qte = new DefenseQteWindow(
                qteDuration,
                normalWindow,
                perfectWindow);
            CombatClock clock = new CombatClock(
                new EnemyActionTimer(enemyActionDuration),
                qte,
                runtimes);
            _session = new CombatSession(
                runSession?.Player ?? new CharacterRuntime(playerHealth),
                new CharacterRuntime(enemyHealth),
                runtimes,
                new DirectionInputBuffer(8, 3f),
                clock,
                enemyDamage);
        }

        private void OnEnable()
        {
            foreach (InputAction action in _directionActions.Keys)
            {
                action.performed += OnDirectionPerformed;
            }

            foreach (InputAction action in _skillActions.Keys)
            {
                action.performed += OnSkillPerformed;
            }

            _defenseAction.performed += OnDefensePerformed;
        }

        private void OnDisable()
        {
            foreach (InputAction action in _directionActions.Keys)
            {
                action.performed -= OnDirectionPerformed;
            }

            foreach (InputAction action in _skillActions.Keys)
            {
                action.performed -= OnSkillPerformed;
            }

            _defenseAction.performed -= OnDefensePerformed;
        }

        private void Update()
        {
            _session.Tick(Time.deltaTime);
            UpdateCharacterColors();
            hudView.Refresh(_session, _recentDirections);

            if (!_isEnding
                && (_session.Phase == CombatPhase.Victory
                    || _session.Phase == CombatPhase.Defeat))
            {
                CompleteCombatAsync(_session.Phase).Forget();
            }
        }

        private void RegisterDirection(
            InputAction action,
            CombatDirection direction)
        {
            _directionActions.Add(action, direction);
        }

        private void RegisterSkill(InputAction action, string skillId)
        {
            _skillActions.Add(action, skillId);
        }

        private void OnDirectionPerformed(InputAction.CallbackContext context)
        {
            CombatDirection direction = _directionActions[context.action];
            if (!_session.AddDirection(direction, Time.unscaledTime))
            {
                _recentDirections.Clear();
                return;
            }

            _recentDirections.Add(direction);
            if (_recentDirections.Count > 8)
            {
                _recentDirections.RemoveAt(0);
            }
        }

        private void OnSkillPerformed(InputAction.CallbackContext context)
        {
            if (_session.TryUseSkill(
                    _skillActions[context.action],
                    Time.unscaledTime))
            {
                _recentDirections.Clear();
                return;
            }

            _recentDirections.Clear();
        }

        private void OnDefensePerformed(InputAction.CallbackContext context)
        {
            _session.TryDefend();
        }

        private void UpdateCharacterColors()
        {
            if (playerRenderer != null)
            {
                playerRenderer.material.color =
                    _session.Phase == CombatPhase.DefenseQte
                        ? new Color(0.2f, 0.75f, 1f)
                        : new Color(0.2f, 0.45f, 0.9f);
            }

            enemyRenderer.material.color = _session.Phase == CombatPhase.SkillExecution
                ? new Color(1f, 0.35f, 0.2f)
                : new Color(0.65f, 0.12f, 0.16f);
        }

        private async UniTaskVoid CompleteCombatAsync(CombatPhase result)
        {
            _isEnding = true;
            await UniTask.Delay(
                TimeSpan.FromSeconds(1.5f),
                cancellationToken: destroyCancellationToken);

            if (GameBootstrapper.Instance == null)
            {
                return;
            }

            if (result == CombatPhase.Victory)
            {
                await GameBootstrapper.Instance.RunFlowController
                    .ReturnToExplorationAsync(
                        GameBootstrapper.Instance.LifetimeToken);
            }
            else
            {
                await GameBootstrapper.Instance.RunFlowController
                    .EnterHubAsync(GameBootstrapper.Instance.LifetimeToken);
            }
        }
    }
}
