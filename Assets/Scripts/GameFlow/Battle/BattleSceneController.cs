using UnityEngine;

public class BattleSceneController : MonoBehaviour
{
    [SerializeField] private BattleEnemyDefinition testEnemyDefinition;
    [SerializeField] private int basePlayerHp = 100;
    public BattleSession CurrentBattleSession { get; private set; }
    private void Start()
    {
        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;
        int playerHp = runSession != null ? runSession.PlayerCurrentHp : 100;

        CurrentBattleSession = new BattleSession(playerHp, testEnemyDefinition);
        GameFlowController gameFlow = GameFlowController.Instance;

        if (gameFlow == null) return;

        if (runSession == null || !runSession.IsActive) return;

        Debug.Log($"Battle started. Class: {runSession.SelectedClassId}, Seed: {runSession.Seed}");
    }
    public void ReturnToExplore()
    {
        if (GameFlowController.Instance == null)
        {
            Debug.LogWarning("BattleSceneController: GameFlowController not found.");
            return;
        }

        GameFlowController.Instance.CurrentRunSession.SetPlayerHp(CurrentBattleSession.PlayerHp);
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
            GameFlowController.Instance.CurrentRunSession.CompletePendingBattle();
            ReturnToExplore();
        }
    }
}