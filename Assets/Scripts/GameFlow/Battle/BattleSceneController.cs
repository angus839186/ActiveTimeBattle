using UnityEngine;

public class BattleSceneController : MonoBehaviour
{
    public BattleSession CurrentBattleSession { get; private set; }
    private void Start()
    {
        CurrentBattleSession = new BattleSession(playerHp: 100, enemyHp: 50);
        Debug.Log("BattleSession created. Player HP: 100, Enemy HP: 50");
        GameFlowController gameFlow = GameFlowController.Instance;

        if (gameFlow == null)
        {
            Debug.LogWarning("BattleSceneController: GameFlowController not found.");
            return;
        }

        RunSession runSession = gameFlow.CurrentRunSession;

        if (runSession == null || !runSession.IsActive)
        {
            Debug.LogWarning("BattleSceneController: No active run session.");
            return;
        }

        Debug.Log($"Battle started. Class: {runSession.SelectedClassId}, Seed: {runSession.Seed}");
    }
    public void ReturnToExplore()
    {
        if (GameFlowController.Instance == null)
        {
            Debug.LogWarning("BattleSceneController: GameFlowController not found.");
            return;
        }

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

        if (CurrentBattleSession.Result == BattleResult.Victory)
        {
            GameFlowController.Instance.CurrentRunSession.RecordBattleVictory();
            ReturnToExplore();
        }
    }
}