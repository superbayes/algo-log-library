using System;
using System.Threading.Tasks;

namespace AlgoLibrary.Interfaces
{
    /// <summary>
    /// 图像处理器接口
    /// 定义图像处理的基本操作
    /// </summary>
    public interface IImageProcessor : IDisposable
    {
        /// <summary>
        /// 加载图像
        /// </summary>
        /// <param name="imagePath">图像文件路径</param>
        /// <returns>是否加载成功</returns>
        bool LoadImage(string imagePath);

        /// <summary>
        /// 异步加载图像
        /// </summary>
        /// <param name="imagePath">图像文件路径</param>
        /// <returns>是否加载成功</returns>
        Task<bool> LoadImageAsync(string imagePath);

        /// <summary>
        /// 保存图像
        /// </summary>
        /// <param name="outputPath">输出文件路径</param>
        /// <returns>是否保存成功</returns>
        bool SaveImage(string outputPath);

        /// <summary>
        /// 异步保存图像
        /// </summary>
        /// <param name="outputPath">输出文件路径</param>
        /// <returns>是否保存成功</returns>
        Task<bool> SaveImageAsync(string outputPath);

        /// <summary>
        /// 转换为灰度图像
        /// </summary>
        /// <returns>是否转换成功</returns>
        bool ConvertToGrayScale();

        /// <summary>
        /// 异步转换为灰度图像
        /// </summary>
        /// <returns>是否转换成功</returns>
        Task<bool> ConvertToGrayScaleAsync();

        /// <summary>
        /// 调整图像大小
        /// </summary>
        /// <param name="width">目标宽度</param>
        /// <param name="height">目标高度</param>
        /// <returns>是否调整成功</returns>
        bool Resize(int width, int height);

        /// <summary>
        /// 异步调整图像大小
        /// </summary>
        /// <param name="width">目标宽度</param>
        /// <param name="height">目标高度</param>
        /// <returns>是否调整成功</returns>
        Task<bool> ResizeAsync(int width, int height);

        /// <summary>
        /// 应用边缘检测
        /// </summary>
        /// <returns>是否应用成功</returns>
        bool ApplyEdgeDetection();

        /// <summary>
        /// 异步应用边缘检测
        /// </summary>
        /// <returns>是否应用成功</returns>
        Task<bool> ApplyEdgeDetectionAsync();

        /// <summary>
        /// 获取图像信息
        /// </summary>
        /// <returns>图像信息字符串</returns>
        string GetImageInfo();
    }
}
