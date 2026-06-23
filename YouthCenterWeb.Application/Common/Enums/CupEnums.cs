namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Enums
{
    public enum CupFormat
    {
        SingleElimination = 1,
        DoubleElimination = 2,
        RoundRobin = 3
    }

    public enum CupStatus
    {
        Draft = 1,
        RegistrationOpen = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5
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
        Bye = 4           // auto-win when bracket has odd team count
    }
}