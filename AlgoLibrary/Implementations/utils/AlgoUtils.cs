using OpenCvSharp;

namespace AlgoLibrary.Implementations.Utils;

public static class AlgoUtils
{
    /// <summary>
    /// 依据点集拟合直线，返回直线的系数 A, B, C
    /// 其中，A, B, C 分别表示直线的法向量和点到直线的距离
    /// 如果要转成斜截式（非竖直线）：y = kx + b, 则 k = -B / A, b = -C / A
    /// 直线倾斜角度：atan2(B, A) .其中，atan2(B, A) 表示与水平线的夹角，范围为 [-pi, pi]
    /// 
    /// </summary>
    /// <param name="points">输入点集</param>>
    /// <returns></returns>
    public static (double A, double B, double C) FitLineFromPoints(IEnumerable<Point2f> points)
    {
        if (points == null) throw new ArgumentNullException(nameof(points));

        var pts = points as Point2f[] ?? points.ToArray();
        if (pts.Length < 2) throw new ArgumentException("点集数量不足，至少需要2个点", nameof(points));

        DistanceTypes distanceType = DistanceTypes.Huber;
        double param = 0;//仅对 DIST_FAIR, DIST_WELSCH, DIST_HUBER 有效。设为 0 时，OpenCV 会自动选择一个最优值
        double reps = 0.01;//径向精度
        double aeps = 0.01;//角度精度
        var line = Cv2.FitLine(pts, distanceType, param, reps, aeps);

        var vx = (double)line.Vx;
        var vy = (double)line.Vy;
        var x0 = (double)line.X1;
        var y0 = (double)line.Y1;

        var vNorm = Math.Sqrt(vx * vx + vy * vy);
        if (vNorm <= 1e-12) throw new InvalidOperationException("拟合失败：方向向量为0");
        vx /= vNorm;
        vy /= vNorm;

        var a = vy;
        var b = -vx;
        var c = -(a * x0 + b * y0);

        var abNorm = Math.Sqrt(a * a + b * b);
        if (abNorm > 1e-12)
        {
            a /= abNorm;
            b /= abNorm;
            c /= abNorm;
        }

        if (a < 0 || (Math.Abs(a) <= 1e-12 && b < 0))
        {
            a = -a;
            b = -b;
            c = -c;
        }

        return (a, b, c);
    }


    /// <summary>
    /// 输入弧度,输出角度 degrees，
    /// 并归一化到 [0, 180) ，表示与水平线的夹角
    /// <param name="radians"></param>
    /// <returns>角度值</returns>
    public static double RadianToAngle0To180(double radians)
    {
        double degrees = radians * 180.0 / Math.PI;
        return (degrees % 180.0 + 180.0) % 180.0;
    }

    /// <summary>
    /// 检查矩形是否完全在灰度图像范围内
    /// </summary>
    /// <param name="grayImage">灰度图像</param>
    /// <param name="rect">矩形区域</param>
    /// <returns>如果矩形完全在图像范围内则为 true，否则为 false</returns>
    public static bool IsRectFullyInside(Mat grayImage, OpenCvSharp.Rect rect)
    {
        if (grayImage == null || grayImage.Empty())
            return false;

        if (rect.Width <= 0 || rect.Height <= 0)
            return false;

        if (rect.X < 0 || rect.Y < 0)
            return false;

        return rect.Right <= grayImage.Width && rect.Bottom <= grayImage.Height;
    }

    /// <summary>
    /// 对灰度图像进行 Hough直线检测
    /// </summary>
    /// <param name="grayImage"></param>
    /// <param name="cannyThreshold1"></param>
    /// <param name="cannyThreshold2"></param>
    /// <param name="cannyApertureSize"></param>
    /// <param name="cannyL2Gradient"></param>
    /// <param name="roi"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static LineSegmentPoint[] DetectLinesHoughP(
        Mat grayImage,
        double cannyThreshold1 = 50,
        double cannyThreshold2 = 150,
        int cannyApertureSize = 3,
        bool cannyL2Gradient = false
        )
    {
        if (grayImage == null)
            throw new ArgumentNullException(nameof(grayImage));
        if (grayImage.Empty())
            throw new ArgumentException("输入图像为空", nameof(grayImage));

        Mat? convertedGray = null;
        Mat gray = grayImage;

        if (grayImage.Channels() != 1)
        {
            convertedGray = new Mat();
            Cv2.CvtColor(grayImage, convertedGray, ColorConversionCodes.BGR2GRAY);
            gray = convertedGray;
        }

        if (gray.Type() != MatType.CV_8UC1)
        {
            var tmp = new Mat();
            gray.ConvertTo(tmp, MatType.CV_8UC1);
            convertedGray?.Dispose();
            convertedGray = tmp;
            gray = convertedGray;
        }

        Mat? roiMat = null;
        OpenCvSharp.Rect roiRect = default;
        Mat grayForDetect = gray;

        // 1,对灰度图像进行平滑处理
        using var blur = new Mat();
        Cv2.GaussianBlur(grayForDetect, blur, new Size(3, 3), 0.0);
        grayForDetect = blur;
        // 2,应用 Canny 边缘检测
        using var edges = new Mat();
        Cv2.Canny(grayForDetect, edges, cannyThreshold1, cannyThreshold2, cannyApertureSize, cannyL2Gradient);
        // 3,hough 变换检测线
        double rho = 1;
        double theta = Math.PI / 180;
        int houghThreshold = 80;
        double minLineLength = 50;
        double maxLineGap = 10;
        var lines = Cv2.HoughLinesP(
            edges, // 输入图像
            rho, // 线参数 rho,控制线的分辨率，单位为像素数，值越大，线越粗，值越小，线越细，值为1时，线为像素级
            theta, // 线参数 theta,控制角度的分辨率，单位为弧度，值越大，线越细，值越小，线越粗，值为1时，线为1度级
            houghThreshold, // 累计投票数阈值，用于筛选出的线，大于该值的线才会被保留，值越大，线越粗，值越小，线越细，值为80时，线为80像素长
            minLineLength, // 最小线长度，单位为像素数，值越大，线越长，值越小，线越短，值为50时，线为50像素长
            maxLineGap // 最大线间距，单位为像素数，值越大，线越密，值越小，线越疏，值为10时，线间距为10像素长
            );

        roiMat?.Dispose();
        convertedGray?.Dispose();

        if (lines.Length == 0)
            return lines;

        int dx = roiRect.X;
        int dy = roiRect.Y;
        var shifted = new LineSegmentPoint[lines.Length];// 转换后的线段,每一个LineSegmentPoint包含了线段的两个端点
        for (int i = 0; i < lines.Length; i++)
        {
            var l = lines[i];
            shifted[i] = new LineSegmentPoint(
                new Point(l.P1.X + dx, l.P1.Y + dy),
                new Point(l.P2.X + dx, l.P2.Y + dy));
        }

        // 过滤掉短线
        shifted = shifted.Where(l => l.Length() > minLineLength).ToArray();

        //计算每一个线段与水平线的夹角
        for (int i = 0; i < shifted.Length; i++)
        {
            var l = shifted[i];
            double angle = Math.Atan2(l.P2.Y - l.P1.Y, l.P2.X - l.P1.X);
            double angleDeg = angle * 180.0 / Math.PI; // 弧度转换为角度
            angleDeg = (angleDeg % 180.0 + 180.0) % 180.0;// 角度转换为0-180度范围
            // LineSegmentPoint 没有 Angle 属性，直接跳过赋值
            // 如需使用角度，可单独存储或改用其他数据结构
        }

        return shifted;
    }

    /// <summary>
    /// 使用霍夫变换检测图像中的圆
    /// </summary>
    /// <param name="image">输入图像（可以是彩色或灰度图）</param>
    /// <param name="dp">累加器分辨率与图像分辨率的反比。dp=1时累加器与输入图像相同分辨率，dp=2时累加器分辨率为输入图像的一半</param>
    /// <param name="minDist">检测到的圆心之间的最小距离。如果太小，可能会检测到多个相邻的圆；如果太大，可能会漏掉一些圆</param>
    /// <param name="param1">Canny边缘检测的高阈值，低阈值自动设为高阈值的一半</param>
    /// <param name="param2">累加器阈值，用于圆心检测。值越小，检测到的圆越多（包括假圆）；值越大，检测到的圆越少但更可靠</param>
    /// <param name="minRadius">要检测的圆的最小半径（像素）</param>
    /// <param name="maxRadius">要检测的圆的最大半径（像素）</param>
    /// <param name="roi">感兴趣区域（ROI），如果为null则处理整个图像</param>
    /// <returns>检测到的圆数组，每个圆包含圆心坐标和半径</returns>
    /// <exception cref="ArgumentNullException">输入图像为空时抛出</exception>
    /// <exception cref="ArgumentException">输入图像为空图像时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">ROI超出图像范围时抛出</exception>
    public static CircleSegment[] DetectHoughCircleFromImage(
        Mat image,
        double dp = 1.0,
        double minDist = 50.0,
        double param1 = 100.0,
        double param2 = 30.0,
        int minRadius = 10,
        int maxRadius = 100
        )
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image));
        if (image.Empty())
            throw new ArgumentException("输入图像为空", nameof(image));

        Mat? convertedGray = null;
        Mat gray = image;

        // 如果输入图像不是灰度图，转换为灰度图
        if (image.Channels() != 1)
        {
            convertedGray = new Mat();
            Cv2.CvtColor(image, convertedGray, ColorConversionCodes.BGR2GRAY);
            gray = convertedGray;
        }

        // 确保图像类型为 CV_8UC1（8位无符号单通道）
        if (gray.Type() != MatType.CV_8UC1)
        {
            var tmp = new Mat();
            gray.ConvertTo(tmp, MatType.CV_8UC1);
            convertedGray?.Dispose();
            convertedGray = tmp;
            gray = convertedGray;
        }

        Mat? roiMat = null;
        OpenCvSharp.Rect roiRect = default;
        Mat grayForDetect = gray;

        // 1. 对灰度图像进行平滑处理，减少噪声
        using var blur = new Mat();
        Cv2.GaussianBlur(grayForDetect, blur, new Size(9, 9), 2.0, 2.0);
        grayForDetect = blur;

        // 2. 使用霍夫圆变换检测圆
        // 参数说明：
        // - dp: 累加器分辨率与图像分辨率的反比，值越大检测速度越快但精度越低
        // - minDist: 检测到的圆心之间的最小距离，避免检测到重叠的圆
        // - param1: Canny边缘检测的高阈值
        // - param2: 累加器阈值，值越大检测标准越严格
        // - minRadius: 要检测的圆的最小半径
        // - maxRadius: 要检测的圆的最大半径
        var circles = Cv2.HoughCircles(
            grayForDetect,           // 输入灰度图像
            HoughModes.Gradient,     // 检测方法，使用梯度法（最常用）
            dp,                      // 累加器分辨率与图像分辨率的反比
            minDist,                 // 圆心之间的最小距离
            param1: param1,          // Canny边缘检测的高阈值,值越大
            param2: param2,          // 累加器阈值
            minRadius: minRadius,    // 最小半径
            maxRadius: maxRadius     // 最大半径
        );

        // 释放资源
        roiMat?.Dispose();
        convertedGray?.Dispose();

        // 如果没有指定ROI或者没有检测到圆，直接返回
        if (circles.Length == 0)
            return circles;

        // 如果指定了ROI，需要将检测到的圆坐标转换回原图像坐标系
        int dx = roiRect.X;
        int dy = roiRect.Y;
        var shifted = new CircleSegment[circles.Length];
        
        for (int i = 0; i < circles.Length; i++)
        {
            var circle = circles[i];
            // 将ROI内的坐标转换为原图像坐标
            shifted[i] = new CircleSegment
            {
                Center = new Point2f(circle.Center.X + dx, circle.Center.Y + dy),
                Radius = circle.Radius
            };
        }

        return shifted;
    }

    /// <summary>
    /// 检测连通域的算法Demo
    /// 演示如何使用OpenCV检测图像中的连通域
    /// </summary>
    /// <param name="image">输入图像（可以是彩色或灰度图）</param>
    public static void DetectConnectAreaDemo(Mat image)
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image));
        if (image.Empty())
            throw new ArgumentException("输入图像为空", nameof(image));

        Mat? convertedGray = null;
        Mat gray = image;

        // 如果输入图像不是灰度图，转换为灰度图
        if (image.Channels() != 1)
        {
            convertedGray = new Mat();
            Cv2.CvtColor(image, convertedGray, ColorConversionCodes.BGR2GRAY);
            gray = convertedGray;
        }

        // 确保图像类型为 CV_8UC1（8位无符号单通道）
        if (gray.Type() != MatType.CV_8UC1)
        {
            var tmp = new Mat();
            gray.ConvertTo(tmp, MatType.CV_8UC1);
            convertedGray?.Dispose();
            convertedGray = tmp;
            gray = convertedGray;
        }

        // 1. 对图像进行二值化处理
        using var binary = new Mat();
        //采用自适应阈值的二值化处理，自动调整阈值以适应不同光照条件
        Cv2.AdaptiveThreshold(gray, binary, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 0.5);

        // 2. 查找连通域轮廓
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

        Console.WriteLine($"检测到 {contours.Length} 个连通域");

        // 3. 创建彩色图像用于可视化
        Mat resultImage;
        if (image.Channels() == 1)
        {
            // 如果是灰度图，转换为彩色图以便可视化
            resultImage = new Mat();
            Cv2.CvtColor(image, resultImage, ColorConversionCodes.GRAY2BGR);
        }
        else
        {
            resultImage = image.Clone();
        }

        // 4. 绘制检测到的连通域
        Random rnd = new Random();
        for (int i = 0; i < contours.Length; i++)
        {
            // 计算连通域的面积
            double area = Cv2.ContourArea(contours[i]);
            
            // 计算连通域的外接矩形
            OpenCvSharp.Rect boundingRect = Cv2.BoundingRect(contours[i]);
            
            // 过滤掉太小的连通域（面积小于100像素）
            if (area < 100)
                continue;

            // 生成随机颜色
            Scalar color = new Scalar(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256));
            
            // 绘制轮廓
            Cv2.DrawContours(resultImage, contours, i, color, 2);
            
            // 绘制外接矩形
            Cv2.Rectangle(resultImage, boundingRect, color, 1);
            
            // 在矩形上方显示面积
            string areaText = $"Area: {area:F0}";
            Cv2.PutText(resultImage, areaText, 
                new Point(boundingRect.X, boundingRect.Y - 5), 
                HersheyFonts.HersheySimplex, 0.5, color, 1);
            
            Console.WriteLine($"连通域 {i + 1}: 面积 = {area:F0}, 外接矩形 = [{boundingRect.X}, {boundingRect.Y}, {boundingRect.Width}, {boundingRect.Height}]");
        }

        // 5. 显示结果
        Cv2.ImShow("二值化图像", binary);

        // 6. 显示结果
        Cv2.ImShow("原始图像", image);
        Cv2.ImShow("二值化图像", binary);
        Cv2.ImShow("连通域检测结果", resultImage);
        Cv2.WaitKey(0);
        Cv2.DestroyAllWindows();

        // 6. 释放资源
        convertedGray?.Dispose();
        resultImage.Dispose();
        binary.Dispose();
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
