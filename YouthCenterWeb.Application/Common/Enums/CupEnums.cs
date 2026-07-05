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
        RoundOf32 = 2,
        RoundOf16 = 3,
        QuarterFinal = 4,
        SemiFinal = 5,
        ThirdPlace = 6,
        FinalMatch = 7
    }
}