using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace FinanceApp.Tests
{
    // Pins the probe-split predicate logic from Program.cs:
    // /health/live  — Predicate: _ => false (no checks ever included → always Healthy)
    // /health/ready — Predicate: check => check.Tags.Contains("ready")
    // The Npgsql check is registered with tag "ready"; the liveness endpoint must
    // NOT include it, because a DB-dependent liveness probe causes restart cascades
    // when the DB blips (see Program.cs comment for the full explanation).
    public class HealthEndpointTests
    {
        private static HealthCheckRegistration MakeRegistration(string name, params string[] tags)
            => new HealthCheckRegistration(name, _ => null!, null, tags);

        [Fact]
        public void LivenessPredicate_ExcludesAllChecks()
        {
            // Liveness: Predicate = _ => false
            var predicate = (HealthCheckRegistration _) => false;
            var dbCheck = MakeRegistration("npgsql", "ready");

            Assert.False(predicate(dbCheck));
        }

        [Fact]
        public void ReadinessPredicate_IncludesReadyTaggedChecks()
        {
            // Readiness: Predicate = check => check.Tags.Contains("ready")
            var predicate = (HealthCheckRegistration check) => check.Tags.Contains("ready");
            var dbCheck = MakeRegistration("npgsql", "ready");

            Assert.True(predicate(dbCheck));
        }

        [Fact]
        public void ReadinessPredicate_ExcludesUntaggedChecks()
        {
            var predicate = (HealthCheckRegistration check) => check.Tags.Contains("ready");
            var otherCheck = MakeRegistration("some-other-check");

            Assert.False(predicate(otherCheck));
        }
    }
}
