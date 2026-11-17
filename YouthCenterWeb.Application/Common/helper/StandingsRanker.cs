using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Helpers;

public static class StandingsRanker
{
    private sealed class H2H { public int Pts, Gf, Ga; }

    public static List<CupTeam> Rank(IEnumerable<CupTeam> teams, IEnumerable<CupMatch> groupMatches)
    {
        var played = groupMatches
            .Where(m => m.Status == CupMatchStatus.Completed &&
                        m.TeamAId != null && m.TeamBId != null &&
                        m.ScoreA != null && m.ScoreB != null)
            .ToList();

        var result = new List<CupTeam>();
        foreach (var tier in teams.GroupBy(t => t.Points).OrderByDescending(g => g.Key))
        {
            var tied = tier.ToList();
            result.AddRange(tied.Count == 1 ? tied : BreakTie(tied, played));
        }
        return result;
    }

    private static IEnumerable<CupTeam> BreakTie(List<CupTeam> tied, List<CupMatch> played)
    {
        var ids = tied.Select(t => t.Id).ToHashSet();
        var h2h = tied.ToDictionary(t => t.Id, _ => new H2H());

        foreach (var m in played.Where(m => ids.Contains(m.TeamAId!.Value) &&
                                            ids.Contains(m.TeamBId!.Value)))
        {
            var a = h2h[m.TeamAId!.Value];
            var b = h2h[m.TeamBId!.Value];
            a.Gf += m.ScoreA!.Value; a.Ga += m.ScoreB!.Value;
            b.Gf += m.ScoreB!.Value; b.Ga += m.ScoreA!.Value;
            if (m.ScoreA > m.ScoreB) a.Pts += 3;
            else if (m.ScoreB > m.ScoreA) b.Pts += 3;
            else { a.Pts++; b.Pts++; }
        }

        return tied
            .OrderByDescending(t => h2h[t.Id].Pts)                    // ← H2H (انقل الأسطر الثلاثة لتحت لو عايز FIFA)
            .ThenByDescending(t => h2h[t.Id].Gf - h2h[t.Id].Ga)
            .ThenByDescending(t => h2h[t.Id].Gf)
            .ThenByDescending(t => t.GoalsFor - t.GoalsAgainst)
            .ThenByDescending(t => t.GoalsFor)
            .ThenBy(t => t.FairPlayPoints)
            .ThenBy(t => t.Id);
    }
}