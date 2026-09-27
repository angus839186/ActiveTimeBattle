using UnityEngine;
public enum BattleResult
{
    None,
    Victory,
    Defeat
}
public class BattleSession
{
    public int PlayerHp { get; private set; }
    public int EnemyHp { get; private set; }
    public bool IsFinished => EnemyHp <= 0 || PlayerHp <= 0;

    public BattleEnemyDefinition EnemyDefinition { get; }
    public string EnemyName => EnemyDefinition != null ? EnemyDefinition.EnemyName : "Unknown Enemy";
    public int EnemyAttackPower => EnemyDefinition != null ? EnemyDefinition.AttackPower : 0;
    public float EnemyAttackCooldown => EnemyDefinition != null ? EnemyDefinition.AttackCooldown : 0f;

    public float EnemyAttackCooldownRemaining { get; private set; }
    public float EnemyAttackCooldownRate => EnemyAttackCooldown > 0f
        ? EnemyAttackCooldownRemaining / EnemyAttackCooldown
        : 0f;

    public bool IsEnemyAttackReady => EnemyAttackCooldownRemaining <= 0f;

    public BattleSession(int playerHp, BattleEnemyDefinition enemyDefinition)
    {
        PlayerHp = playerHp;
        EnemyDefinition = enemyDefinition;
        EnemyAttackCooldownRemaining = EnemyAttackCooldown;
        EnemyHp = enemyDefinition != null ? enemyDefinition.MaxHp : 1;
    }

    public void DealDamageToEnemy(int damage)
    {
        EnemyHp = Mathf.Max(0, EnemyHp - damage);
        Debug.Log($"Enemy HP: {EnemyHp}");
    }
    public BattleResult Result
    {
        get
        {
            if (EnemyHp <= 0)
            {
                return BattleResult.Victory;
            }

            if (PlayerHp <= 0)
            {
                return BattleResult.Defeat;
            }

            return BattleResult.None;
        }
    }
    public void TickEnemyAttackCooldown(float deltaTime)
    {
        if (IsFinished)
        {
            return;
        }

        EnemyAttackCooldownRemaining = Mathf.Max(0f, EnemyAttackCooldownRemaining - deltaTime);
    }

    public void ResetEnemyAttackCooldown()
    {
        EnemyAttackCooldownRemaining = EnemyAttackCooldown;
    }

    public void TakeDamageToPlayer(int damage)
    {
        PlayerHp = Mathf.Max(0, PlayerHp - damage);
        Debug.Log($"Player HP: {PlayerHp}");
    }

    public void ApplyEnemyAttack(DefenseQTEResult qteResult)
    {
        int damage = EnemyAttackPower;

        if (qteResult == DefenseQTEResult.PerfectGuard)
        {
            damage = 0;
        }
        else if (qteResult == DefenseQTEResult.Guard)
        {
            damage = Mathf.CeilToInt(EnemyAttackPower * 0.5f);
        }

        TakeDamageToPlayer(damage);
        ResetEnemyAttackCooldown();
    }
}