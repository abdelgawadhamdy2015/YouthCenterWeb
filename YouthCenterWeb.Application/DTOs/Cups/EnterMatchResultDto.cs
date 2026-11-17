namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class EnterMatchResultDto
    {
        public int ScoreA { get; set; }
        public int ScoreB { get; set; }
        public int? PenaltyScoreA { get; set; }
        public int? PenaltyScoreB { get; set; }
        public int? YellowCardsA { get; set; }
        public int? YellowCardsB { get; set; }
        public int? RedCardsA { get; set; }
        public int? RedCardsB { get; set; }
    }
}