using ActiveTimeBattle.Domain.Narrative;
using NUnit.Framework;

namespace ActiveTimeBattle.Tests
{
    public sealed class NarrativeContextTests
    {
        [Test]
        public void FlagsCanBeSetAndCleared()
        {
            NarrativeContext context = new NarrativeContext();

            context.SetFlag("met_keeper");
            Assert.That(context.HasFlag("met_keeper"), Is.True);

            context.Clear();
            Assert.That(context.HasFlag("met_keeper"), Is.False);
        }

        [Test]
        public void EmptyFlagsAreIgnored()
        {
            NarrativeContext context = new NarrativeContext();

            context.SetFlag(string.Empty);
            context.SetFlag(null);

            Assert.That(context.HasFlag(string.Empty), Is.False);
            Assert.That(context.HasFlag(null), Is.False);
        }
    }
}
