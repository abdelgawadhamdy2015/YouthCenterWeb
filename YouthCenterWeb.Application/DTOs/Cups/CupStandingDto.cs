namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CupStandingDto
    {
        public int TeamId { get; set; }

        public string TeamName { get; set; } = string.Empty;

        public int PlayedMatches { get; set; }

        public int Wins { get; set; }

        public int Draws { get; set; }

        public int Losses { get; set; }

        public int GoalsFor { get; set; }

        public int GoalsAgainst { get; set; }

        public int GoalDifference { get; set; }

        public int Points { get; set; }
    }
}