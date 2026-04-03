using OpenCvSharp;

namespace AlgoLibrary.Implementations.Utils;

public static class AlgoUtils
{
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
    /// 对灰度图像进行 Hough 变换检测线
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
            double angleDeg = angle * 180.0 / Math.PI;
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
            param1: param1,          // Canny边缘检测的高阈值
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
}
