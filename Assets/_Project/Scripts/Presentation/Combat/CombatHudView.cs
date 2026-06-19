using System.Collections.Generic;
using ActiveTimeBattle.Application.Combat;
using ActiveTimeBattle.Domain.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ActiveTimeBattle.Presentation.Combat
{
    public sealed class CombatHudView : MonoBehaviour
    {
        [SerializeField] private Slider playerHealth;
        [SerializeField] private Slider enemyHealth;
        [SerializeField] private Slider enemyAction;
        [SerializeField] private Slider qteWindow;
        [SerializeField] private TMP_Text playerHealthText;
        [SerializeField] private TMP_Text enemyHealthText;
        [SerializeField] private TMP_Text commandText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text skillsText;

        public void Refresh(
            CombatSession session,
            IReadOnlyList<CombatDirection> recentDirections)
        {
            SetHealth(
                playerHealth,
                playerHealthText,
                session.Player.CurrentHealth,
                session.Player.MaximumHealth);
            SetHealth(
                enemyHealth,
                enemyHealthText,
                session.Enemy.CurrentHealth,
                session.Enemy.MaximumHealth);

            enemyAction.value =
                1f - session.Clock.EnemyActionTimer.NormalizedRemaining;
            qteWindow.gameObject.SetActive(
                session.Clock.DefenseQteWindow.IsActive);
            qteWindow.value = session.Clock.DefenseQteWindow.IsActive
                ? session.Clock.DefenseQteWindow.Remaining
                    / session.Clock.DefenseQteWindow.Duration
                : 0f;
            Image qteFill = qteWindow.fillRect != null
                ? qteWindow.fillRect.GetComponent<Image>()
                : null;
            if (qteFill != null && session.Clock.DefenseQteWindow.IsActive)
            {
                qteFill.color = GetQteColor(
                    session.Clock.DefenseQteWindow.GetCurrentResult());
            }

            commandText.text = recentDirections.Count == 0
                ? "方向指令：等待輸入"
                : $"方向指令：{FormatDirections(recentDirections)}";
            statusText.text = GetStatusText(session.Phase)
                + (session.Phase == CombatPhase.DefenseQte
                    ? "\n紅色：潰防　黃色：普通　藍色：完美"
                    : string.Empty)
                + (session.EnemyIsExposed ? "\n敵人狀態：易傷（下次攻擊 +50%）" : string.Empty);
            skillsText.text = FormatSkills(session.Skills);
        }

        private static void SetHealth(
            Slider slider,
            TMP_Text label,
            int current,
            int maximum)
        {
            slider.maxValue = maximum;
            slider.value = current;
            label.text = $"{current} / {maximum}";
        }

        private static string FormatDirections(
            IReadOnlyList<CombatDirection> directions)
        {
            string result = string.Empty;
            for (int index = 0; index < directions.Count; index++)
            {
                if (index > 0)
                {
                    result += " ";
                }

                result += directions[index] switch
                {
                    CombatDirection.Up => "↑",
                    CombatDirection.Down => "↓",
                    CombatDirection.Left => "←",
                    CombatDirection.Right => "→",
                    _ => "?"
                };
            }

            return result;
        }

        private static string FormatSkills(
            IEnumerable<SkillRuntime> skills)
        {
            string result = string.Empty;
            foreach (SkillRuntime skill in skills)
            {
                string cooldown = skill.IsReady
                    ? "可用"
                    : $"{skill.CooldownRemaining:0.0}s";
                result +=
                    $"{FormatDirections(skill.Definition.Command, " + ")}"
                    + $" + {skill.Definition.Id.ToUpperInvariant()}  "
                    + $"{skill.Definition.DisplayName} [{cooldown}]\n";
            }

            return result.TrimEnd();
        }

        private static string FormatDirections(
            IReadOnlyList<CombatDirection> directions,
            string separator)
        {
            string result = string.Empty;
            for (int index = 0; index < directions.Count; index++)
            {
                if (index > 0)
                {
                    result += separator;
                }

                result += FormatDirection(directions[index]);
            }

            return result;
        }

        private static string FormatDirection(CombatDirection direction)
        {
            return direction switch
            {
                CombatDirection.Up => "↑",
                CombatDirection.Down => "↓",
                CombatDirection.Left => "←",
                CombatDirection.Right => "→",
                _ => "?"
            };
        }

        private static string GetStatusText(CombatPhase phase)
        {
            return phase switch
            {
                CombatPhase.PlayerInput => "輸入方向後按技能鍵",
                CombatPhase.SkillExecution => "技能施放中：敵人行動 CD 暫停",
                CombatPhase.DefenseQte => "防禦 QTE：在適當顏色按 Space",
                CombatPhase.Victory => "戰鬥勝利",
                CombatPhase.Defeat => "戰鬥失敗",
                _ => phase.ToString()
            };
        }

        private static Color GetQteColor(DefenseQteResult result)
        {
            return result switch
            {
                DefenseQteResult.Perfect => new Color(0.2f, 0.9f, 1f),
                DefenseQteResult.Normal => new Color(1f, 0.75f, 0.1f),
                _ => new Color(0.95f, 0.15f, 0.18f)
            };
        }
    }
}
