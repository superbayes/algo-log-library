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
    /// 计算排除离群矩形后的平均矩形
    /// 所有矩形具有相同的宽度和高度，仅位置可能不同
    /// </summary>
    /// <param name="rects">矩形集合</param>
    /// <param name="outlierThreshold">离群阈值，默认1.5（基于中位数绝对偏差MAD方法）</param>
    /// <returns>平均矩形，保持原始宽度和高度</returns>
    /// <exception cref="ArgumentNullException">输入矩形集合为空时抛出</exception>
    /// <exception cref="ArgumentException">矩形集合为空或数量不足时抛出</exception>
    public static OpenCvSharp.Rect CalculateAverageRectWithoutOutliers(
        IEnumerable<OpenCvSharp.Rect> rects,
        double outlierThreshold = 1.5)
    {
        if (rects == null)
            throw new ArgumentNullException(nameof(rects));

        var rectArray = rects as OpenCvSharp.Rect[] ?? rects.ToArray();
        if (rectArray.Length == 0)
            throw new ArgumentException("矩形集合不能为空", nameof(rects));

        if (rectArray.Length == 1)
            return rectArray[0];

        // 提取所有矩形的中心点
        var centers = rectArray.Select(r => new Point2f(r.X + r.Width / 2.0f, r.Y + r.Height / 2.0f)).ToArray();

        // 使用中位数作为初始参考点（对离群值更鲁棒）
        var medianCenter = CalculateMedianPoint(centers);
        
        // 计算每个点到中位点的距离
        var distances = centers.Select(c => Distance(c, medianCenter)).ToArray();
        
        // 计算距离的中位数绝对偏差（MAD）
        var mad = CalculateMAD(distances);
        
        // 如果MAD为0（所有点相同），直接使用所有点
        if (mad < 1e-6)
        {
            return CalculateAverageRectFromIndices(rectArray, Enumerable.Range(0, rectArray.Length).ToList());
        }
        
        // 找出非离群点（距离 <= medianDistance + outlierThreshold * mad）
        double medianDistance = CalculateMedian(distances);
        var validIndices = new List<int>();
        for (int i = 0; i < distances.Length; i++)
        {
            if (distances[i] <= medianDistance + outlierThreshold * mad)
            {
                validIndices.Add(i);
            }
        }

        // 如果没有有效的矩形，使用所有矩形
        if (validIndices.Count == 0)
        {
            validIndices = Enumerable.Range(0, rectArray.Length).ToList();
        }

        // 计算非离群矩形的平均矩形
        return CalculateAverageRectFromIndices(rectArray, validIndices);
    }

    /// <summary>
    /// 计算点的中位数
    /// </summary>
    private static Point2f CalculateMedianPoint(Point2f[] points)
    {
        if (points.Length == 0)
            return new Point2f(0, 0);
            
        var xValues = points.Select(p => p.X).OrderBy(x => x).ToArray();
        var yValues = points.Select(p => p.Y).OrderBy(y => y).ToArray();
        
        float medianX, medianY;
        int n = xValues.Length;
        
        if (n % 2 == 0)
        {
            medianX = (xValues[n / 2 - 1] + xValues[n / 2]) / 2.0f;
            medianY = (yValues[n / 2 - 1] + yValues[n / 2]) / 2.0f;
        }
        else
        {
            medianX = xValues[n / 2];
            medianY = yValues[n / 2];
        }
        
        return new Point2f(medianX, medianY);
    }

    /// <summary>
    /// 计算中位数绝对偏差（MAD）
    /// </summary>
    private static double CalculateMAD(float[] values)
    {
        if (values.Length == 0)
            return 0;
            
        // 计算中位数
        var sortedValues = values.OrderBy(v => v).ToArray();
        double median;
        int n = sortedValues.Length;
        
        if (n % 2 == 0)
        {
            median = (sortedValues[n / 2 - 1] + sortedValues[n / 2]) / 2.0;
        }
        else
        {
            median = sortedValues[n / 2];
        }
        
        // 计算绝对偏差
        var absoluteDeviations = values.Select(v => Math.Abs(v - median)).ToArray();
        
        // 计算绝对偏差的中位数
        var sortedDeviations = absoluteDeviations.OrderBy(d => d).ToArray();
        double mad;
        
        if (n % 2 == 0)
        {
            mad = (sortedDeviations[n / 2 - 1] + sortedDeviations[n / 2]) / 2.0;
        }
        else
        {
            mad = sortedDeviations[n / 2];
        }
        
        return mad;
    }

    /// <summary>
    /// 计算中位数
    /// </summary>
    private static double CalculateMedian(float[] values)
    {
        if (values.Length == 0)
            return 0;
            
        var sortedValues = values.OrderBy(v => v).ToArray();
        int n = sortedValues.Length;
        
        if (n % 2 == 0)
        {
            return (sortedValues[n / 2 - 1] + sortedValues[n / 2]) / 2.0;
        }
        else
        {
            return sortedValues[n / 2];
        }
    }

    /// <summary>
    /// 从指定索引计算平均矩形
    /// </summary>
    private static OpenCvSharp.Rect CalculateAverageRectFromIndices(OpenCvSharp.Rect[] rectArray, List<int> indices)
    {
        if (indices.Count == 0)
            return rectArray[0];

        // 计算非离群矩形的平均位置
        float avgX = 0, avgY = 0;
        foreach (int idx in indices)
        {
            var rect = rectArray[idx];
            avgX += rect.X + rect.Width / 2.0f;
            avgY += rect.Y + rect.Height / 2.0f;
        }

        avgX /= indices.Count;
        avgY /= indices.Count;

        // 使用第一个矩形的宽度和高度（假设所有矩形尺寸相同）
        var firstRect = rectArray[0];
        int avgRectX = (int)(avgX - firstRect.Width / 2.0f);
        int avgRectY = (int)(avgY - firstRect.Height / 2.0f);

        return new OpenCvSharp.Rect(avgRectX, avgRectY, firstRect.Width, firstRect.Height);
    }

    /// <summary>
    /// 计算两点之间的欧氏距离
    /// </summary>
    private static float Distance(Point2f p1, Point2f p2)
    {
        float dx = p1.X - p2.X;
        float dy = p1.Y - p2.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 使用IQR方法过滤离群值的辅助函数
    /// </summary>
    /// <param name="values">数值数组</param>
    /// <param name="threshold">离群阈值，默认1.5</param>
    /// <returns>离群值的索引集合</returns>
    private static HashSet<int> FilterOutliers(float[] values, double threshold = 1.5)
    {
        var outliers = new HashSet<int>();
        
        if (values.Length < 4) // IQR方法需要至少4个数据点
            return outliers;

        // 复制并排序值
        var sortedValues = values.ToArray();
        Array.Sort(sortedValues);

        // 计算四分位数
        int n = sortedValues.Length;
        float q1, q3;

        if (n % 2 == 0)
        {
            // 偶数个元素
            q1 = sortedValues[n / 4];
            q3 = sortedValues[(3 * n) / 4];
        }
        else
        {
            // 奇数个元素
            q1 = sortedValues[n / 4];
            q3 = sortedValues[(3 * n) / 4];
        }

        float iqr = q3 - q1;
        float lowerBound = q1 - (float)(threshold * iqr);
        float upperBound = q3 + (float)(threshold * iqr);

        // 找出离群值
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] < lowerBound || values[i] > upperBound)
            {
                outliers.Add(i);
            }
        }

        return outliers;
    }

    /// <summary>
    /// 计算图像ROI区域的平均亮度（灰度平均值）
    /// </summary>
    /// <param name="mat">输入图像（灰度图或彩色图）</param>
    /// <param name="rect">感兴趣区域（ROI）矩形</param>
    /// <returns>ROI区域的平均亮度值（对于彩色图，返回所有通道的平均值）</returns>
    /// <exception cref="ArgumentNullException">输入图像为空时抛出</exception>
    /// <exception cref="ArgumentException">输入图像为空图像或矩形无效时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">ROI超出图像范围时抛出</exception>
    public static double CalculateRoiAverageBrightness(Mat mat, OpenCvSharp.Rect rect)
    {
        if (mat == null)
            throw new ArgumentNullException(nameof(mat));
        if (mat.Empty())
            throw new ArgumentException("输入图像为空", nameof(mat));
        if (rect.Width <= 0 || rect.Height <= 0)
            throw new ArgumentException("矩形尺寸无效", nameof(rect));
        
        // 检查矩形是否完全在图像范围内
        if (!IsRectFullyInside(mat, rect))
            throw new ArgumentOutOfRangeException(nameof(rect), "ROI矩形超出图像范围");

        Mat? convertedGray = null;
        Mat gray = mat;

        // 如果输入图像不是灰度图，转换为灰度图
        if (mat.Channels() != 1)
        {
            convertedGray = new Mat();
            Cv2.CvtColor(mat, convertedGray, ColorConversionCodes.BGR2GRAY);
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

        try
        {
            // 提取ROI区域
            using var roi = new Mat(gray, rect);
            
            // 计算ROI的平均值
            var mean = Cv2.Mean(roi);
            
            // 对于灰度图，mean[0]就是平均亮度
            // 对于彩色图转换为的灰度图，同样使用mean[0]
            double averageBrightness = mean[0];
            
            return averageBrightness;
        }
        finally
        {
            // 释放转换的图像资源
            convertedGray?.Dispose();
        }
    }

    
}
