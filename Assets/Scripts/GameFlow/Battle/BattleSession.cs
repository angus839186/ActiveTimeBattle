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

    public BattleSession(int playerHp, BattleEnemyDefinition enemyDefinition)
    {
        PlayerHp = playerHp;
        EnemyDefinition = enemyDefinition;
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
}