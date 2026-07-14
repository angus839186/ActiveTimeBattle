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

    public BattleSession(int playerHp, int enemyHp)
    {
        PlayerHp = playerHp;
        EnemyHp = enemyHp;
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