using UnityEngine;
using System;

public class BattleSceneController : MonoBehaviour
{
    [SerializeField] private BattleEnemyDefinition testEnemyDefinition;
    [SerializeField] private float defenseQTETimeLimit = 1f;
    [SerializeField] private float perfectGuardTime = 0.3f;
    [SerializeField] private BattleSkillDefinition skillQ;
    [SerializeField] private BattleSkillDefinition skillW;
    [SerializeField] private BattleSkillDefinition skillE;
    [SerializeField] private BattleSkillDefinition skillR;
    [SerializeField] private BattleEnemyDefinition eliteEnemyDefinition;

    public event Action DefenseQTEStarted;

    public bool CanAcceptSkillInput =>
    CurrentBattleSession != null &&
    !CurrentBattleSession.IsFinished &&
    !isDefenseQTEActive;
    public bool IsDefenseQTEActive => isDefenseQTEActive;
    public string CurrentInputText { get; private set; } = string.Empty;

    private bool isDefenseQTEActive;
    private float defenseQTETimer;
    public BattleSession CurrentBattleSession { get; private set; }

    public float DefenseQTERemaining =>
    isDefenseQTEActive
        ? Mathf.Max(0f, defenseQTETimeLimit - defenseQTETimer)
        : 0f;

    public float DefenseQTERemainingRate =>
        isDefenseQTEActive && defenseQTETimeLimit > 0f
            ? 1f - Mathf.Clamp01(defenseQTETimer / defenseQTETimeLimit)
            : 0f;

    public bool IsPerfectGuardWindow =>
        isDefenseQTEActive && defenseQTETimer <= perfectGuardTime;

    public DefenseQTEResult? LastDefenseQTEResult { get; private set; }

    private void Start()
    {
        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;
        int playerHp = runSession != null ? runSession.PlayerCurrentHp : 100;

        BattleEnemyDefinition enemyDefinition = GetEnemyDefinition(runSession);
        CurrentBattleSession = new BattleSession(playerHp, enemyDefinition);
        GameFlowController gameFlow = GameFlowController.Instance;

        if (gameFlow == null) return;

        if (runSession == null || !runSession.IsActive) return;

        Debug.Log($"Battle started. Class: {runSession.SelectedClassId}, Seed: {runSession.Seed}");
    }
    private void Update()
    {
        if (CurrentBattleSession == null || CurrentBattleSession.IsFinished)
        {
            return;
        }

        if (isDefenseQTEActive)
        {
            TickDefenseQTE(Time.deltaTime);
            return;
        }

        CurrentBattleSession.TickEnemyAttackCooldown(Time.deltaTime);

        if (CurrentBattleSession.IsEnemyAttackReady)
        {
            StartDefenseQTE();
        }
    }
    private BattleEnemyDefinition GetEnemyDefinition(RunSession runSession)
    {
        if (runSession != null &&
            runSession.PendingBattleType == BattleType.Elite &&
            eliteEnemyDefinition != null)
        {
            return eliteEnemyDefinition;
        }

        return testEnemyDefinition;
    }
    private void ResolveEnemyAttack(DefenseQTEResult result)
    {
        CurrentBattleSession.ApplyEnemyAttack(result);
        SavePlayerHpToRunSession();

        if (CurrentBattleSession.Result == BattleResult.Defeat)
        {
            Debug.Log("Battle Defeat");
            HandleDefeat();
        }
    }
    private void SavePlayerHpToRunSession()
    {
        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;

        if (runSession != null)
        {
            runSession.SetPlayerHp(CurrentBattleSession.PlayerHp);
        }
    }
    public void ReturnToExplore()
    {
        if (GameFlowController.Instance == null)
        {
            Debug.LogWarning("BattleSceneController: GameFlowController not found.");
            return;
        }

        SavePlayerHpToRunSession();
        GameFlowController.Instance.ChangeToExplore();
    }

    public void DealDamageToEnemy(int damage)
    {
        if (CurrentBattleSession == null)
        {
            Debug.LogWarning("BattleSceneController: BattleSession is not ready.");
            return;
        }

        CurrentBattleSession.DealDamageToEnemy(damage);

        SavePlayerHpToRunSession();

        if (CurrentBattleSession.Result == BattleResult.Victory)
        {
            GameFlowController.Instance.CurrentRunSession.CompletePendingBattle();
            ReturnToExplore();
        }
    }
    private void StartDefenseQTE()
    {
        isDefenseQTEActive = true;
        defenseQTETimer = 0f;
        LastDefenseQTEResult = null;
        DefenseQTEStarted?.Invoke();

        Debug.Log("Defense QTE Start");
    }

    private void TickDefenseQTE(float deltaTime)
    {
        defenseQTETimer += deltaTime;

        if (defenseQTETimer >= defenseQTETimeLimit)
        {
            SubmitDefenseQTE(DefenseQTEResult.Fail);
        }
    }
    public void TryDefense()
    {
        if (!isDefenseQTEActive)
        {
            Debug.Log("Defense input ignored.");
            return;
        }

        DefenseQTEResult result = defenseQTETimer <= perfectGuardTime
            ? DefenseQTEResult.PerfectGuard
            : DefenseQTEResult.Guard;

        SubmitDefenseQTE(result);
    }

    private void SubmitDefenseQTE(DefenseQTEResult result)
    {
        isDefenseQTEActive = false;
        LastDefenseQTEResult = result;

        Debug.Log($"Defense QTE Result: {result}");
        ResolveEnemyAttack(result);
    }
    public BattleSkillDefinition GetSkill(BattleSkillSlot slot)
    {
        RunSession runSession =
            GameFlowController.Instance?.CurrentRunSession;

        if (runSession != null && runSession.IsActive)
        {
            return runSession.SkillLoadout.GetEquippedSkill(slot);
        }

        return GetTestSkill(slot);
    }

    private BattleSkillDefinition GetTestSkill(BattleSkillSlot slot)
    {
        switch (slot)
        {
            case BattleSkillSlot.Q:
                return skillQ;
            case BattleSkillSlot.W:
                return skillW;
            case BattleSkillSlot.E:
                return skillE;
            case BattleSkillSlot.R:
                return skillR;
            default:
                return null;
        }
    }
    public void SetCurrentInputText(string inputText)
    {
        CurrentInputText = inputText;
    }
    public void UseSkill(BattleSkillDefinition skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("BattleSceneController: Skill is null.");
            return;
        }

        DealDamageToEnemy(skill.Power);
    }
    private void HandleDefeat()
    {
        GameFlowController gameFlow = GameFlowController.Instance;

        if (gameFlow == null)
        {
            Debug.LogWarning("BattleSceneController: GameFlowController not found.");
            return;
        }

        RunSession runSession = gameFlow.CurrentRunSession;

        if (runSession != null)
        {
            runSession.EndRun();
        }

        gameFlow.ChangeToLobby();
    }
}