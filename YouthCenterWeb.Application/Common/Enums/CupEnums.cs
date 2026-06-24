namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Enums
{
    public enum CupFormat
    {
        SingleElimination = 1,
        RoundRobin = 2          // group stage → knockout
    }

    public enum CupStatus
    {
        Draft = 1,
        RegistrationOpen = 2,
        GroupStage = 3,         // matches being played in groups
        KnockoutStage = 4,      // group stage done, knockout running
        Completed = 5,
        Cancelled = 6
    }

    public enum CupTeamStatus
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3
    }

    public enum CupMatchStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Bye = 4
    }

    /// <summary>
    /// Which stage of the tournament this match belongs to.
    /// GroupStage matches carry a GroupId; all others are knockout.
    /// </summary>
    public enum CupMatchStage
    {
        GroupStage = 1,
        RoundOf16 = 2,
        QuarterFinal = 3,
        SemiFinal = 4,
        ThirdPlace = 5,
        Final = 6
    }
}