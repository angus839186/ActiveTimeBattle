using System.Linq;
using ActiveTimeBattle.Application.Combat;
using ActiveTimeBattle.Domain.Combat;
using NUnit.Framework;

namespace ActiveTimeBattle.Tests
{
    public sealed class CombatSessionTests
    {
        [Test]
        public void DirectionBufferMatchesSuffixAndExpiresOldInput()
        {
            DirectionInputBuffer buffer = new DirectionInputBuffer(4, 1f);
            buffer.Add(CombatDirection.Left, 0f);
            buffer.Add(CombatDirection.Up, 0.5f);
            buffer.Add(CombatDirection.Right, 0.75f);

            Assert.That(
                buffer.EndsWith(
                    new[] { CombatDirection.Up, CombatDirection.Right },
                    0.75f),
                Is.True);
            Assert.That(
                buffer.EndsWith(
                    new[] { CombatDirection.Left, CombatDirection.Up },
                    1.2f),
                Is.False);
        }

        [Test]
        public void SkillRequiresMatchingCommandAndStartsCooldown()
        {
            CombatSession session = CreateSession(
                out SkillRuntime skill,
                enemyHealth: 100);

            session.AddDirection(CombatDirection.Up, 0f);
            Assert.That(session.TryUseSkill("q", 0f), Is.False);

            session.AddDirection(CombatDirection.Up, 0.05f);
            session.AddDirection(CombatDirection.Right, 0.1f);
            Assert.That(session.TryUseSkill("q", 0.1f), Is.True);
            Assert.That(session.Enemy.CurrentHealth, Is.EqualTo(80));
            Assert.That(skill.IsReady, Is.False);
            Assert.That(session.Phase, Is.EqualTo(CombatPhase.SkillExecution));
        }

        [Test]
        public void DirectionThatMatchesNoSkillPrefixClearsInput()
        {
            CombatSession session = CreateSession(
                out _,
                enemyHealth: 100);

            Assert.That(
                session.AddDirection(CombatDirection.Down, 0f),
                Is.False);
            Assert.That(session.InputBuffer.Count, Is.Zero);
        }

        [Test]
        public void WrongSkillKeyClearsInput()
        {
            CombatSession session = CreateSession(
                out _,
                enemyHealth: 100);
            session.AddDirection(CombatDirection.Up, 0f);
            session.AddDirection(CombatDirection.Right, 0.1f);

            Assert.That(session.TryUseSkill("w", 0.1f), Is.False);
            Assert.That(session.InputBuffer.Count, Is.Zero);
            Assert.That(session.TryUseSkill("q", 0.1f), Is.False);
        }

        [Test]
        public void SkillExecutionPausesEnemyTimerButNotSkillCooldown()
        {
            CombatSession session = CreateSession(
                out SkillRuntime skill,
                enemyHealth: 100);
            session.AddDirection(CombatDirection.Up, 0f);
            session.AddDirection(CombatDirection.Right, 0.1f);
            session.TryUseSkill("q", 0.1f);
            float enemyRemaining = session.Clock.EnemyActionTimer.Remaining;

            session.Tick(0.25f);

            Assert.That(
                session.Clock.EnemyActionTimer.Remaining,
                Is.EqualTo(enemyRemaining).Within(0.001f));
            Assert.That(
                skill.CooldownRemaining,
                Is.EqualTo(2.75f).Within(0.001f));
        }

        [Test]
        public void MissingDefenseDealsDamage()
        {
            CombatSession session = CreateSession(
                out _,
                enemyHealth: 100,
                enemyActionDuration: 0.5f,
                qteDuration: 0.4f);

            session.Tick(0.5f);
            Assert.That(session.Phase, Is.EqualTo(CombatPhase.DefenseQte));

            session.Tick(0.4f);

            Assert.That(session.Player.CurrentHealth, Is.EqualTo(75));
            Assert.That(session.Phase, Is.EqualTo(CombatPhase.PlayerInput));
        }

        [Test]
        public void PerfectDefensePreventsDamage()
        {
            CombatSession session = CreateSession(
                out _,
                enemyHealth: 100,
                enemyActionDuration: 0.5f,
                qteDuration: 0.4f,
                perfectWindow: 0.15f);
            session.Tick(0.5f);
            session.Tick(0.3f);

            Assert.That(session.TryDefend(), Is.True);
            Assert.That(session.Player.CurrentHealth, Is.EqualTo(100));
            Assert.That(session.Phase, Is.EqualTo(CombatPhase.PlayerInput));
        }

        [Test]
        public void NormalDefenseDealsHalfDamageRoundedUp()
        {
            CombatSession session = CreateSession(
                out _,
                enemyHealth: 100,
                enemyActionDuration: 0.5f,
                qteDuration: 0.6f,
                perfectWindow: 0.1f);
            session.Tick(0.5f);
            session.Tick(0.25f);

            Assert.That(session.TryDefend(), Is.True);
            Assert.That(session.Player.CurrentHealth, Is.EqualTo(87));
            Assert.That(session.Phase, Is.EqualTo(CombatPhase.PlayerInput));
        }

        [Test]
        public void EarlyDefenseBreakDealsFullDamage()
        {
            CombatSession session = CreateSession(
                out _,
                enemyHealth: 100,
                enemyActionDuration: 0.5f,
                qteDuration: 0.6f,
                perfectWindow: 0.1f);
            session.Tick(0.5f);

            Assert.That(session.TryDefend(), Is.True);
            Assert.That(session.Player.CurrentHealth, Is.EqualTo(75));
            Assert.That(session.Phase, Is.EqualTo(CombatPhase.PlayerInput));
        }

        [Test]
        public void LethalSkillEndsCombatInVictory()
        {
            CombatSession session = CreateSession(
                out _,
                enemyHealth: 20);
            session.AddDirection(CombatDirection.Up, 0f);
            session.AddDirection(CombatDirection.Right, 0.1f);

            Assert.That(session.TryUseSkill("q", 0.1f), Is.True);
            Assert.That(session.Phase, Is.EqualTo(CombatPhase.Victory));
            Assert.That(session.Enemy.IsDefeated, Is.True);
        }

        [Test]
        public void BreakSkillExposesEnemyAndNextAttackConsumesExposure()
        {
            SkillRuntime breakSkill = CreateSkill(
                "w",
                new[] { CombatDirection.Left },
                10);
            SkillRuntime attackSkill = CreateSkill(
                "q",
                new[] { CombatDirection.Right },
                20);
            SkillRuntime[] skills = { breakSkill, attackSkill };
            CombatSession session = new CombatSession(
                new CharacterRuntime(100),
                new CharacterRuntime(100),
                skills,
                new DirectionInputBuffer(8, 2f),
                new CombatClock(
                    new EnemyActionTimer(5f),
                    new DefenseQteWindow(1f, 0.6f, 0.25f),
                    skills),
                20);

            session.AddDirection(CombatDirection.Left, 0f);
            Assert.That(session.TryUseSkill("w", 0f), Is.True);
            Assert.That(session.EnemyIsExposed, Is.True);
            session.Tick(0.5f);

            session.AddDirection(CombatDirection.Right, 1f);
            Assert.That(session.TryUseSkill("q", 1f), Is.True);
            Assert.That(session.Enemy.CurrentHealth, Is.EqualTo(60));
            Assert.That(session.EnemyIsExposed, Is.False);
        }

        private static CombatSession CreateSession(
            out SkillRuntime skill,
            int enemyHealth,
            float enemyActionDuration = 4f,
            float qteDuration = 1f,
            float perfectWindow = 0.25f)
        {
            SkillDefinition definition = new SkillDefinition(
                "q",
                "測試技能",
                new[] { CombatDirection.Up, CombatDirection.Right },
                20,
                3f,
                0.5f);
            skill = new SkillRuntime(definition);
            SkillRuntime[] skills = { skill };
            CombatClock clock = new CombatClock(
                new EnemyActionTimer(enemyActionDuration),
                new DefenseQteWindow(
                    qteDuration,
                    System.Math.Max(perfectWindow, qteDuration * 0.65f),
                    perfectWindow),
                skills);

            return new CombatSession(
                new CharacterRuntime(100),
                new CharacterRuntime(enemyHealth),
                skills,
                new DirectionInputBuffer(8, 2f),
                clock,
                25);
        }

        private static SkillRuntime CreateSkill(
            string id,
            CombatDirection[] command,
            int damage)
        {
            return new SkillRuntime(
                new SkillDefinition(
                    id,
                    id,
                    command,
                    damage,
                    0f,
                    0.25f));
        }
    }
}
