using System;
using System.Collections.Generic;
using System.Linq;

namespace Logtingsval2026;

/// <summary>
/// Tilfældig fordeling af stemmer blandt 7 partier; 38.955 mulige stemmer (valgkreds);
/// et tilfældigt antal faktiske gyldige stemmer (valgdeltagelse).
/// </summary>
public static class DummyDataGenerator
{
    public const int DummyElectorate = 38_955;
    public const int DummyPartyCount = 7;
    public const int DummyTotalSeats = 33;
    public const int DummyMajority = DummyTotalSeats / 2 + 1;

    private static readonly (string Letter, string Name, string Fg, string Bg)[] PartyDef =
    [
        ("A", "Alpha-flokkurin", "#498B75", "#1a2e28"),
        ("B", "Beta-samband", "#4B748F", "#1a2429"),
        ("C", "Charlie-javnaður", "#BC443D", "#2e1a1a"),
        ("D", "Delta-sjálvstýri", "#DA5098", "#2e1a27"),
        ("E", "Echo-tjóðveldi", "#97C1A8", "#1a2a22"),
        ("F", "Foxtrot-framsókn", "#F5931C", "#2e2415"),
        ("G", "Golf-miðja", "#2F4E79", "#1a2330"),
    ];

    /// <summary>Tilfældige vægte → stemmer der summer til <paramref name="validVotes"/>.</summary>
    public static int[] SplitVotes(Random rnd, int partyCount, int validVotes)
    {
        var weights = new int[partyCount];
        for (var i = 0; i < partyCount; i++)
            weights[i] = rnd.Next(800, 50_000);

        var sumW = weights.Sum();
        var votes = new int[partyCount];
        var acc = 0;
        for (var i = 0; i < partyCount; i++)
        {
            votes[i] = (int)Math.Round((double)weights[i] / sumW * validVotes);
            acc += votes[i];
        }

        var diff = validVotes - acc;
        votes[rnd.Next(partyCount)] += diff;
        for (var i = 0; i < partyCount; i++)
            votes[i] = Math.Max(0, votes[i]);

        // Ret evt. afrunding så summen altid er præcis
        var fix = validVotes - votes.Sum();
        votes[Array.IndexOf(votes, votes.Max())] += fix;
        return votes;
    }

    public static int[] Dhondt(int[] partyVotes, int seats)
    {
        var n = partyVotes.Length;
        var m = new int[n];
        for (var s = 0; s < seats; s++)
        {
            var bestI = 0;
            var bestQ = -1.0;
            for (var i = 0; i < n; i++)
            {
                var q = partyVotes[i] / (1.0 + m[i]);
                if (q > bestQ)
                {
                    bestQ = q;
                    bestI = i;
                }
            }
            m[bestI]++;
        }
        return m;
    }

    public static ElectionSnapshot Generate(Random? random = null)
    {
        var rnd = random ?? Random.Shared;
        // Tilfældig valgdeltagelse blandt afgivne gyldige stemmer (35 % … 92 % af valgkredsen)
        var turnout = rnd.Next(3500, 9201) / 10_000.0;
        var validVotes = (int)Math.Round(DummyElectorate * turnout);
        validVotes = Math.Clamp(validVotes, 1, DummyElectorate);

        var votes = SplitVotes(rnd, DummyPartyCount, validVotes);
        var mandates = Dhondt(votes, DummyTotalSeats);

        var rows = new List<PartyRow>(DummyPartyCount);
        for (var i = 0; i < DummyPartyCount; i++)
        {
            var d = PartyDef[i];
            var pct = 100.0 * votes[i] / validVotes;
            rows.Add(new PartyRow
            {
                Letter = d.Letter,
                Name = d.Name,
                Votes = votes[i],
                VotesPctDisplay = pct.ToString("0.00").Replace('.', ',') + "%",
                Mandates = mandates[i],
                BadgeColorHex = d.Fg,
                RowBgHex = d.Bg,
            });
        }

        rows.Sort((a, b) => string.CompareOrdinal(a.Letter, b.Letter));

        var candidates = new List<CandidateRow>();
        for (var i = 0; i < DummyPartyCount; i++)
        {
            var d = PartyDef[i];
            var m = mandates[i];
            var count = Math.Max(m + 3, 6);
            var personal = new int[count];
            for (var k = 0; k < count; k++)
                personal[k] = rnd.Next(200, 8000);
            Array.Sort(personal);
            Array.Reverse(personal);
            for (var k = 0; k < count; k++)
            {
                candidates.Add(new CandidateRow
                {
                    PartyLetter = d.Letter,
                    PartyName = d.Name,
                    Name = $"Kandi {d.Letter}-{k + 1}",
                    PersonalVotes = personal[k],
                    IsPartyListRow = false,
                    Elected = k < m,
                });
            }
        }

        return new ElectionSnapshot
        {
            IsDummy = true,
            DataSource = ElectionDataSource.Dummy,
            ParliamentTotalSeats = DummyTotalSeats,
            MajoritySeats = DummyMajority,
            Electorate = DummyElectorate,
            TitleLine = $"PRÓGV — áljóðað val ({DateTime.Now:HH:mm:ss})",
            SubLine = $"{turnout * 100:0.0}% valmdeltøka í prógvnum · {validVotes:N0} gild av {DummyElectorate:N0} atkvæðisrøðum · ⌘D / Ctrl+D → KVF",
            Parties = rows,
            Candidates = candidates,
        };
    }
}
