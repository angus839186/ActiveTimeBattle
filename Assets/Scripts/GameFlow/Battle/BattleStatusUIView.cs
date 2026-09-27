using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleStatusUIView : MonoBehaviour
{
    [SerializeField] private BattleSceneController battleSceneController;

    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private Image playerHpFillImage;

    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private TMP_Text enemyHpText;
    [SerializeField] private Image enemyHpFillImage;

    [SerializeField] private TMP_Text enemyAttackCooldownText;
    [SerializeField] private Image enemyAttackCooldownFillImage;
    [SerializeField] private Image defenseQTEFillImage;

    [SerializeField] private TMP_Text inputBufferText;
    [SerializeField] private TMP_Text defenseQTEText;

    private void Update()
    {
        if (battleSceneController == null)
        {
            battleSceneController = FindFirstObjectByType<BattleSceneController>();
        }

        BattleSession session = battleSceneController != null
            ? battleSceneController.CurrentBattleSession
            : null;

        if (session == null)
        {
            SetEmpty();
            return;
        }

        playerHpText.text = $"Player HP: {session.PlayerHp}";
        playerHpFillImage.fillAmount = GetHpRate(session.PlayerHp, GameFlowController.Instance?.CurrentRunSession?.PlayerMaxHp ?? 100);

        enemyNameText.text = $"Enemy: {session.EnemyName}";
        enemyHpText.text = $"Enemy HP: {session.EnemyHp}";
        enemyHpFillImage.fillAmount = GetHpRate(session.EnemyHp, session.EnemyDefinition != null ? session.EnemyDefinition.MaxHp : 1);

        enemyAttackCooldownText.text = $"Enemy CD: {session.EnemyAttackCooldownRemaining:0.0}";
        enemyAttackCooldownFillImage.fillAmount = 1f - session.EnemyAttackCooldownRate;

        inputBufferText.text = battleSceneController.CurrentInputText;
        UpdateDefenseQTE();
    }

    private float GetHpRate(int currentHp, int maxHp)
    {
        return maxHp > 0 ? (float)currentHp / maxHp : 0f;
    }

    private void SetEmpty()
    {
        playerHpText.text = "Player HP: -";
        playerHpFillImage.fillAmount = 0f;

        enemyNameText.text = "Enemy: -";
        enemyHpText.text = "Enemy HP: -";
        enemyHpFillImage.fillAmount = 0f;

        enemyAttackCooldownText.text = "Enemy CD: -";
        enemyAttackCooldownFillImage.fillAmount = 0f;
        defenseQTEFillImage.fillAmount = 0f;

        inputBufferText.text = string.Empty;
        defenseQTEText.text = string.Empty;
    }
    private void UpdateDefenseQTE()
    {
        if (battleSceneController.IsDefenseQTEActive)
        {
            string windowText = battleSceneController.IsPerfectGuardWindow
                ? "Perfect"
                : "Guard";

            defenseQTEText.text =
                $"Defense: {battleSceneController.DefenseQTERemaining:0.00} ({windowText})";

            defenseQTEFillImage.fillAmount =
                battleSceneController.DefenseQTERemainingRate;

            return;
        }

        defenseQTEFillImage.fillAmount = 0f;

        defenseQTEText.text = battleSceneController.LastDefenseQTEResult.HasValue
            ? $"Result: {battleSceneController.LastDefenseQTEResult.Value}"
            : string.Empty;
    }
}