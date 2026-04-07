using OpenCvSharp;

namespace AlgoLibrary.Implementations.Utils;

public sealed record DropsImageAlgoOptions(
    double CircleDp = 1.0,
    double CircleMinDist = 30.0,
    double CircleCannyHighThreshold = 120.0,
    double CircleAccumulatorThreshold = 30.0,
    int CircleMinRadius = 8,
    int CircleMaxRadius = 200,
    double LineCannyThreshold1 = 50,
    double LineCannyThreshold2 = 150,
    int LineCannyApertureSize = 3,
    bool LineCannyL2Gradient = false,
    int DominantAngleBinSizeDeg = 5,
    double DominantAngleToleranceDeg = 7.5,
    int FitLineMinPoints = 6
);

public readonly record struct FittedLine(Point2f PointOnLine, Point2f Direction, LineSegmentPoint Segment);

public sealed record DropsImageAlgoResult(
    CircleSegment[] Drops,
    LineSegmentPoint[] LineSegments,
    FittedLine? MainLine
);

public static class DropsImageAlgo
{
    public static DropsImageAlgoResult Detect(Mat image, DropsImageAlgoOptions? options = null)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));
        if (image.Empty()) throw new ArgumentException("输入图像为空", nameof(image));

        options ??= new DropsImageAlgoOptions();

        using var gray = EnsureGray8U(image);

        var drops = AlgoUtils.DetectHoughCircleFromImage(
            gray,
            dp: options.CircleDp,
            minDist: options.CircleMinDist,
            param1: options.CircleCannyHighThreshold,
            param2: options.CircleAccumulatorThreshold,
            minRadius: options.CircleMinRadius,
            maxRadius: options.CircleMaxRadius);

        drops = drops.Where(c => IsCircleInside(gray.Size(), c)).ToArray();

        var lines = AlgoUtils.DetectLinesHoughP(
            gray,
            cannyThreshold1: options.LineCannyThreshold1,
            cannyThreshold2: options.LineCannyThreshold2,
            cannyApertureSize: options.LineCannyApertureSize,
            cannyL2Gradient: options.LineCannyL2Gradient);

        var mainLine = TryFitDominantLine(gray.Size(), lines, options, out var fitted) ? fitted : (FittedLine?)null;

        return new DropsImageAlgoResult(drops, lines, mainLine);
    }

    private static Mat EnsureGray8U(Mat src)
    {
        Mat gray;
        if (src.Channels() == 1)
        {
            gray = src.Clone();
        }
        else
        {
            gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
        }

        if (gray.Type() == MatType.CV_8UC1) return gray;

        var converted = new Mat();
        gray.ConvertTo(converted, MatType.CV_8UC1);
        gray.Dispose();
        return converted;
    }

    private static bool IsCircleInside(Size size, CircleSegment c)
    {
        var x = c.Center.X;
        var y = c.Center.Y;
        var r = c.Radius;

        return x - r >= 0 &&
               y - r >= 0 &&
               x + r < size.Width &&
               y + r < size.Height;
    }

    private static bool TryFitDominantLine(Size size, LineSegmentPoint[] segments, DropsImageAlgoOptions options, out FittedLine fitted)
    {
        fitted = default;
        if (segments.Length == 0) return false;

        var binSize = Math.Max(1, options.DominantAngleBinSizeDeg);
        var binCount = 180 / binSize + 1;
        var weights = new double[binCount];

        for (var i = 0; i < segments.Length; i++)
        {
            var s = segments[i];
            var dx = s.P2.X - s.P1.X;
            var dy = s.P2.Y - s.P1.Y;
            var angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            angle = (angle % 180.0 + 180.0) % 180.0;
            var bin = (int)Math.Round(angle / binSize);
            if ((uint)bin >= (uint)binCount) continue;
            weights[bin] += s.Length();
        }

        var bestBin = 0;
        var bestW = weights[0];
        for (var i = 1; i < weights.Length; i++)
        {
            if (weights[i] > bestW)
            {
                bestW = weights[i];
                bestBin = i;
            }
        }

        var dominantAngle = bestBin * binSize;
        var tol = Math.Max(0.0, options.DominantAngleToleranceDeg);

        var points = new List<Point2f>(segments.Length * 2);
        for (var i = 0; i < segments.Length; i++)
        {
            var s = segments[i];
            var dx = s.P2.X - s.P1.X;
            var dy = s.P2.Y - s.P1.Y;
            var angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            angle = (angle % 180.0 + 180.0) % 180.0;

            var d = Math.Abs(angle - dominantAngle);
            d = Math.Min(d, 180.0 - d);
            if (d > tol) continue;

            points.Add(new Point2f(s.P1.X, s.P1.Y));
            points.Add(new Point2f(s.P2.X, s.P2.Y));
        }

        if (points.Count < options.FitLineMinPoints) return false;

        var line = Cv2.FitLine(points, DistanceTypes.L2, 0, 0.01, 0.01);

        var vx = (float)line.Vx;
        var vy = (float)line.Vy;
        var x0 = (float)line.X1;
        var y0 = (float)line.Y1;

        var dir = new Point2f(vx, vy);
        var p0 = new Point2f(x0, y0);

        if (!TryClipInfiniteLineToImage(size, p0, dir, out var clipped)) return false;

        fitted = new FittedLine(p0, dir, clipped);
        return true;
    }

    private static bool TryClipInfiniteLineToImage(Size size, Point2f p0, Point2f dir, out LineSegmentPoint segment)
    {
        segment = default;
        var w = size.Width;
        var h = size.Height;
        if (w <= 0 || h <= 0) return false;

        var candidates = new List<Point2f>(4);

        if (Math.Abs(dir.X) > 1e-6f)
        {
            var t0 = (0 - p0.X) / dir.X;
            var y0 = p0.Y + dir.Y * t0;
            if (y0 >= 0 && y0 <= h - 1) candidates.Add(new Point2f(0, y0));

            var t1 = (w - 1 - p0.X) / dir.X;
            var y1 = p0.Y + dir.Y * t1;
            if (y1 >= 0 && y1 <= h - 1) candidates.Add(new Point2f(w - 1, y1));
        }

        if (Math.Abs(dir.Y) > 1e-6f)
        {
            var t0 = (0 - p0.Y) / dir.Y;
            var x0 = p0.X + dir.X * t0;
            if (x0 >= 0 && x0 <= w - 1) candidates.Add(new Point2f(x0, 0));

            var t1 = (h - 1 - p0.Y) / dir.Y;
            var x1 = p0.X + dir.X * t1;
            if (x1 >= 0 && x1 <= w - 1) candidates.Add(new Point2f(x1, h - 1));
        }

        if (candidates.Count < 2) return false;

        var bestI = 0;
        var bestJ = 1;
        var bestD2 = Distance2(candidates[0], candidates[1]);

        for (var i = 0; i < candidates.Count; i++)
        {
            for (var j = i + 1; j < candidates.Count; j++)
            {
                var d2 = Distance2(candidates[i], candidates[j]);
                if (d2 > bestD2)
                {
                    bestD2 = d2;
                    bestI = i;
                    bestJ = j;
                }
            }
        }

        var a = candidates[bestI];
        var b = candidates[bestJ];
        segment = new LineSegmentPoint(new Point((int)Math.Round(a.X), (int)Math.Round(a.Y)),
            new Point((int)Math.Round(b.X), (int)Math.Round(b.Y)));
        return true;
    }

    private static double Distance2(Point2f a, Point2f b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }
}
