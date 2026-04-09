using OpenCvSharp;
using System;

public static class ContourFeatureExtractor
{
    /// <summary>
    /// 提取轮廓的 14 维固定长度归一化特征向量
    /// 包括了轮廓的几何特征（面积、周长、宽高比等）和形状特征（Hu矩）
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

    /// <summary>
    /// 轮廓归一化 + 极坐标展开,输出固定长度特征向量
    /// 1. 以重心为中心
    /// 2. 轮廓点转极坐标（角度+距离）
    /// 3. 按角度均匀采样
    /// 4. 返回固定长度距离序列（特征向量）
    /// </summary>
    /// <param name="contour">输入轮廓点集</param>
    /// <param name="featureLength">输出特征长度（默认32维）</param>
    /// <returns>固定长度归一化距离特征向量</returns>
    public static double[] ContourToPolarFeature(Point[] contour, int featureLength = 32)
    {
        // 安全判断
        if (contour == null || contour.Length < 3)
            return new double[featureLength];

        // ==========================================
        // 步骤1：计算轮廓重心（作为极坐标原点）
        // ==========================================
        Moments moments = Cv2.Moments(contour);
        double cx = moments.M10 / (moments.M00 + 1e-8); // 重心X
        double cy = moments.M01 / (moments.M00 + 1e-8); // 重心Y

        // ==========================================
        // 步骤2：把所有轮廓点转为 极坐标 (角度, 距离)
        // ==========================================
        List<(double angle, double dist)> polarPoints = new List<(double, double)>();

        foreach (Point p in contour)
        {
            // 点相对于重心的坐标
            double dx = p.X - cx;
            double dy = p.Y - cy;

            // 计算距离
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // 计算角度（转为 0~2π）
            double angle = Math.Atan2(dy, dx);
            if (angle < 0) angle += 2 * Math.PI;

            polarPoints.Add((angle, dist));
        }

        // ==========================================
        // 步骤3：按角度从小到大排序
        // ==========================================
        polarPoints.Sort((a, b) => a.angle.CompareTo(b.angle));

        // ==========================================
        // 步骤4：按角度均匀采样 → 固定长度距离序列
        // ==========================================
        double[] feature = new double[featureLength];
        double stepAngle = 2 * Math.PI / featureLength; // 每段角度步长

        for (int i = 0; i < featureLength; i++)
        {
            // 当前要采样的目标角度
            double targetAngle = i * stepAngle;

            // 找到离目标角度最近的轮廓点 → 取距离
            double minDiff = double.MaxValue;
            double bestDist = 0;

            foreach (var pt in polarPoints)
            {
                // 角度差（环形处理）
                double diff = Math.Abs(pt.angle - targetAngle);
                diff = Math.Min(diff, 2 * Math.PI - diff);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    bestDist = pt.dist;
                }
            }

            feature[i] = bestDist;
        }

        // ==========================================
        // 步骤5：归一化到 [0,1]（便于机器学习）
        // ==========================================
        double maxDist = 0;
        foreach (double d in feature) maxDist = Math.Max(maxDist, d);

        if (maxDist > 1e-8)
        {
            for (int i = 0; i < featureLength; i++)
                feature[i] /= maxDist;
        }

        return feature;
    }

    /// <summary>
    /// 通过线性插值对轮廓进行重采样，并将点集中心化到原点
    /// 算法原理：沿着轮廓累积距离进行均匀采样，然后计算质心并将所有点平移到原点
    /// </summary>
    /// <param name="contour">输入轮廓点集</param>
    /// <param name="sampleCount">输出采样点数量</param>
    /// <returns>中心化到原点的均匀分布轮廓点数组</returns>
    /// <exception cref="ArgumentNullException">轮廓点集为空时抛出</exception>
    /// <exception cref="ArgumentException">轮廓点数量不足或采样数量无效时抛出</exception>
    public static Point2f[] ResampleAndCenterContour(Point[] contour, int sampleCount = 32)
    {
        // 输入验证
        if (contour == null)
            throw new ArgumentNullException(nameof(contour));

        if (contour.Length < 3)
            throw new ArgumentException("轮廓至少需要3个点以形成闭合轮廓", nameof(contour));

        if (sampleCount <= 0)
            throw new ArgumentException("采样数量必须大于0", nameof(sampleCount));

        // 特殊情况处理：如果所有点重合或轮廓长度为0
        double totalLen = Cv2.ArcLength(contour, true);
        if (totalLen < 1e-10)
        {
            // 返回所有点都相同的采样点（已经在原点附近）
            var uniformPoints = new Point2f[sampleCount];
            Point center = contour[0];
            for (int i = 0; i < sampleCount; i++)
            {
                uniformPoints[i] = new Point2f(center.X, center.Y);
            }
            return uniformPoints;
        }

        var resampled = new Point2f[sampleCount];
        double step = totalLen / sampleCount;  // 每个采样点之间的弧长距离
        double accumulatedDistance = 0;        // 当前累积的弧长
        int sampleIndex = 0;                   // 当前采样点索引

        // 遍历轮廓的每条边
        for (int i = 0; i < contour.Length && sampleIndex < sampleCount; i++)
        {
            Point p1 = contour[i];
            Point p2 = contour[(i + 1) % contour.Length];

            // 计算当前边的长度
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double edgeLength = Math.Sqrt(dx * dx + dy * dy);

            if (edgeLength < 1e-10)
            {
                // 边长度为零，跳过这条边
                continue;
            }

            // 沿着当前边生成采样点
            while (accumulatedDistance + edgeLength >= (sampleIndex + 1) * step && sampleIndex < sampleCount)
            {
                // 计算插值参数 t ∈ [0, 1]
                double targetDistance = (sampleIndex + 1) * step;
                double t = (targetDistance - accumulatedDistance) / edgeLength;

                // 确保 t 在有效范围内（处理浮点误差）
                t = Math.Max(0, Math.Min(1, t));

                // 线性插值计算采样点坐标（使用浮点数保持精度）
                double x = p1.X + t * dx;
                double y = p1.Y + t * dy;

                // 存储采样点（使用浮点坐标）
                resampled[sampleIndex] = new Point2f((float)x, (float)y);
                sampleIndex++;
            }

            // 更新累积距离
            accumulatedDistance += edgeLength;
        }

        // 处理最后一个采样点（确保闭合）
        if (sampleIndex < sampleCount)
        {
            resampled[sampleIndex] = new Point2f(contour[0].X, contour[0].Y);
        }

        // 中心化：计算质心并将所有点平移到原点
        double sumX = 0, sumY = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            sumX += resampled[i].X;
            sumY += resampled[i].Y;
        }
        
        double centerX = sumX / sampleCount;
        double centerY = sumY / sampleCount;
        
        // 平移所有点，使质心位于原点
        for (int i = 0; i < sampleCount; i++)
        {
            resampled[i] = new Point2f(
                (float)(resampled[i].X - centerX),
                (float)(resampled[i].Y - centerY)
            );
        }

        return resampled;
    }

    /// <summary>
    /// 通过线性插值对轮廓进行重采样，输出固定数量的均匀分布的轮廓点
    /// （兼容旧版本，不进行中心化）
    /// </summary>
    /// <param name="contour">输入轮廓点集</param>
    /// <param name="sampleCount">输出采样点数量</param>
    /// <returns>均匀分布的轮廓点数组</returns>
    /// <exception cref="ArgumentNullException">轮廓点集为空时抛出</exception>
    /// <exception cref="ArgumentException">轮廓点数量不足或采样数量无效时抛出</exception>
    [Obsolete("Use ResampleAndCenterContour for centered points or consider ExtractResampledContourFeature for feature extraction")]
    public static Point[] ResampleContour(Point[] contour, int sampleCount = 32)
    {
        // 输入验证
        if (contour == null)
            throw new ArgumentNullException(nameof(contour));

        if (contour.Length < 3)
            throw new ArgumentException("轮廓至少需要3个点以形成闭合轮廓", nameof(contour));

        if (sampleCount <= 0)
            throw new ArgumentException("采样数量必须大于0", nameof(sampleCount));

        // 特殊情况处理：如果所有点重合或轮廓长度为0
        double totalLen = Cv2.ArcLength(contour, true);
        if (totalLen < 1e-10)
        {
            // 返回所有点都相同的采样点
            var uniformPoints = new Point[sampleCount];
            Point center = contour[0];
            for (int i = 0; i < sampleCount; i++)
            {
                uniformPoints[i] = center;
            }
            return uniformPoints;
        }

        var resampled = new Point[sampleCount];
        double step = totalLen / sampleCount;  // 每个采样点之间的弧长距离
        double accumulatedDistance = 0;        // 当前累积的弧长
        int sampleIndex = 0;                   // 当前采样点索引

        // 遍历轮廓的每条边
        for (int i = 0; i < contour.Length && sampleIndex < sampleCount; i++)
        {
            Point p1 = contour[i];
            Point p2 = contour[(i + 1) % contour.Length];

            // 计算当前边的长度
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double edgeLength = Math.Sqrt(dx * dx + dy * dy);

            if (edgeLength < 1e-10)
            {
                // 边长度为零，跳过这条边
                continue;
            }

            // 沿着当前边生成采样点
            while (accumulatedDistance + edgeLength >= (sampleIndex + 1) * step && sampleIndex < sampleCount)
            {
                // 计算插值参数 t ∈ [0, 1]
                double targetDistance = (sampleIndex + 1) * step;
                double t = (targetDistance - accumulatedDistance) / edgeLength;

                // 确保 t 在有效范围内（处理浮点误差）
                t = Math.Max(0, Math.Min(1, t));

                // 线性插值计算采样点坐标
                double x = p1.X + t * dx;
                double y = p1.Y + t * dy;

                // 存储采样点（四舍五入到最近的整数）
                resampled[sampleIndex] = new Point((int)Math.Round(x), (int)Math.Round(y));
                sampleIndex++;
            }

            // 更新累积距离
            accumulatedDistance += edgeLength;
        }

        // 处理最后一个采样点（确保闭合）
        if (sampleIndex < sampleCount)
        {
            resampled[sampleIndex] = contour[0];
        }

        return resampled;
    }

    /// <summary>
    /// 基于重采样和中心化轮廓提取固定长度特征向量
    /// 通过对轮廓进行均匀重采样、中心化到原点，然后将坐标展平为特征向量
    /// </summary>
    /// <param name="contour">输入轮廓点集</param>
    /// <param name="featureLength">输出特征向量长度（默认64维：32个点的x和y坐标）</param>
    /// <returns>归一化的固定长度特征向量</returns>
    /// <exception cref="ArgumentNullException">轮廓点集为空时抛出</exception>
    /// <exception cref="ArgumentException">轮廓点数量不足或特征长度无效时抛出</exception>
    public static double[] ExtractResampledContourFeature(Point[] contour, int featureLength = 64)
    {
        // 输入验证
        if (contour == null)
            throw new ArgumentNullException(nameof(contour));

        if (contour.Length < 3)
            throw new ArgumentException("轮廓至少需要3个点以形成闭合轮廓", nameof(contour));

        if (featureLength <= 0 || featureLength % 2 != 0)
            throw new ArgumentException("特征长度必须大于0且为偶数（每个点包含x和y坐标）", nameof(featureLength));

        // 计算采样点数量：特征长度的一半（每个点贡献x和y两个值）
        int sampleCount = featureLength / 2;
        
        // 1. 获取中心化重采样点集
        Point2f[] centeredPoints = ResampleAndCenterContour(contour, sampleCount);
        
        // 2. 将点坐标展平为特征向量
        double[] feature = new double[featureLength];
        for (int i = 0; i < sampleCount; i++)
        {
            feature[2 * i] = centeredPoints[i].X;     // x坐标
            feature[2 * i + 1] = centeredPoints[i].Y; // y坐标
        }
        
        // 3. L2归一化（使特征向量具有单位长度）
        double norm = 0;
        for (int i = 0; i < featureLength; i++)
        {
            norm += feature[i] * feature[i];
        }
        
        norm = Math.Sqrt(norm);
        if (norm > 1e-12)
        {
            for (int i = 0; i < featureLength; i++)
            {
                feature[i] /= norm;
            }
        }
        
        return feature;
    }

    //========================================================================
}
