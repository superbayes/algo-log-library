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

}