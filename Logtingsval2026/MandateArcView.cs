using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Logtingsval2026;

/// <summary>
/// Halvcirkel-gauge: 0 venstre, 17 top, 33 højre. Segmenter animeres blødt ved ændring; etiketter spredes så de ikke overlapper.
/// </summary>
public sealed class MandateArcView : Control
{
    public const int ArcMaxSeats = 33;

    public const int MajoritySeats = 17;

    private readonly DispatcherTimer _timer;
    private readonly List<double> _animMandates = [];
    private List<ArcSegmentDto> _targets = [];
    private int _targetSum;
    private double _animSum;

    public MandateArcView()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (_, _) => OnAnimTick();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _timer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    public void SetSegments(IReadOnlyList<ArcSegmentDto> segments, int selectedSum)
    {
        _targets = segments.ToList();
        _targetSum = selectedSum;

        while (_animMandates.Count < _targets.Count)
            _animMandates.Add(0);
        while (_animMandates.Count > _targets.Count)
            _animMandates.RemoveAt(_animMandates.Count - 1);

        _timer.Start();
        InvalidateVisual();
    }

    private void OnAnimTick()
    {
        const double k = 0.2;
        var moving = false;
        for (var i = 0; i < _targets.Count; i++)
        {
            var tgt = (double)_targets[i].Mandates;
            var a = _animMandates[i];
            var n = a + (tgt - a) * k;
            if (Math.Abs(tgt - n) < 0.03)
                n = tgt;
            else
                moving = true;
            _animMandates[i] = n;
        }

        _animSum += (_targetSum - _animSum) * k;
        if (Math.Abs(_targetSum - _animSum) < 0.04)
            _animSum = _targetSum;
        else
            moving = true;

        InvalidateVisual();
        if (!moving)
            _timer.Stop();
    }

    private static double ThetaAt(double m)
    {
        m = Math.Clamp(m, 0, ArcMaxSeats);
        if (m <= MajoritySeats)
            return Math.PI - m / MajoritySeats * (Math.PI / 2);
        return Math.PI / 2 * (ArcMaxSeats - m) / (ArcMaxSeats - MajoritySeats);
    }

    private static Point PointOnArc(Point center, double radius, double m)
    {
        var θ = ThetaAt(m);
        return new Point(center.X + radius * Math.Cos(θ), center.Y - radius * Math.Sin(θ));
    }

    /// <summary>Oven på buen: retning væk fra centrum i punktet for m.</summary>
    private static Vector OutwardNormal(Point center, double r, double m)
    {
        var p = PointOnArc(center, r, m);
        var vx = p.X - center.X;
        var vy = p.Y - center.Y;
        var len = Math.Sqrt(vx * vx + vy * vy);
        if (len < 1e-6) return new Vector(0, -1);
        return new Vector(vx / len, vy / len);
    }

    private static void DrawThickArcPolyline(
        DrawingContext ctx,
        Point center,
        double r,
        double mStart,
        double mEnd,
        Pen pen,
        int steps)
    {
        if (mEnd <= mStart) return;
        steps = Math.Clamp(steps, 8, 96);
        var geo = new StreamGeometry();
        using (var s = geo.Open())
        {
            var first = true;
            for (var i = 0; i <= steps; i++)
            {
                var t = i / (double)steps;
                var m = mStart + t * (mEnd - mStart);
                var p = PointOnArc(center, r, m);
                if (first)
                {
                    s.BeginFigure(p, false);
                    first = false;
                }
                else
                    s.LineTo(p);
            }
        }
        ctx.DrawGeometry(null, pen, geo);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var pad = 14.0;
        var cx = w / 2;
        var r = Math.Min(w / 2 - pad, (h - pad - 36) / 1.05);
        if (r < 40) r = 40;
        var cy = h - pad;
        var center = new Point(cx, cy);
        const double labelRadiusExtra = 32;

        var trackPen = new Pen(new SolidColorBrush(Color.FromArgb(100, 120, 125, 135)), 22, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        DrawThickArcPolyline(context, center, r, 0, ArcMaxSeats, trackPen, 64);

        // Byg segmenter med animerede længder
        var visuals = new List<(ArcSegmentDto seg, double m0, double m1)>();
        double acc = 0;
        for (var i = 0; i < _targets.Count; i++)
        {
            var seg = _targets[i];
            var wv = i < _animMandates.Count ? _animMandates[i] : 0;
            if (wv <= 1e-6) continue;
            var m0 = Math.Clamp(acc, 0, ArcMaxSeats);
            acc += wv;
            var m1 = Math.Clamp(acc, 0, ArcMaxSeats);
            if (m1 > m0)
                visuals.Add((seg, m0, m1));
            if (acc >= ArcMaxSeats)
                break;
        }

        var nSeg = visuals.Count;
        for (var i = 0; i < nSeg; i++)
        {
            var (seg, m0, m1) = visuals[i];
            var pen = new Pen(new SolidColorBrush(seg.Color), 20, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
            var steps = Math.Max(8, (int)(40 * (m1 - m0) / ArcMaxSeats));
            DrawThickArcPolyline(context, center, r, m0, m1, pen, steps);
        }

        // Etiketter: midpoint i mandat-space + spredning når segmenter er korte / mange
        for (var i = 0; i < nSeg; i++)
        {
            var (seg, m0, m1) = visuals[i];
            var mid = (m0 + m1) / 2;
            var span = m1 - m0;
            var spread = 0.0;
            if (nSeg > 1)
            {
                var baseSpread = span < 1.8 ? 1.15 : span < 3.5 ? 0.55 : 0.28;
                spread = (i - (nSeg - 1) / 2.0) * baseSpread;
            }
            var labelM = Math.Clamp(mid + spread, 0, ArcMaxSeats);

            var lp = PointOnArc(center, r + labelRadiusExtra, labelM);
            var nrm = OutwardNormal(center, r, labelM);
            lp += nrm * 6;

            var ft = new FormattedText(
                seg.Letter,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                16,
                Brushes.White);
            context.DrawText(ft, new Point(lp.X - ft.Width / 2, lp.Y - ft.Height / 2));
        }

        var top = PointOnArc(center, r, MajoritySeats);
        var dash = new DashStyle(new double[] { 4, 4 }, 0);
        var majPen = new Pen(Brushes.White, 1.5) { DashStyle = dash, LineCap = PenLineCap.Flat };
        context.DrawLine(majPen, center, top);

        var majLbl = new FormattedText(
            $"{MajoritySeats} meirilutamórk",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            10,
            new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)));
        context.DrawText(majLbl, new Point(top.X - 48, top.Y - 20));

        foreach (var (m, txt, vec) in new[]
                 {
                     (0, "0", new Vector(-2, 10)),
                     (ArcMaxSeats, "33", new Vector(-12, 10)),
                 })
        {
            var p = PointOnArc(center, r, m);
            var ft = new FormattedText(
                txt,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                11,
                new SolidColorBrush(Color.FromArgb(200, 200, 205, 215)));
            context.DrawText(ft, p + vec);
        }

        var sumInt = (int)Math.Round(_animSum, MidpointRounding.AwayFromZero);
        var big = new FormattedText(
            sumInt.ToString(CultureInfo.InvariantCulture),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(Typeface.Default.FontFamily, FontStyle.Normal, FontWeight.Bold),
            42,
            Brushes.White);
        context.DrawText(big, new Point(cx - big.Width / 2, cy - r * 0.72 - big.Height / 2));

        var hint = new FormattedText(
            _targets.Count == 0
                ? "Trýst á flokkar · summa á løgtingsskala 0–33"
                : $"valt (rør · í mest {ArcMaxSeats} á búganum)",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            11,
            new SolidColorBrush(Color.FromArgb(160, 200, 200, 210)));
        context.DrawText(hint, new Point(10, h - 20));
    }
}

public readonly record struct ArcSegmentDto(string Letter, Color Color, int Mandates);
