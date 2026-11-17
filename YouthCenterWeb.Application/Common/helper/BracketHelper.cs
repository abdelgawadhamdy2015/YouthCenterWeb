using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Helpers;

public static class BracketHelper
{
    public static int NextPowerOfTwo(int n)
    {
        var p = 1;
        while (p < n) p <<= 1;
        return p;
    }

    /// ترتيب الـ seeds في الشجرة: seed1 ضد الأخير، و seed1 / seed2 في نصفين متقابلين.
    /// size=8 → [1,8,4,5,2,7,3,6]  (المباراة i = العنصرين 2i-1 و 2i)
    public static List<int> SeedOrder(int size)
    {
        var order = new List<int> { 1 };
        while (order.Count < size)
        {
            var n = order.Count * 2;
            order = order.SelectMany(s => new[] { s, n + 1 - s }).ToList();
        }
        return order;
    }

    public static CupMatchStage StageFor(int matchesInRound) => matchesInRound switch
    {
        1 => CupMatchStage.FinalMatch,
        2 => CupMatchStage.SemiFinal,
        4 => CupMatchStage.QuarterFinal,
        8 => CupMatchStage.RoundOf16,
        _ => CupMatchStage.RoundOf32
    };

    /// seededTeamIds[0] = أقوى seed. أي seed بعد آخر فريق = bye.
    /// مباريات الـ bye ما بتتولدش؛ الفريق بيتحط مباشرة في خانته في الدور التالي.
    public static List<CupMatch> Build(int cupId, IReadOnlyList<int> seededTeamIds, int firstRound)
    {
        int teams = seededTeamIds.Count;
        int size = NextPowerOfTwo(teams);
        int byes = size - teams;
        var order = SeedOrder(size);
        int? TeamOf(int seed) => seed <= teams ? seededTeamIds[seed - 1] : null;

        var matches = new List<CupMatch>();
        int count = size / 2;
        int round = firstRound;
        int finalRound = firstRound + (int)Math.Log2(size) - 1;

        while (count >= 1)
        {
            for (int i = 1; i <= count; i++)
            {
                var m = new CupMatch
                {
                    CupId = cupId,
                    Stage = StageFor(count),
                    Round = round,
                    MatchNumber = i,
                    Status = CupMatchStatus.Pending
                };
                if (round == firstRound)
                {
                    m.TeamAId = TeamOf(order[2 * i - 2]);
                    m.TeamBId = TeamOf(order[2 * i - 1]);
                }
                matches.Add(m);
            }
            count /= 2;
            round++;
        }

        // byes: نشيل المباراة وننقل الفريق للدور التالي
        foreach (var m in matches.Where(x => x.Round == firstRound).ToList())
        {
            if ((m.TeamAId == null) == (m.TeamBId == null)) continue; // مباراة حقيقية
            int winner = (m.TeamAId ?? m.TeamBId)!.Value;
            var next = matches.First(x => x.Round == firstRound + 1 &&
                                          x.Stage != CupMatchStage.ThirdPlace &&
                                          x.MatchNumber == (m.MatchNumber + 1) / 2);
            if (m.MatchNumber % 2 == 1) next.TeamAId = winner; else next.TeamBId = winner;
            matches.Remove(m);
        }

        // مركز ثالث: لازم نصفي النهائي الاتنين حقيقيين
        bool thirdPlace = size >= 4 && !(size == 4 && byes > 0);
        if (thirdPlace)
            matches.Add(new CupMatch
            {
                CupId = cupId,
                Stage = CupMatchStage.ThirdPlace,
                Round = finalRound,
                MatchNumber = 2,
                Status = CupMatchStatus.Pending
            });

        return matches;
    }
}