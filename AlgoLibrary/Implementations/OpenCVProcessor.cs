using System;
using System.Threading.Tasks;
using AlgoLibrary.Interfaces;
using LogLibrary.Interfaces;
using OpenCvSharp;

namespace AlgoLibrary.Implementations
{
    /// <summary>
    /// 基于OpenCVSharp的图像处理器实现
    /// </summary>
    public class OpenCVProcessor : IImageProcessor
    {
        private readonly ILogger _logger;
        private Mat? _image;
        private bool _disposed = false;

        /// <summary>
        /// 当前加载的图像
        /// </summary>
        public Mat? Image => _image;

        /// <summary>
        /// 是否已加载图像
        /// </summary>
        public bool IsImageLoaded => _image != null && !_image.Empty();

        public OpenCVProcessor(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool LoadImage(string imagePath)
        {
            try
            {
                _logger.Debug($"开始加载图像: {imagePath}");

                _image?.Dispose();
                _image = Cv2.ImRead(imagePath);
                
                if (_image == null || _image.Empty())
                {
                    _logger.Warning($"图像加载失败(空图像): {imagePath}");
                    return false;
                }

                _logger.Info($"图像加载完成: {imagePath}. {GetImageInfo()}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"加载图像时发生错误: {imagePath}", ex);
                return false;
            }
        }

        public async Task<bool> LoadImageAsync(string imagePath)
        {
            return await Task.Run(() => LoadImage(imagePath));
        }

        public bool SaveImage(string outputPath)
        {
            if (!IsImageLoaded)
            {
                _logger.Warning($"保存图像失败：没有可保存的图像。目标路径: {outputPath}");
                return false;
            }

            try
            {
                var success = Cv2.ImWrite(outputPath, _image!);

                if (success)
                {
                    _logger.Info($"图像保存完成: {outputPath}. {GetImageInfo()}");
                }
                else
                {
                    _logger.Warning($"图像保存失败: {outputPath}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.Error($"保存图像时发生错误: {outputPath}", ex);
                return false;
            }
        }

        public async Task<bool> SaveImageAsync(string outputPath)
        {
            if (!IsImageLoaded)
            {
                _logger.Warning($"保存图像失败：没有可保存的图像。目标路径: {outputPath}");
                return false;
            }

            return await Task.Run(() => SaveImage(outputPath));
        }

        public bool ConvertToGrayScale()
        {
            if (!IsImageLoaded)
            {
                _logger.Warning("灰度转换失败：没有可处理的图像");
                return false;
            }

            try
            {
                var grayImage = new Mat();
                Cv2.CvtColor(_image!, grayImage, ColorConversionCodes.BGR2GRAY);
                _image!.Dispose();
                _image = grayImage;

                _logger.Info($"灰度转换完成. {GetImageInfo()}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("转换为灰度图像时发生错误", ex);
                return false;
            }
        }

        public async Task<bool> ConvertToGrayScaleAsync()
        {
            if (!IsImageLoaded)
            {
                _logger.Warning("灰度转换失败：没有可处理的图像");
                return false;
            }

            return await Task.Run(() => ConvertToGrayScale());
        }

        public bool Resize(int width, int height)
        {
            if (!IsImageLoaded)
            {
                _logger.Warning("调整大小失败：没有可处理的图像");
                return false;
            }

            if (width <= 0 || height <= 0)
            {
                _logger.Warning($"调整大小失败：宽度和高度必须大于0 (width={width}, height={height})");
                return false;
            }

            try
            {
                var resizedImage = new Mat();
                Cv2.Resize(_image!, resizedImage, new Size(width, height));
                _image!.Dispose();
                _image = resizedImage;

                _logger.Info($"调整大小完成: {width}x{height}. {GetImageInfo()}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"调整图像大小时发生错误 (width={width}, height={height})", ex);
                return false;
            }
        }

        public async Task<bool> ResizeAsync(int width, int height)
        {
            if (!IsImageLoaded)
            {
                _logger.Warning("调整大小失败：没有可处理的图像");
                return false;
            }

            if (width <= 0 || height <= 0)
            {
                _logger.Warning($"调整大小失败：宽度和高度必须大于0 (width={width}, height={height})");
                return false;
            }

            return await Task.Run(() => Resize(width, height));
        }

        public bool ApplyEdgeDetection()
        {
            if (!IsImageLoaded)
            {
                _logger.Warning("边缘检测失败：没有可处理的图像");
                return false;
            }

            try
            {
                // 先转换为灰度图像
                var grayImage = new Mat();
                Cv2.CvtColor(_image!, grayImage, ColorConversionCodes.BGR2GRAY);

                // 应用Canny边缘检测
                var edges = new Mat();
                Cv2.Canny(grayImage, edges, 50, 150);

                _image!.Dispose();
                _image = edges;
                grayImage.Dispose();
                
                _logger.Info($"边缘检测完成. {GetImageInfo()}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("应用边缘检测时发生错误", ex);
                return false;
            }
        }

        public async Task<bool> ApplyEdgeDetectionAsync()
        {
            if (!IsImageLoaded)
            {
                _logger.Warning("边缘检测失败：没有可处理的图像");
                return false;
            }

            return await Task.Run(() => ApplyEdgeDetection());
        }

        public string GetImageInfo()
        {
            if (!IsImageLoaded)
            {
                return "没有加载图像";
            }

            try
            {
                return $"图像尺寸: {_image!.Width} x {_image.Height}, 通道数: {_image.Channels()}, 类型: {_image.Type()}";
            }
            catch (Exception ex)
            {
                return $"获取图像信息时发生错误: {ex.Message}";
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _image?.Dispose();
                }

                _image = null;
                _disposed = true;
            }
        }

        ~OpenCVProcessor()
        {
            Dispose(false);
        }
    }
}
