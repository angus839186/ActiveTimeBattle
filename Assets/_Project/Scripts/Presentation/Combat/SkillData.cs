using ActiveTimeBattle.Domain.Combat;
using UnityEngine;

namespace ActiveTimeBattle.Presentation.Combat
{
    [CreateAssetMenu(
        fileName = "SkillData",
        menuName = "Active Time Battle/Combat/Skill")]
    public sealed class SkillData : ScriptableObject
    {
        [SerializeField] private string skillId = "skill";
        [SerializeField] private string displayName = "技能";
        [SerializeField] private CombatDirection[] command =
        {
            CombatDirection.Up
        };
        [SerializeField, Min(0)] private int damage = 10;
        [SerializeField, Min(0f)] private float cooldownDuration = 3f;
        [SerializeField, Min(0f)] private float executionDuration = 0.4f;

        public string SkillId => skillId;

        public string DisplayName => displayName;

        public SkillDefinition CreateDefinition()
        {
            return new SkillDefinition(
                skillId,
                displayName,
                command,
                damage,
                cooldownDuration,
                executionDuration);
        }
    }
}
