using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Skill Definition")]
public class BattleSkillDefinition : ScriptableObject
{
    [SerializeField] private string skillName;
    [SerializeField] private int power = 10;
    [SerializeField] private BattleDirection[] directionSequence;

    public string SkillName => skillName;
    public int Power => power;
    public BattleDirection[] DirectionSequence => directionSequence;
}