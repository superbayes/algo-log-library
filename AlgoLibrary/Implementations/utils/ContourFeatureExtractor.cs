using OpenCvSharp;
using System;

public static class ContourFeatureExtractor
{
    /// <summary>
    /// 提取轮廓的 14 维固定长度归一化特征向量
    /// </summary>
    /// <param name="contour">输入轮廓点集</param>
    /// <returns>14维 [0~1] 归一化特征向量</returns>
    public static double[] Get14DNormalizedFeature(Point[] contour)
    {
        // 安全校验
        if (contour == null || contour.Length < 5)
            return new double[14]; // 不足点数返回空特征

        // 1. 面积
        double area = Cv2.ContourArea(contour);
        area = Math.Max(area, 1e-6); // 防止为0

        // 2. 周长
        double perimeter = Cv2.ArcLength(contour, true);
        perimeter = Math.Max(perimeter, 1e-6);

        // 3. 外接矩形宽高比
        Rect rect = Cv2.BoundingRect(contour);
        double ratio = rect.Width / (double)(rect.Height + 1e-6);

        // 4. 最小外接圆半径
        Cv2.MinEnclosingCircle(contour, out var center, out float radius);
        radius = MathF.Max(radius, 1e-6f);

        // 5. 凸度（凸包面积/原面积）,越接近1越凸,越小越凹
        Point[] hull = Cv2.ConvexHull(contour);
        double hullArea = Math.Max(Cv2.ContourArea(hull), 1e-6);
        double convexity = Math.Clamp(area / hullArea, 0.01, 1);

        // 6. 重心坐标
        Moments M = Cv2.Moments(contour);
        double cx = M.M10 / (M.M00 + 1e-6);
        double cy = M.M01 / (M.M00 + 1e-6);

        // 7. 椭圆长短轴比
        double ellipseRatio = 1;
        if (contour.Length >= 5)
        {
            RotatedRect ellipse = Cv2.FitEllipse(contour);
            double w = ellipse.Size.Width + 1e-6;
            double h = ellipse.Size.Height + 1e-6;
            ellipseRatio = Math.Max(w, h) / Math.Min(w, h);
        }

        // 8. Hu 矩（7维）
        double[] hu = M.HuMoments();

        // Hu 矩对数压缩（让数值更稳定）
        for (int i = 0; i < 7; i++)
        {
            hu[i] = -Math.Sign(hu[i]) * Math.Log10(Math.Abs(hu[i]) + 1e-10);
        }

        // ===================== 组合 14 维原始特征 =====================
        double[] raw = new double[14];
        raw[0] = area;
        raw[1] = perimeter;
        raw[2] = ratio;
        raw[3] = radius;
        raw[4] = convexity;
        raw[5] = cx;
        raw[6] = cy;
        raw[7] = ellipseRatio;
        raw[8] = hu[0];
        raw[9] = hu[1];
        raw[10] = hu[2];
        raw[11] = hu[3];
        raw[12] = hu[4];
        raw[13] = hu[5]; // 只用前6个Hu，保持14维

        // ===================== 全局归一化到 [0, 1] =====================
        double[] norm = NormalizeMinMax(raw);
        return norm;
    }

    /// <summary>
    /// Min-Max 归一化：把数组缩放到 0~1
    /// </summary>
    private static double[] NormalizeMinMax(double[] input)
    {
        double min = double.MaxValue;
        double max = double.MinValue;

        foreach (var v in input)
        {
            min = Math.Min(min, v);
            max = Math.Max(max, v);
        }

        double[] res = new double[input.Length];
        double range = max - min;

        for (int i = 0; i < input.Length; i++)
        {
            res[i] = range < 1e-6 ? 0 : (input[i] - min) / range;
        }
        return res;
    }
}
