using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Logtingsval2026;

internal static class CandidateListHelper
{
    public static List<CandidateRow> ElectedByTopMandatesPerParty(
        IReadOnlyList<CandidateRow> candidates,
        IReadOnlyList<PartyRow> parties)
    {
        var elected = new List<CandidateRow>();
        foreach (var party in parties.OrderBy(p => p.Letter, StringComparer.Ordinal))
        {
            var m = party.Mandates;
            if (m <= 0)
                continue;
            var top = candidates
                .Where(c => !c.IsPartyListRow && string.Equals(c.PartyLetter, party.Letter, StringComparison.Ordinal))
                .OrderByDescending(c => c.PersonalVotes)
                .ThenBy(c => c.Name, StringComparer.Ordinal)
                .Take(m)
                .ToList();
            elected.AddRange(top);
        }

        return elected
            .OrderBy(c => c.PartyLetter, StringComparer.Ordinal)
            .ThenByDescending(c => c.PersonalVotes)
            .ToList();
    }

    /// <summary>Rúða við jøvnari rókkhædd — til vald liste uttan scroll (t.d. 33).</summary>
    public static Grid BuildCandidateGridStarRows(
        IReadOnlyList<CandidateRow> items,
        IReadOnlyDictionary<string, string> partyBadgeHexByLetter,
        double dataFontSize = 10,
        double headerFontSize = 11)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.4, GridUnitType.Star)));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        for (var i = 0; i < items.Count; i++)
            grid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));

        void Cell(int row, int col, string text, bool header, IBrush? bg, IBrush? fg, bool stretch, double font)
        {
            var background = header
                ? new SolidColorBrush(Color.Parse("#2e2e38"))
                : bg ?? new SolidColorBrush(Color.Parse("#1a1a22"));
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = header ? FontWeight.Bold : FontWeight.Normal,
                FontSize = font,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxLines = 2,
            };
            if (fg != null)
                tb.Foreground = fg;
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.Parse("#3d3d48")),
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(6, 2),
                Background = background,
                Child = tb,
                VerticalAlignment = stretch ? VerticalAlignment.Stretch : VerticalAlignment.Center,
            };
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            grid.Children.Add(border);
        }

        Cell(0, 0, "#", true, null, null, false, headerFontSize);
        Cell(0, 1, "Bk.", true, null, null, false, headerFontSize);
        Cell(0, 2, "Flokkur", true, null, null, false, headerFontSize);
        Cell(0, 3, "Navn", true, null, null, false, headerFontSize);
        Cell(0, 4, "Pers. atk.", true, null, null, false, headerFontSize);

        var da = CultureInfo.GetCultureInfo("da-DK");
        for (var i = 0; i < items.Count; i++)
        {
            var r = i + 1;
            var item = items[i];
            Cell(r, 0, (i + 1).ToString(CultureInfo.InvariantCulture), false, null, null, true, dataFontSize);
            Cell(r, 1, item.PartyLetter, false, null, null, true, dataFontSize);

            if (partyBadgeHexByLetter.TryGetValue(item.PartyLetter, out var hex))
            {
                try
                {
                    Cell(r, 2, item.PartyName, false, new SolidColorBrush(Color.Parse(hex)), Brushes.White, true, dataFontSize);
                }
                catch (FormatException)
                {
                    Cell(r, 2, item.PartyName, false, null, null, true, dataFontSize);
                }
            }
            else
                Cell(r, 2, item.PartyName, false, null, null, true, dataFontSize);

            Cell(r, 3, item.Name, false, null, null, true, dataFontSize);
            Cell(r, 4, item.PersonalVotes.ToString("N0", da), false, null, null, true, dataFontSize);
        }

        return grid;
    }

    /// <summary>Vanlig rúða við Auto røðum — til scroll (topskorarar).</summary>
    public static Grid BuildCandidateGridAutoRows(
        IReadOnlyList<CandidateRow> items,
        IReadOnlyDictionary<string, string> partyBadgeHexByLetter)
    {
        var grid = new Grid();
        for (var c = 0; c < 5; c++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        var rowCount = items.Count + 1;
        for (var r = 0; r < rowCount; r++)
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        void Cell(int row, int col, string text, bool header = false, IBrush? background = null, IBrush? foreground = null)
        {
            var bg = header
                ? new SolidColorBrush(Color.Parse("#2e2e38"))
                : background ?? new SolidColorBrush(Color.Parse("#1a1a22"));
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = header ? FontWeight.Bold : FontWeight.Normal,
                FontSize = header ? 12 : 11,
                VerticalAlignment = VerticalAlignment.Center,
            };
            if (foreground != null)
                tb.Foreground = foreground;

            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.Parse("#3d3d48")),
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(8, 5),
                Background = bg,
                Child = tb,
            };
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            grid.Children.Add(border);
        }

        Cell(0, 0, "#", true);
        Cell(0, 1, "Bk.", true);
        Cell(0, 2, "Flokkur", true);
        Cell(0, 3, "Navn", true);
        Cell(0, 4, "Pers. atk.", true);

        var da = CultureInfo.GetCultureInfo("da-DK");
        for (var i = 0; i < items.Count; i++)
        {
            var r = i + 1;
            var item = items[i];
            Cell(r, 0, (i + 1).ToString(CultureInfo.InvariantCulture));
            Cell(r, 1, item.PartyLetter);

            if (partyBadgeHexByLetter.TryGetValue(item.PartyLetter, out var hex))
            {
                try
                {
                    Cell(r, 2, item.PartyName, false, new SolidColorBrush(Color.Parse(hex)), Brushes.White);
                }
                catch (FormatException)
                {
                    Cell(r, 2, item.PartyName);
                }
            }
            else
                Cell(r, 2, item.PartyName);

            Cell(r, 3, item.Name);
            Cell(r, 4, item.PersonalVotes.ToString("N0", da));
        }

        return grid;
    }
}
