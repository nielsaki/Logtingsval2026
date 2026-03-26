using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Logtingsval2026;

/// <summary>
/// rpc.php + sum-*.json som på <see href="https://kvf.fo/lv26"/> og <see href="https://kvf.fo/fv26"/>.
/// </summary>
public static class KvfApi
{
    public const string Lv2026Base = "https://kvf.fo/valurslit/lv2026/";
    public const string Fv2026Base = "https://kvf.fo/valurslit/fv2026/";

    public const string Lv26PageUrl = "https://kvf.fo/lv26";
    public const string Fv26PageUrl = "https://kvf.fo/fv26";

    private const string UserAgent = "Mozilla/5.0 (Logtingsval2026/1.0)";

    private static readonly HttpClient Http = CreateHttp();

    private static HttpClient CreateHttp()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
        http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        return http;
    }

    /// <summary>Færøske partier (A–F, H) som på valgresultaterne.</summary>
    private static readonly Dictionary<string, (string Name, string Fg, string Bg)> Style = new()
    {
        ["A"] = (Name: "Fólkaflokkurin", Fg: "#498B75", Bg: "#1a2e28"),
        ["B"] = (Name: "Sambandsflokkurin", Fg: "#4B748F", Bg: "#1a2429"),
        ["C"] = (Name: "Javnaðurflokkurin", Fg: "#BC443D", Bg: "#2e1a1a"),
        ["D"] = (Name: "Sjálvstýri", Fg: "#DA5098", Bg: "#2e1a27"),
        ["E"] = (Name: "Tjóðveldi", Fg: "#97C1A8", Bg: "#1a2a22"),
        ["F"] = (Name: "Framsókn", Fg: "#F5931C", Bg: "#2e2415"),
        ["H"] = (Name: "Miðflokkurin", Fg: "#2F4E79", Bg: "#1a2330"),
    };

    public static Task<ElectionSnapshot?> FetchLv26Async(CancellationToken ct = default) =>
        FetchFromValurslitAsync(Lv2026Base, ElectionDataSource.Lv26, Lv26PageUrl, parliamentTotal: 33, ct);

    /// <summary>Færøsk Fólkatingskreds: 2 mandater (jvf. <see href="https://kvf.fo/fv26"/>).</summary>
    public const int Fv26FaroeSeats = 2;

    public static Task<ElectionSnapshot?> FetchFv26Async(CancellationToken ct = default) =>
        FetchFromValurslitAsync(Fv2026Base, ElectionDataSource.Fv26, Fv26PageUrl, parliamentTotal: Fv26FaroeSeats, ct);

    public static async Task<ElectionSnapshot?> FetchFromValurslitAsync(
        string baseUrl,
        ElectionDataSource source,
        string publicPageUrl,
        int parliamentTotal,
        CancellationToken ct = default)
    {
        try
        {
            var rpcJson = await Http.GetStringAsync(new Uri(baseUrl.TrimEnd('/') + "/rpc.php"), ct).ConfigureAwait(false);
            using var rpc = JsonDocument.Parse(rpcJson);
            if (!rpc.RootElement.TryGetProperty("sum", out var sumEl))
                return EmptyLive(source, publicPageUrl, parliamentTotal, "KVF: sum manglar í rpc.");
            if (!TryGetLatestSumRelativePath(sumEl, out var path) || string.IsNullOrEmpty(path))
                return EmptyLive(source, publicPageUrl, parliamentTotal, "KVF: einki sum í rpc enn.");

            var sumUrl = path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? path
                : baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');

            var sumBody = await Http.GetStringAsync(new Uri(sumUrl), ct).ConfigureAwait(false);
            using var sumDoc = JsonDocument.Parse(sumBody);
            return ParseSumDocument(sumDoc, source, publicPageUrl, parliamentTotal);
        }
        catch (Exception ex)
        {
            return EmptyLive(source, publicPageUrl, parliamentTotal, "KVF-feilur: " + ex.Message);
        }
    }

    /// <summary>
    /// LV26: <c>sum</c> er et array (_tag_ seneste indeks). FV26: <c>sum</c> er et objekt med talnøgler (antal tællende valstøð) — tag højeste nøgle.
    /// </summary>
    private static bool TryGetLatestSumRelativePath(JsonElement sumEl, out string? path)
    {
        path = null;
        switch (sumEl.ValueKind)
        {
            case JsonValueKind.Array:
                if (sumEl.GetArrayLength() == 0)
                    return false;
                var last = sumEl[sumEl.GetArrayLength() - 1];
                path = last.TryGetProperty("name", out var n) ? n.GetString() : null;
                return !string.IsNullOrEmpty(path);
            case JsonValueKind.Object:
                var bestKey = int.MinValue;
                JsonElement bestEntry = default;
                var found = false;
                foreach (var prop in sumEl.EnumerateObject())
                {
                    if (!int.TryParse(prop.Name, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var key))
                        continue;
                    if (!found || key > bestKey)
                    {
                        found = true;
                        bestKey = key;
                        bestEntry = prop.Value;
                    }
                }
                if (!found)
                    return false;
                path = bestEntry.TryGetProperty("name", out var n2) ? n2.GetString() : null;
                return !string.IsNullOrEmpty(path);
            default:
                return false;
        }
    }

    private static ElectionSnapshot EmptyLive(ElectionDataSource source, string publicPageUrl, int parliamentTotal, string title) =>
        new()
        {
            IsDummy = false,
            DataSource = source,
            ParliamentTotalSeats = parliamentTotal,
            MajoritySeats = parliamentTotal / 2 + 1,
            Electorate = 0,
            TitleLine = title,
            SubLine = $"Kelda: {publicPageUrl} · ⌘D / Ctrl+D = prógv.",
            Parties = [],
            Candidates = [],
        };

    private static ElectionSnapshot? ParseSumDocument(
        JsonDocument sumDoc,
        ElectionDataSource source,
        string publicPageUrl,
        int parliamentTotal)
    {
        var majority = parliamentTotal / 2 + 1;
        var root = sumDoc.RootElement;
        root.TryGetProperty("header", out var h);
        var title = TryStr(h, "electionname") ?? (source == ElectionDataSource.Fv26 ? "Fólkatingsval" : "Løgtingsval");
        var suffrage = TryInt(h, "total_suffrage") ?? 0;
        var valid = TryInt(h, "validvotes");
        var pctRaw = TryStr(h, "total_pct") ?? (h.ValueKind != JsonValueKind.Undefined ? GetNumStr(h, "total_pct") : null);

        var subParts = new List<string> { $"Kelda: {publicPageUrl}" };
        if (!string.IsNullOrEmpty(pctRaw))
            subParts.Add($"{pctRaw}% uptalt");
        if (valid.HasValue)
            subParts.Add($"{valid.Value:N0} atkvøður");

        if (!root.TryGetProperty("parties", out var partiesEl) || partiesEl.ValueKind != JsonValueKind.Array)
            return new ElectionSnapshot
            {
                IsDummy = false,
                DataSource = source,
                ParliamentTotalSeats = parliamentTotal,
                MajoritySeats = majority,
                Electorate = suffrage,
                TitleLine = title,
                SubLine = string.Join(" · ", subParts) + " — eingin flokkur í JSON enn.",
                Parties = [],
                Candidates = [],
            };

        var rows = new List<PartyRow>();
        foreach (var p in partiesEl.EnumerateArray())
        {
            var letter = (TryStr(p, "party_letter") ?? "").Trim().ToUpperInvariant();
            if (letter.Length > 0)
                letter = letter[..1];
            if (letter.Length == 0) continue;

            var votes = TryInt(p, "party_votes") ?? 0;
            var pctDisplay = TryStr(p, "party_votes_pct")
                             ?? GetNumStr(p, "party_votes_pct");
            if (!string.IsNullOrEmpty(pctDisplay) && !pctDisplay.Contains('%'))
                pctDisplay = pctDisplay.Replace('.', ',') + "%";

            var md = 0;
            if (p.TryGetProperty("mandates", out var m) && m.ValueKind == JsonValueKind.Array)
                md = m.GetArrayLength();

            var st = Style.GetValueOrDefault(letter, (Name: $"Parti {letter}", Fg: "#666", Bg: "#2a2a2a"));
            var displayName = TryLocalPartyName(p) ?? st.Name;
            rows.Add(new PartyRow
            {
                Letter = letter,
                Name = displayName,
                Votes = votes,
                VotesPctDisplay = string.IsNullOrEmpty(pctDisplay) ? "—" : pctDisplay,
                Mandates = md,
                BadgeColorHex = st.Fg,
                RowBgHex = st.Bg,
            });
        }

        var letterToPartyName = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var r in rows)
            letterToPartyName[r.Letter] = r.Name;

        var candidates = ParseCandidates(root, letterToPartyName);

        rows.Sort((a, b) => string.CompareOrdinal(a.Letter, b.Letter));

        subParts.Add("⌘D / Ctrl+D = prógv.");

        return new ElectionSnapshot
        {
            IsDummy = false,
            DataSource = source,
            ParliamentTotalSeats = parliamentTotal,
            MajoritySeats = majority,
            Electorate = suffrage,
            TitleLine = title,
            SubLine = string.Join(" · ", subParts),
            Parties = rows,
            Candidates = candidates,
        };
    }

    private static List<CandidateRow> ParseCandidates(JsonElement root, Dictionary<string, string> letterToPartyName)
    {
        var list = new List<CandidateRow>();
        if (!root.TryGetProperty("candidates", out var arr) || arr.ValueKind != JsonValueKind.Array)
            return list;

        foreach (var c in arr.EnumerateArray())
        {
            var typeStr = TryStr(c, "candidate_type");
            var isPartyList = string.Equals(typeStr, "party", StringComparison.OrdinalIgnoreCase);

            var letter = (TryStr(c, "party_letter") ?? "").Trim().ToUpperInvariant();
            if (letter.Length > 1)
                letter = letter[..1];

            var partyName = TryStr(c, "party_name");
            if (string.IsNullOrEmpty(partyName))
                partyName = letterToPartyName.GetValueOrDefault(letter, "");

            var name = TryStr(c, "name") ?? "";
            if (string.IsNullOrEmpty(name))
                continue;

            var votes = TryInt(c, "votes") ?? 0;
            var elected = IsElectedYes(c);

            list.Add(new CandidateRow
            {
                PartyLetter = letter,
                PartyName = partyName,
                Name = name,
                PersonalVotes = votes,
                IsPartyListRow = isPartyList,
                Elected = elected,
            });
        }

        return list;
    }

    private static bool IsElectedYes(JsonElement c)
    {
        if (!c.TryGetProperty("elected", out var e))
            return false;
        return e.ValueKind switch
        {
            JsonValueKind.String => string.Equals(e.GetString(), "Yes", StringComparison.OrdinalIgnoreCase),
            JsonValueKind.True => true,
            _ => false,
        };
    }

    /// <summary>Som på KVF (f.eks. FV26 JSON med <c>party_name</c>).</summary>
    private static string? TryLocalPartyName(JsonElement party) =>
        TryStr(party, "party_name");

    private static string? TryStr(JsonElement e, string name) =>
        e.ValueKind != JsonValueKind.Undefined && e.TryGetProperty(name, out var x) && x.ValueKind == JsonValueKind.String
            ? x.GetString()
            : null;

    private static int? TryInt(JsonElement e, string name)
    {
        if (e.ValueKind == JsonValueKind.Undefined || !e.TryGetProperty(name, out var x))
            return null;
        return x.ValueKind switch
        {
            JsonValueKind.Number when x.TryGetInt32(out var i) => i,
            JsonValueKind.String => int.TryParse(x.GetString()?.Replace(".", ""), out var j) ? j : null,
            _ => null,
        };
    }

    private static string? GetNumStr(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var x)) return null;
        return x.ValueKind switch
        {
            JsonValueKind.Number => x.GetDouble().ToString("0.00").Replace('.', ','),
            JsonValueKind.String => x.GetString(),
            _ => null,
        };
    }
}
