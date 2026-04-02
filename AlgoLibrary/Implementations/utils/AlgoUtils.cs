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

    public static LineSegmentPoint[] DetectLinesHoughP(
        Mat grayImage,
        double cannyThreshold1 = 50,
        double cannyThreshold2 = 150,
        int cannyApertureSize = 3,
        bool cannyL2Gradient = false,
        OpenCvSharp.Rect? roi = null)
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

        if (roi.HasValue)
        {
            roiRect = roi.Value;
            if (!IsRectFullyInside(gray, roiRect))
            {
                convertedGray?.Dispose();
                throw new ArgumentOutOfRangeException(nameof(roi), "ROI 超出图像范围");
            }

            roiMat = new Mat(gray, roiRect);
            grayForDetect = roiMat;
        }

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

        if (!roi.HasValue || lines.Length == 0)
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

}
