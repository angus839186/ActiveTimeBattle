using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Enemy Definition")]
public class BattleEnemyDefinition : ScriptableObject
{
    [SerializeField] private string enemyName;
    [SerializeField] private int maxHp = 50;
    [SerializeField] private int attackPower = 10;
    [SerializeField] private float attackCooldown = 5f;

    public string EnemyName => enemyName;
    public int MaxHp => maxHp;
    public int AttackPower => attackPower;
    public float AttackCooldown => attackCooldown;
}