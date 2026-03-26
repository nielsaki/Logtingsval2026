using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;

namespace Logtingsval2026;

public partial class MainWindow : Window
{
    private bool _useDummy;
    private int _parliamentTotalSeats = DummyDataGenerator.DummyTotalSeats;
    private int _majoritySeats = DummyDataGenerator.DummyMajority;
    private readonly DispatcherTimer _poll = new() { Interval = TimeSpan.FromSeconds(5) };
    private readonly List<PartyRowUi> _rows = [];
    private readonly List<string> _selectionOrder = [];
    private readonly Random _dummyRnd = new();
    private CoalitionsWindow? _coalitionsWindow;
    private IReadOnlyList<CandidateRow> _lastCandidates = [];
    private IReadOnlyList<PartyRow> _lastParties = [];

    private sealed class PartyRowUi
    {
        public required string Letter { get; init; }
        public required string Name { get; init; }
        public int Mandates { get; init; }
        public int Votes { get; init; }
        public required string BadgeColorHex { get; init; }
        public required Border Host { get; init; }
    }

    public MainWindow()
    {
        InitializeComponent();
        TrySetWindowIcon();
        Opened += (_, _) => WindowState = WindowState.FullScreen;

        _poll.Tick += (_, _) => _ = PollLiveKvfIfNeededAsync();
        _poll.Start();

        PeopleSearch.TextChanged += (_, _) =>
        {
            if (!IsLoaded)
                return;
            RefreshCandidateSide(preserveTopsScroll: false);
        };

        Loaded += async (_, _) =>
        {
            ModeBanner.Text = "Lesur inn KVF …";
            HeaderDetail.Text = "";
            await FetchLiveAsync();
        };
    }

    private void TrySetWindowIcon()
    {
        try
        {
            var uri = new Uri("avares://Logtingsval2026/Assets/app-logo.png");
            using var stream = AssetLoader.Open(uri);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            Icon = new WindowIcon(ms);
        }
        catch
        {
            // ikon kann ikki lesast (t.d. manglandi skrá í prógvnum)
        }
    }

    private async Task PollLiveKvfIfNeededAsync()
    {
        if (_useDummy || ChkAuto.IsChecked != true)
            return;
        var snap = await KvfApi.FetchLv26Async().ConfigureAwait(true);
        if (snap != null)
            ApplySnapshot(snap);
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        var mod = e.KeyModifiers;
        var cmdOrCtrl = mod.HasFlag(KeyModifiers.Meta) || mod.HasFlag(KeyModifiers.Control);
        if (cmdOrCtrl && e.Key == Key.D)
        {
            _useDummy = !_useDummy;
            e.Handled = true;
            if (_useDummy)
                ApplySnapshot(DummyDataGenerator.Generate(_dummyRnd));
            else
                _ = FetchLiveAsync();
        }
    }

    private async void OnFetchLv26Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _useDummy = false;
        await FetchLiveAsync();
    }

    private async void OnNewDummyClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _useDummy = true;
        ApplySnapshot(DummyDataGenerator.Generate(_dummyRnd));
    }

    private async Task FetchLiveAsync()
    {
        ModeBanner.Text = "Lesur inn KVF …";
        var snap = await KvfApi.FetchLv26Async(CancellationToken.None);
        if (snap != null)
            ApplySnapshot(snap);
    }

    private void ApplySnapshot(ElectionSnapshot snap)
    {
        _lastCandidates = snap.Candidates;
        _lastParties = snap.Parties;
        _useDummy = snap.IsDummy;
        var presentLetters = new HashSet<string>(snap.Parties.Select(p => p.Letter), StringComparer.Ordinal);
        var savedSelection = _selectionOrder.Where(presentLetters.Contains).ToList();

        _rows.Clear();
        _selectionOrder.Clear();
        PartyRowsHost.Children.Clear();

        if (snap.IsDummy)
        {
            _parliamentTotalSeats = DummyDataGenerator.DummyTotalSeats;
            _majoritySeats = DummyDataGenerator.DummyMajority;
        }
        else
        {
            _parliamentTotalSeats = snap.ParliamentTotalSeats;
            _majoritySeats = snap.MajoritySeats;
        }

        ModeBanner.Text = snap.IsDummy
            ? "Støða: PRÓGV (⌘D / Ctrl+D → KVF)"
            : "Støða: løgtingsval (KVF) (⌘D / Ctrl+D → prógv)";
        HeaderDetail.Text = $"{snap.TitleLine}\n{snap.SubLine}";

        BtnDummyNew.IsVisible = snap.IsDummy;

        PartyRowsHost.Children.Add(BuildHeaderRow());

        foreach (var p in snap.Parties)
            PartyRowsHost.Children.Add(BuildPartyRow(p));

        foreach (var letter in savedSelection)
        {
            _selectionOrder.Add(letter);
            var row = _rows.FirstOrDefault(r => r.Letter == letter);
            if (row != null)
                row.Host.BorderBrush = new SolidColorBrush(Color.Parse("#5B9FFF"));
        }

        RecalculateCoalitions();
        UpdateArc();
        RefreshCoalitionsWindowIfOpen();
        RefreshCandidateSide(preserveTopsScroll: true);
    }

    private void RefreshCandidateSide(bool preserveTopsScroll)
    {
        var persons = _lastCandidates.Where(c => !c.IsPartyListRow).ToList();
        var electedFull = CandidateListHelper.ElectedByTopMandatesPerParty(_lastCandidates, _lastParties);
        var topsFull = persons
            .OrderByDescending(c => c.PersonalVotes)
            .ThenBy(c => c.PartyLetter, StringComparer.Ordinal)
            .ThenBy(c => c.Name, StringComparer.Ordinal)
            .ToList();

        var q = PeopleSearch.Text?.Trim() ?? "";
        var elected = string.IsNullOrEmpty(q)
            ? electedFull
            : electedFull.Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        var tops = string.IsNullOrEmpty(q)
            ? topsFull
            : topsFull.Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        var baseInfo =
            $"Vald persónar: {electedFull.Count} (hægst M í hvørjum flokki eftir persónligum atkvøðum, har M = mandatatal) · " +
            $"Persónrøðir: {persons.Count} · «Listin»-røðir: {_lastCandidates.Count - persons.Count}.";
        if (!string.IsNullOrEmpty(q))
            baseInfo += $" · Leitan «{q}»: {elected.Count} vald · {tops.Count} topskorarar.";
        PeopleInfoHeader.Text = baseInfo;

        var badgeByLetter = _lastParties.ToDictionary(p => p.Letter, p => p.BadgeColorHex, StringComparer.Ordinal);

        Vector topsOff = default;
        var restoreTops = preserveTopsScroll && PeopleTabs.SelectedIndex == 1;
        if (restoreTops)
            topsOff = ScrollTops.Offset;

        ElectedPanelHost.Content = elected.Count == 0
            ? new TextBlock { Text = "Einki at vísa.", Opacity = 0.7, Margin = new Thickness(8) }
            : CandidateListHelper.BuildCandidateGridStarRows(elected, badgeByLetter);

        ScrollTops.Content = CandidateListHelper.BuildCandidateGridAutoRows(tops, badgeByLetter);

        if (restoreTops)
        {
            Dispatcher.UIThread.Post(() => { ScrollTops.Offset = topsOff; }, DispatcherPriority.Loaded);
        }
    }

    private void RefreshCoalitionsWindowIfOpen()
    {
        if (_coalitionsWindow == null)
            return;
        var parties = _rows.Select(r => (r.Letter, r.Name, r.Mandates)).ToList();
        if (parties.Count == 0)
            return;
        _coalitionsWindow.RefreshData(parties, _majoritySeats, _parliamentTotalSeats);
    }

    private static Grid BuildHeaderRow()
    {
        var g = new Grid { Margin = new Thickness(0, 0, 0, 6) };
        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        g.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("100")));
        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("72")));
        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("56")));

        void cell(int c, string t, FontWeight w = FontWeight.Bold)
        {
            var tb = new TextBlock { Text = t, FontWeight = w, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
            Grid.SetColumn(tb, c);
            g.Children.Add(tb);
        }

        cell(0, " ");
        cell(1, "Flokkur");
        cell(2, "Atkvæði", FontWeight.Bold);
        cell(3, "%", FontWeight.Bold);
        cell(4, "Md.", FontWeight.Bold);
        return g;
    }

    private Control BuildPartyRow(PartyRow p)
    {
        var g = new Grid { MinHeight = 36 };
        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        g.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("100")));
        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("72")));
        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("56")));

        var badge = new Border
        {
            Background = new SolidColorBrush(Color.Parse(p.BadgeColorHex)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 4),
            Margin = new Thickness(0, 0, 8, 0),
            Child = new TextBlock
            {
                Text = p.Letter,
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        Grid.SetColumn(badge, 0);

        var name = new TextBlock
        {
            Text = p.Name,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 8, 0),
        };
        Grid.SetColumn(name, 1);

        var votes = new TextBlock
        {
            Text = p.Votes.ToString("N0", CultureInfo.GetCultureInfo("da-DK")),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 8, 0),
        };
        Grid.SetColumn(votes, 2);

        var pct = new TextBlock
        {
            Text = p.VotesPctDisplay,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 8, 0),
        };
        Grid.SetColumn(pct, 3);

        var mandatesTb = new TextBlock
        {
            Text = p.Mandates.ToString(CultureInfo.InvariantCulture),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
        };
        Grid.SetColumn(mandatesTb, 4);

        g.Children.Add(badge);
        g.Children.Add(name);
        g.Children.Add(votes);
        g.Children.Add(pct);
        g.Children.Add(mandatesTb);

        g.Background = new SolidColorBrush(Color.Parse(p.RowBgHex));

        var wrap = new Border
        {
            Child = g,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(4),
            BorderThickness = new Thickness(2),
            BorderBrush = Brushes.Transparent,
            Background = Brushes.Transparent,
            Margin = new Thickness(0, 2, 0, 2),
            Cursor = new Cursor(StandardCursorType.Hand),
        };

        var ui = new PartyRowUi
        {
            Letter = p.Letter,
            Name = p.Name,
            Mandates = p.Mandates,
            Votes = p.Votes,
            BadgeColorHex = p.BadgeColorHex,
            Host = wrap,
        };

        wrap.PointerPressed += (_, e) =>
        {
            TogglePartySelection(ui);
            e.Handled = true;
        };

        _rows.Add(ui);
        return wrap;
    }

    private void TogglePartySelection(PartyRowUi row)
    {
        var letter = row.Letter;
        if (_selectionOrder.Contains(letter))
        {
            _selectionOrder.Remove(letter);
            row.Host.BorderBrush = Brushes.Transparent;
        }
        else
        {
            _selectionOrder.Add(letter);
            row.Host.BorderBrush = new SolidColorBrush(Color.Parse("#5B9FFF"));
        }

        UpdateArc();
    }

    private void UpdateArc()
    {
        var segments = new List<ArcSegmentDto>();
        var sum = 0;
        foreach (var letter in _selectionOrder)
        {
            var row = _rows.FirstOrDefault(r => r.Letter == letter);
            if (row == null || row.Mandates <= 0)
                continue;
            sum += row.Mandates;
            segments.Add(new ArcSegmentDto(letter, Color.Parse(row.BadgeColorHex), row.Mandates));
        }

        ArcView.SetSegments(segments, sum);
    }

    private void RecalculateCoalitions()
    {
        var parties = _rows.Select(r => (r.Letter, r.Name, r.Mandates)).ToList();

        var totalMandates = parties.Sum(p => p.Item3);
        SumMandatesLabel.Text = $"Tilsamans: {totalMandates} / {_parliamentTotalSeats} mandat — meiriluti: {_majoritySeats}";
    }

    private void OnCoalitionsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var parties = _rows.Select(r => (r.Letter, r.Name, r.Mandates)).ToList();
        if (parties.Count == 0)
            return;
        if (_coalitionsWindow == null)
        {
            _coalitionsWindow = new CoalitionsWindow(parties, _majoritySeats, _parliamentTotalSeats);
            _coalitionsWindow.Closed += (_, _) => _coalitionsWindow = null;
            _coalitionsWindow.Show(this);
        }
        else
        {
            _coalitionsWindow.RefreshData(parties, _majoritySeats, _parliamentTotalSeats);
            _coalitionsWindow.Activate();
        }
    }
}
