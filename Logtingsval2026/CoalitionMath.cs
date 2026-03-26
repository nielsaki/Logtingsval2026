using System.Collections.Generic;
using System.Linq;

namespace Logtingsval2026;

public static class CoalitionMath
{
    public sealed class CoalitionTableEntry
    {
        public required string Category { get; init; }
        public required int RowIndex { get; init; }
        public required int Sum { get; init; }
        /// <summary>Partibogstav → mandater i denne koalition (0 udelades i celler som ””).</summary>
        public required IReadOnlyDictionary<string, int> SeatByLetter { get; init; }
        public required int PartyCount { get; init; }
    }

    /// <summary>Beregner minimale og alle flertalskombinationer som tabelrækker.</summary>
    public static (IReadOnlyList<CoalitionTableEntry> Minimal, IReadOnlyList<CoalitionTableEntry> All) BuildCoalitionTables(
        IReadOnlyList<(string Letter, string Name, int Seats)> parties,
        int majorityNeed,
        int totalSeats)
    {
        if (parties.Count == 0)
            return ([], []);

        var n = parties.Count;
        var winning = new List<(int Size, int[] Ixs)>();

        for (var mask = 1; mask < 1 << n; mask++)
        {
            var ixs = new List<int>();
            for (var i = 0; i < n; i++)
                if ((mask & (1 << i)) != 0)
                    ixs.Add(i);

            var sum = ixs.Sum(i => parties[i].Seats);
            if (sum >= majorityNeed)
                winning.Add((ixs.Count, [..ixs]));
        }

        if (winning.Count == 0)
            return ([], []);

        var minimal = new List<int[]>();
        foreach (var (_, ixs) in winning)
        {
            var sumT = ixs.Sum(i => parties[i].Seats);
            if (sumT < majorityNeed) continue;

            var redundant = false;
            for (var j = 0; j < ixs.Length; j++)
            {
                var sumWithout = 0;
                for (var k = 0; k < ixs.Length; k++)
                    if (k != j) sumWithout += parties[ixs[k]].Seats;
                if (sumWithout >= majorityNeed)
                {
                    redundant = true;
                    break;
                }
            }
            if (!redundant)
                minimal.Add(ixs);
        }

        minimal = minimal
            .OrderBy(l => l.Length)
            .ThenByDescending(l => l.Sum(i => parties[i].Seats))
            .ToList();

        static CoalitionTableEntry Make(string cat, int rowIdx, int[] ixs, IReadOnlyList<(string Letter, string Name, int Seats)> p)
        {
            var dict = new Dictionary<string, int>();
            foreach (var i in ixs)
                dict[p[i].Letter] = p[i].Seats;
            return new CoalitionTableEntry
            {
                Category = cat,
                RowIndex = rowIdx,
                Sum = ixs.Sum(i => p[i].Seats),
                SeatByLetter = dict,
                PartyCount = ixs.Length,
            };
        }

        var minRows = new List<CoalitionTableEntry>();
        var r = 1;
        foreach (var ixs in minimal)
            minRows.Add(Make("Minst", r++, ixs, parties));

        var allRows = new List<CoalitionTableEntry>();
        r = 1;
        foreach (var (_, ixs) in winning
                     .OrderBy(w => w.Size)
                     .ThenByDescending(w => w.Ixs.Sum(i => parties[i].Seats)))
            allRows.Add(Make("Øll", r++, ixs, parties));

        return (minRows, allRows);
    }
}
