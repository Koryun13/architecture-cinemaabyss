using CinemaAbyss.Proxy.Domain.Enums;

namespace CinemaAbyss.Proxy.Domain.Policies;

/// <summary>
/// The Strangler Fig feature flag for the movies domain.
///
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Gradual migration on</b> — <see cref="Percent"/> percent of requests
///       go to the movies microservice, the rest stay on the monolith. Raising
///       the percentage moves traffic over step by step without a redeploy of
///       either backend.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Gradual migration off</b> — the domain is considered migrated and
///       every request goes to the microservice (as specified in README.md).
///     </description>
///   </item>
/// </list>
///
/// The decision is a pure function of a roll in [0, 100), so the policy holds
/// no randomness of its own and its behaviour is fully determined by the input.
/// </summary>
public sealed record MoviesMigrationPolicy
{
    public const int MinPercent = 0;
    public const int MaxPercent = 100;

    public MoviesMigrationPolicy(bool gradualMigration, int percent)
    {
        if (percent is < MinPercent or > MaxPercent)
        {
            throw new ArgumentOutOfRangeException(
                nameof(percent),
                percent,
                $"The migration percentage must be between {MinPercent} and {MaxPercent}.");
        }

        GradualMigration = gradualMigration;
        Percent = percent;
    }

    public bool GradualMigration { get; }

    public int Percent { get; }

    /// <param name="roll">A uniformly distributed value in [0, 100).</param>
    public MigrationTarget Choose(int roll)
    {
        if (!GradualMigration)
        {
            return MigrationTarget.MoviesService;
        }

        return roll < Percent ? MigrationTarget.MoviesService : MigrationTarget.Monolith;
    }

    public override string ToString() => GradualMigration
        ? $"gradual migration, {Percent}% to movies-service"
        : "migration complete, 100% to movies-service";
}
