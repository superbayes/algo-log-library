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


    /// <summary>
    /// 基于傅里叶变换提取轮廓的特征向量
    /// 将轮廓点集转换为复数序列，应用离散傅里叶变换（DFT），
    /// 提取固定数量的傅里叶描述符作为特征向量
    /// </summary>
    /// <param name="points">轮廓点集，通常为闭合轮廓的边界点</param>
    /// <param name="descriptorLength">特征向量的长度（傅里叶描述符的数量）</param>
    /// <returns>固定长度的特征向量，包含归一化的傅里叶描述符幅度</returns>
    /// <exception cref="ArgumentNullException">点集为空时抛出</exception>
    /// <exception cref="ArgumentException">点集数量不足或描述符长度无效时抛出</exception>
    public static double[] ExtractFourierDescriptors(IEnumerable<Point2f> points, int descriptorLength = 32)
    {
        if (points == null)
            throw new ArgumentNullException(nameof(points));

        var pts = points as Point2f[] ?? points.ToArray();

        if (pts.Length < 3)
            throw new ArgumentException("点集数量不足，至少需要3个点以形成轮廓", nameof(points));

        if (descriptorLength <= 0 || descriptorLength > pts.Length)
            throw new ArgumentException("描述符长度必须大于0且不超过点集数量", nameof(descriptorLength));

        // 1. 将点集转换为复数序列 (x + iy)
        var complexPoints = new Mat(1, pts.Length, MatType.CV_64FC2);
        for (int i = 0; i < pts.Length; i++)
        {
            complexPoints.Set(0, i, new Vec2d(pts[i].X, pts[i].Y));
        }

        // 2. 应用离散傅里叶变换
        using var dftResult = new Mat();
        Cv2.Dft(complexPoints, dftResult, DftFlags.ComplexOutput);

        // 3. 提取傅里叶系数
        var descriptors = new double[descriptorLength];

        // 获取第一个系数（DC分量）用于归一化
        var dcComponent = dftResult.At<Vec2d>(0, 0);
        double dcMagnitude = Math.Sqrt(dcComponent.Item0 * dcComponent.Item0 + dcComponent.Item1 * dcComponent.Item1);

        if (Math.Abs(dcMagnitude) < 1e-12)
            throw new InvalidOperationException("DC分量为零，无法进行归一化");

        // 4. 提取前N个傅里叶系数（排除DC分量）
        for (int i = 0; i < descriptorLength; i++)
        {
            // 使用循环索引，确保不超过可用系数数量
            int idx = i % (pts.Length / 2);
            if (idx == 0 && i > 0) idx = 1; // 跳过DC分量

            var coefficient = dftResult.At<Vec2d>(0, idx);
            double magnitude = Math.Sqrt(coefficient.Item0 * coefficient.Item0 + coefficient.Item1 * coefficient.Item1);

            // 归一化：除以DC分量的幅度，使特征具有尺度不变性
            descriptors[i] = magnitude / dcMagnitude;
        }

        // 5. 可选：对特征向量进行进一步归一化（L2归一化）
        double norm = 0;
        for (int i = 0; i < descriptorLength; i++)
        {
            norm += descriptors[i] * descriptors[i];
        }

        norm = Math.Sqrt(norm);
        if (norm > 1e-12)
        {
            for (int i = 0; i < descriptorLength; i++)
            {
                descriptors[i] /= norm;
            }
        }

        return descriptors;
    }

    /// <summary>
    /// 基于傅里叶变换提取轮廓的特征向量（增强版本）
    /// 提供更多选项控制特征提取过程，包括尺度、旋转和起始点归一化
    /// </summary>
    /// <param name="points">轮廓点集</param>
    /// <param name="descriptorLength">特征向量长度</param>
    /// <param name="normalizeScale">是否进行尺度归一化</param>
    /// <param name="normalizeRotation">是否进行旋转归一化</param>
    /// <param name="normalizeStartingPoint">是否进行起始点归一化</param>
    /// <returns>固定长度的特征向量</returns>
    public static double[] ExtractFourierDescriptorsEx(
        IEnumerable<Point2f> points,
        int descriptorLength = 32,
        bool normalizeScale = true,
        bool normalizeRotation = true,
        bool normalizeStartingPoint = true)
    {
        if (points == null)
            throw new ArgumentNullException(nameof(points));

        var pts = points as Point2f[] ?? points.ToArray();

        if (pts.Length < 3)
            throw new ArgumentException("点集数量不足，至少需要3个点以形成轮廓", nameof(points));

        if (descriptorLength <= 0 || descriptorLength > pts.Length)
            throw new ArgumentException("描述符长度必须大于0且不超过点集数量", nameof(descriptorLength));

        // 如果需要起始点归一化，找到距离质心最远的点作为起始点
        if (normalizeStartingPoint)
        {
            // 计算质心
            double centerX = 0, centerY = 0;
            foreach (var pt in pts)
            {
                centerX += pt.X;
                centerY += pt.Y;
            }
            centerX /= pts.Length;
            centerY /= pts.Length;

            // 找到距离质心最远的点
            int startIndex = 0;
            double maxDist = 0;
            for (int i = 0; i < pts.Length; i++)
            {
                double dist = Math.Pow(pts[i].X - centerX, 2) + Math.Pow(pts[i].Y - centerY, 2);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    startIndex = i;
                }
            }

            // 重新排列点集，以最远点作为起始点
            var rearranged = new Point2f[pts.Length];
            for (int i = 0; i < pts.Length; i++)
            {
                rearranged[i] = pts[(startIndex + i) % pts.Length];
            }
            pts = rearranged;
        }

        // 1. 将点集转换为复数序列 (x + iy)
        var complexPoints = new Mat(1, pts.Length, MatType.CV_64FC2);
        for (int i = 0; i < pts.Length; i++)
        {
            complexPoints.Set(0, i, new Vec2d(pts[i].X, pts[i].Y));
        }

        // 2. 应用离散傅里叶变换
        using var dftResult = new Mat();
        Cv2.Dft(complexPoints, dftResult, DftFlags.ComplexOutput);

        // 3. 提取傅里叶系数
        var descriptors = new double[descriptorLength];

        // 获取DC分量（第一个系数）
        var dcComponent = dftResult.At<Vec2d>(0, 0);
        double dcMagnitude = Math.Sqrt(dcComponent.Item0 * dcComponent.Item0 + dcComponent.Item1 * dcComponent.Item1);
        double dcPhase = Math.Atan2(dcComponent.Item1, dcComponent.Item0);

        if (Math.Abs(dcMagnitude) < 1e-12)
            throw new InvalidOperationException("DC分量为零，无法进行归一化");

        // 4. 提取傅里叶描述符
        for (int i = 0; i < descriptorLength; i++)
        {
            // 使用低频分量（更具代表性），跳过DC分量
            int idx = i + 1;
            if (idx >= pts.Length) idx = pts.Length - 1;

            var coefficient = dftResult.At<Vec2d>(0, idx);
            double magnitude = Math.Sqrt(coefficient.Item0 * coefficient.Item0 + coefficient.Item1 * coefficient.Item1);
            double phase = Math.Atan2(coefficient.Item1, coefficient.Item0);

            // 尺度归一化
            if (normalizeScale)
            {
                magnitude /= dcMagnitude;
            }

            // 旋转归一化：减去DC分量的相位
            if (normalizeRotation)
            {
                phase -= dcPhase;
                // 相位信息在旋转归一化后主要用于一致性，这里我们只关心幅度
                // 注意：这里我们保留了原始的幅度值，因为旋转不影响幅度
            }

            descriptors[i] = magnitude;
        }

        // 5. 如果需要旋转归一化，使用第一个非DC分量的相位进行进一步归一化
        if (normalizeRotation && descriptorLength >= 2)
        {
            // 获取第一个非DC分量的相位
            var firstCoeff = dftResult.At<Vec2d>(0, 1);
            double firstPhase = Math.Atan2(firstCoeff.Item1, firstCoeff.Item0);

            // 通过相位旋转使第一个非DC分量的相位为零
            for (int i = 0; i < descriptorLength; i++)
            {
                int idx = i + 1;
                if (idx >= pts.Length) idx = pts.Length - 1;

                var coefficient = dftResult.At<Vec2d>(0, idx);
                double real = coefficient.Item0;
                double imag = coefficient.Item1;

                // 旋转复数：乘以 exp(-i*firstPhase)
                double cosPhase = Math.Cos(-firstPhase);
                double sinPhase = Math.Sin(-firstPhase);
                double rotatedReal = real * cosPhase - imag * sinPhase;
                double rotatedImag = real * sinPhase + imag * cosPhase;

                // 计算旋转后的幅度
                double rotatedMagnitude = Math.Sqrt(rotatedReal * rotatedReal + rotatedImag * rotatedImag);

                // 尺度归一化
                if (normalizeScale)
                {
                    rotatedMagnitude /= dcMagnitude;
                }

                descriptors[i] = rotatedMagnitude;
            }
        }

        // 6. L2归一化
        double norm = 0;
        for (int i = 0; i < descriptorLength; i++)
        {
            norm += descriptors[i] * descriptors[i];
        }

        norm = Math.Sqrt(norm);
        if (norm > 1e-12)
        {
            for (int i = 0; i < descriptorLength; i++)
            {
                descriptors[i] /= norm;
            }
        }

        return descriptors;
    }

}
