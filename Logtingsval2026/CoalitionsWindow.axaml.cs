using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Logtingsval2026;

public partial class CoalitionsWindow : Window
{
    public CoalitionsWindow()
    {
        InitializeComponent();
    }

    public CoalitionsWindow(
        IReadOnlyList<(string Letter, string Name, int Seats)> parties,
        int majorityNeed,
        int parliamentSeats)
        : this()
    {
        ApplyGrids(parties, majorityNeed, parliamentSeats);
    }

    /// <summary>Kald ved hver live-opdatering: tabeller nulstilles og beregnes forfra.</summary>
    public void RefreshData(
        IReadOnlyList<(string Letter, string Name, int Seats)> parties,
        int majorityNeed,
        int parliamentSeats) =>
        ApplyGrids(parties, majorityNeed, parliamentSeats);

    private void ApplyGrids(
        IReadOnlyList<(string Letter, string Name, int Seats)> parties,
        int majorityNeed,
        int parliamentSeats)
    {
        InfoHeader.Text =
            $"Meiriluti: {majorityNeed} av {parliamentSeats} mandat. Rúður: bókstavur = mandat í samslagnum.";

        var letters = parties.Select(p => p.Letter).OrderBy(x => x).ToList();
        var (minimal, all) = CoalitionMath.BuildCoalitionTables(parties, majorityNeed, parliamentSeats);

        ScrollMinimal.Content = BuildExcelGrid(letters, minimal);
        ScrollAll.Content = BuildExcelGrid(letters, all);
    }

    private static Control BuildExcelGrid(IReadOnlyList<string> letters, IReadOnlyList<CoalitionMath.CoalitionTableEntry> rows)
    {
        var grid = new Grid();
        var cols = 3 + letters.Count;
        for (var c = 0; c < cols; c++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        var rowCount = rows.Count + 1;
        for (var r = 0; r < rowCount; r++)
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        void Cell(int r, int c, string text, bool header = false)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.Parse("#3d3d48")),
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(8, 5),
                Background = header
                    ? new SolidColorBrush(Color.Parse("#2e2e38"))
                    : new SolidColorBrush(Color.Parse("#1a1a22")),
                Child = new TextBlock
                {
                    Text = text,
                    FontWeight = header ? FontWeight.Bold : FontWeight.Normal,
                    FontSize = header ? 12 : 11,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
            Grid.SetRow(border, r);
            Grid.SetColumn(border, c);
            grid.Children.Add(border);
        }

        Cell(0, 0, "Slag", true);
        Cell(0, 1, "#", true);
        var c0 = 2;
        for (var i = 0; i < letters.Count; i++)
            Cell(0, c0 + i, letters[i], true);
        Cell(0, c0 + letters.Count, "Σ", true);

        for (var ri = 0; ri < rows.Count; ri++)
        {
            var row = rows[ri];
            var r = ri + 1;
            Cell(r, 0, row.Category);
            Cell(r, 1, row.RowIndex.ToString());
            for (var i = 0; i < letters.Count; i++)
            {
                var letter = letters[i];
                var v = row.SeatByLetter.TryGetValue(letter, out var s) && s > 0 ? s.ToString() : "";
                Cell(r, c0 + i, v);
            }
            Cell(r, c0 + letters.Count, row.Sum.ToString());
        }

        return grid;
    }
}
