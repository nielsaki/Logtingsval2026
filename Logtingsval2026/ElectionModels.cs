using System.Collections.Generic;

namespace Logtingsval2026;

public enum ElectionDataSource
{
    Dummy,
    /// <summary>Løgting — <see href="https://kvf.fo/lv26"/> (valurslit/lv2026).</summary>
    Lv26,
    /// <summary>Fólketing — <see href="https://kvf.fo/fv26"/> (valurslit/fv2026).</summary>
    Fv26,
}

/// <summary>Kandidat/person fra KVF <c>candidates</c> i sum-JSON.</summary>
public sealed class CandidateRow
{
    public required string PartyLetter { get; init; }
    public required string PartyName { get; init; }
    public required string Name { get; init; }
    public int PersonalVotes { get; init; }
    /// <summary>Sandt for «Listin»-rækker (stemmer på liste, ikke personvalg).</summary>
    public bool IsPartyListRow { get; init; }
    /// <summary>KVF-felt <c>elected: "Yes"</c>.</summary>
    public bool Elected { get; init; }
}

public sealed class PartyRow
{
    public required string Letter { get; init; }
    public required string Name { get; init; }
    public int Votes { get; init; }
    public string VotesPctDisplay { get; init; } = "—";
    public int Mandates { get; init; }
    public string BadgeColorHex { get; init; } = "#444";
    public string RowBgHex { get; init; } = "#2a2a2a";
}

public sealed class ElectionSnapshot
{
    public required string TitleLine { get; init; }
    public required string SubLine { get; init; }
    public bool IsDummy { get; init; }
    public ElectionDataSource DataSource { get; init; }
    /// <summary>Antal mandater i parlamentet (Løgting 33, Fólketing 179).</summary>
    public int ParliamentTotalSeats { get; init; } = 33;
    /// <summary>Mindst dette antal mandater for flertal.</summary>
    public int MajoritySeats { get; init; } = 17;
    public int Electorate { get; init; }
    public IReadOnlyList<PartyRow> Parties { get; init; } = [];
    public IReadOnlyList<CandidateRow> Candidates { get; init; } = [];
}
