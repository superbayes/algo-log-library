using System;
using AlgoLibrary.Implementations.Utils;

namespace SampleApp
{
    /// <summary>
    /// 测试 AlgoUtils 类的类
    /// </summary>
    public static class TestAlgoUtils
    {
        /// <summary>
        /// 运行测试
        /// </summary>
        public static void RunTest()
        {
            Console.WriteLine($"=== 测试 AlgoUtils 类 ===");
            Console.WriteLine();
            
            // 测试1: RadianToAngle0To180 函数 - 基本转换
            Console.WriteLine("测试1: RadianToAngle0To180 函数 - 基本转换");
            try
            {
                // 测试 0 弧度 -> 0 度
                double result1 = AlgoUtils.RadianToAngle0To180(0);
                Console.WriteLine($"输入: 0 弧度, 输出: {result1:F2} 度");
                Console.WriteLine($"预期: 0.00 度");
                Console.WriteLine($"测试结果: {(Math.Abs(result1 - 0) < 0.01 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试2: RadianToAngle0To180 函数 - π/2 弧度 -> 90 度
            Console.WriteLine("测试2: RadianToAngle0To180 函数 - π/2 弧度 -> 90 度");
            try
            {
                double result2 = AlgoUtils.RadianToAngle0To180(Math.PI / 2);
                Console.WriteLine($"输入: π/2 弧度, 输出: {result2:F2} 度");
                Console.WriteLine($"预期: 90.00 度");
                Console.WriteLine($"测试结果: {(Math.Abs(result2 - 90) < 0.01 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试3: RadianToAngle0To180 函数 - π 弧度 -> 0 度（归一化到 [0, 180)）
            Console.WriteLine("测试3: RadianToAngle0To180 函数 - π 弧度 -> 0 度");
            try
            {
                double result3 = AlgoUtils.RadianToAngle0To180(Math.PI);
                Console.WriteLine($"输入: π 弧度, 输出: {result3:F2} 度");
                Console.WriteLine($"预期: 0.00 度（归一化）");
                Console.WriteLine($"测试结果: {(Math.Abs(result3 - 0) < 0.01 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试4: RadianToAngle0To180 函数 - 3π/2 弧度 -> 90 度（归一化）
            Console.WriteLine("测试4: RadianToAngle0To180 函数 - 3π/2 弧度 -> 90 度");
            try
            {
                double result4 = AlgoUtils.RadianToAngle0To180(3 * Math.PI / 2);
                Console.WriteLine($"输入: 3π/2 弧度, 输出: {result4:F2} 度");
                Console.WriteLine($"预期: 90.00 度（归一化）");
                Console.WriteLine($"测试结果: {(Math.Abs(result4 - 90) < 0.01 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试5: RadianToAngle0To180 函数 - 负弧度测试
            Console.WriteLine("测试5: RadianToAngle0To180 函数 - 负弧度测试");
            try
            {
                double result5 = AlgoUtils.RadianToAngle0To180(-Math.PI / 4);
                Console.WriteLine($"输入: -π/4 弧度, 输出: {result5:F2} 度");
                Console.WriteLine($"预期: 135.00 度（-45° + 180°）");
                Console.WriteLine($"测试结果: {(Math.Abs(result5 - 135) < 0.01 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试6: IsRectFullyInside 函数 - 基本测试
            Console.WriteLine("测试6: IsRectFullyInside 函数 - 基本测试");
            try
            {
                // 创建一个测试图像（100x100）
                using var testImage = new OpenCvSharp.Mat(100, 100, OpenCvSharp.MatType.CV_8UC1, new OpenCvSharp.Scalar(0));
                
                // 测试矩形完全在图像内
                var rect1 = new OpenCvSharp.Rect(10, 10, 50, 50);
                bool result6a = AlgoUtils.IsRectFullyInside(testImage, rect1);
                Console.WriteLine($"图像: 100x100, 矩形: (10,10,50,50)");
                Console.WriteLine($"结果: {result6a}, 预期: True");
                
                // 测试矩形部分在图像外
                var rect2 = new OpenCvSharp.Rect(90, 90, 20, 20);
                bool result6b = AlgoUtils.IsRectFullyInside(testImage, rect2);
                Console.WriteLine($"图像: 100x100, 矩形: (90,90,20,20)");
                Console.WriteLine($"结果: {result6b}, 预期: False");
                
                Console.WriteLine($"测试结果: {(result6a && !result6b ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试7: CalculateAverageRectWithoutOutliers 函数 - 基本测试
            Console.WriteLine("测试7: CalculateAverageRectWithoutOutliers 函数 - 基本测试");
            try
            {
                // 创建一组矩形，所有矩形尺寸相同 (50x50)
                var rects = new OpenCvSharp.Rect[]
                {
                    new OpenCvSharp.Rect(10, 10, 50, 50),   // 正常矩形
                    new OpenCvSharp.Rect(15, 15, 50, 50),   // 正常矩形
                    new OpenCvSharp.Rect(12, 12, 50, 50),   // 正常矩形
                    new OpenCvSharp.Rect(100, 100, 50, 50), // 离群矩形（位置远离其他矩形）
                    new OpenCvSharp.Rect(200, 200, 50, 50)  // 离群矩形
                };
                
                var result7 = AlgoUtils.CalculateAverageRectWithoutOutliers(rects);
                Console.WriteLine($"输入矩形: 5个矩形 (50x50)");
                Console.WriteLine($"矩形位置: (10,10), (15,15), (12,12), (100,100), (200,200)");
                Console.WriteLine($"计算的平均矩形: ({result7.X}, {result7.Y}, {result7.Width}, {result7.Height})");
                
                // 预期结果：排除离群矩形后，平均位置应该在 (12, 12) 附近
                bool testPassed = Math.Abs(result7.X - 12) <= 5 && 
                                 Math.Abs(result7.Y - 12) <= 5 &&
                                 result7.Width == 50 && 
                                 result7.Height == 50;
                
                Console.WriteLine($"预期: 排除离群矩形后的平均位置，尺寸保持 50x50");
                Console.WriteLine($"测试结果: {(testPassed ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试8: CalculateAverageRectWithoutOutliers 函数 - 单个矩形测试
            Console.WriteLine("测试8: CalculateAverageRectWithoutOutliers 函数 - 单个矩形测试");
            try
            {
                var singleRect = new OpenCvSharp.Rect[] { new OpenCvSharp.Rect(30, 40, 60, 70) };
                var result8 = AlgoUtils.CalculateAverageRectWithoutOutliers(singleRect);
                Console.WriteLine($"输入矩形: 1个矩形 (60x70) 位置 (30,40)");
                Console.WriteLine($"计算的平均矩形: ({result8.X}, {result8.Y}, {result8.Width}, {result8.Height})");
                
                bool testPassed = result8.X == 30 && result8.Y == 40 && 
                                 result8.Width == 60 && result8.Height == 70;
                
                Console.WriteLine($"预期: 与原矩形相同");
                Console.WriteLine($"测试结果: {(testPassed ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试9: CalculateAverageRectWithoutOutliers 函数 - 无离群矩形测试
            Console.WriteLine("测试9: CalculateAverageRectWithoutOutliers 函数 - 无离群矩形测试");
            try
            {
                var rects = new OpenCvSharp.Rect[]
                {
                    new OpenCvSharp.Rect(100, 100, 30, 30),
                    new OpenCvSharp.Rect(105, 105, 30, 30),
                    new OpenCvSharp.Rect(110, 110, 30, 30),
                    new OpenCvSharp.Rect(115, 115, 30, 30),
                    new OpenCvSharp.Rect(120, 120, 30, 30)
                };
                
                var result9 = AlgoUtils.CalculateAverageRectWithoutOutliers(rects);
                Console.WriteLine($"输入矩形: 5个紧密排列的矩形 (30x30)");
                Console.WriteLine($"计算的平均矩形: ({result9.X}, {result9.Y}, {result9.Width}, {result9.Height})");
                
                // 预期平均位置应该在 (110, 110) 附近
                bool testPassed = Math.Abs(result9.X - 110) <= 5 && 
                                 Math.Abs(result9.Y - 110) <= 5 &&
                                 result9.Width == 30 && 
                                 result9.Height == 30;
                
                Console.WriteLine($"预期: 所有矩形都保留，平均位置在中心附近");
                Console.WriteLine($"测试结果: {(testPassed ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试10: CalculateRoiAverageBrightness 函数 - 灰度图像测试
            Console.WriteLine("测试10: CalculateRoiAverageBrightness 函数 - 灰度图像测试");
            try
            {
                // 创建一个100x100的灰度测试图像，所有像素值为128（中等灰度）
                using var testImage = new OpenCvSharp.Mat(100, 100, OpenCvSharp.MatType.CV_8UC1, new OpenCvSharp.Scalar(128));
                
                // 测试矩形完全在图像内
                var rect1 = new OpenCvSharp.Rect(10, 10, 50, 50);
                double result10a = AlgoUtils.CalculateRoiAverageBrightness(testImage, rect1);
                Console.WriteLine($"图像: 100x100灰度图（所有像素值128）");
                Console.WriteLine($"ROI矩形: (10,10,50,50)");
                Console.WriteLine($"计算的平均亮度: {result10a:F2}");
                Console.WriteLine($"预期: 128.00（所有像素值相同）");
                
                bool testPassed10a = Math.Abs(result10a - 128.0) < 0.1;
                Console.WriteLine($"测试结果: {(testPassed10a ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试11: CalculateRoiAverageBrightness 函数 - 彩色图像测试
            Console.WriteLine("测试11: CalculateRoiAverageBrightness 函数 - 彩色图像测试");
            try
            {
                // 创建一个100x100的彩色测试图像，所有像素值为(64, 128, 192)
                using var testImage = new OpenCvSharp.Mat(100, 100, OpenCvSharp.MatType.CV_8UC3, new OpenCvSharp.Scalar(64, 128, 192));
                
                // 测试矩形完全在图像内
                var rect1 = new OpenCvSharp.Rect(20, 20, 30, 30);
                double result11 = AlgoUtils.CalculateRoiAverageBrightness(testImage, rect1);
                Console.WriteLine($"图像: 100x100彩色图（所有像素值B=64,G=128,R=192）");
                Console.WriteLine($"ROI矩形: (20,20,30,30)");
                Console.WriteLine($"计算的平均亮度: {result11:F2}");
                Console.WriteLine($"预期: 约140.00（转换为灰度后的平均值：0.299*192 + 0.587*128 + 0.114*64 ≈ 140.00）");
                
                // 彩色图转换为灰度图的计算公式：Gray = 0.299*R + 0.587*G + 0.114*B
                // 对于(64,128,192): Gray = 0.299*192 + 0.587*128 + 0.114*64 ≈ 140.0
                bool testPassed11 = Math.Abs(result11 - 140.0) < 1.0;
                Console.WriteLine($"测试结果: {(testPassed11 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试12: CalculateRoiAverageBrightness 函数 - 不同亮度区域测试
            Console.WriteLine("测试12: CalculateRoiAverageBrightness 函数 - 不同亮度区域测试");
            try
            {
                // 创建一个100x100的灰度测试图像，左半部分为0，右半部分为255
                using var testImage = new OpenCvSharp.Mat(100, 100, OpenCvSharp.MatType.CV_8UC1, new OpenCvSharp.Scalar(0));
                
                // 设置右半部分为255 - 使用SubMat和SetTo方法
                var rightHalf = new OpenCvSharp.Rect(50, 0, 50, 100);
                using (var rightHalfMat = testImage.SubMat(rightHalf))
                {
                    rightHalfMat.SetTo(new OpenCvSharp.Scalar(255));
                }
                
                // 测试左半部分的ROI
                var leftRect = new OpenCvSharp.Rect(10, 10, 30, 30);
                double result12a = AlgoUtils.CalculateRoiAverageBrightness(testImage, leftRect);
                Console.WriteLine($"图像: 100x100灰度图（左半部分0，右半部分255）");
                Console.WriteLine($"左半部分ROI矩形: (10,10,30,30)");
                Console.WriteLine($"计算的平均亮度: {result12a:F2}");
                Console.WriteLine($"预期: 0.00（左半部分全黑）");
                
                bool testPassed12a = Math.Abs(result12a - 0.0) < 0.1;
                Console.WriteLine($"测试结果: {(testPassed12a ? "通过" : "失败")}");
                Console.WriteLine();
                
                // 测试右半部分的ROI
                var rightRect = new OpenCvSharp.Rect(60, 10, 30, 30);
                double result12b = AlgoUtils.CalculateRoiAverageBrightness(testImage, rightRect);
                Console.WriteLine($"右半部分ROI矩形: (60,10,30,30)");
                Console.WriteLine($"计算的平均亮度: {result12b:F2}");
                Console.WriteLine($"预期: 255.00（右半部分全白）");
                
                bool testPassed12b = Math.Abs(result12b - 255.0) < 0.1;
                Console.WriteLine($"测试结果: {(testPassed12b ? "通过" : "失败")}");
                Console.WriteLine();
                
                // 测试跨越边界的ROI
                var crossRect = new OpenCvSharp.Rect(40, 10, 30, 30);
                double result12c = AlgoUtils.CalculateRoiAverageBrightness(testImage, crossRect);
                Console.WriteLine($"跨越边界ROI矩形: (40,10,30,30)");
                Console.WriteLine($"计算的平均亮度: {result12c:F2}");
                Console.WriteLine($"预期: 170.00（10个像素为0，20个像素为255的平均值：(10*0 + 20*255)/30 = 170.00）");
                
                bool testPassed12c = Math.Abs(result12c - 170.0) < 0.1;
                Console.WriteLine($"测试结果: {(testPassed12c ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试13: CalculateRoiAverageBrightness 函数 - 异常情况测试
            Console.WriteLine("测试13: CalculateRoiAverageBrightness 函数 - 异常情况测试");
            try
            {
                using var testImage = new OpenCvSharp.Mat(100, 100, OpenCvSharp.MatType.CV_8UC1, new OpenCvSharp.Scalar(128));
                
                // 测试无效矩形（宽度为0）
                var invalidRect = new OpenCvSharp.Rect(10, 10, 0, 50);
                try
                {
                    double result13a = AlgoUtils.CalculateRoiAverageBrightness(testImage, invalidRect);
                    Console.WriteLine($"测试无效矩形（宽度为0）: 失败（应抛出异常但未抛出）");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"测试无效矩形（宽度为0）: 通过（正确抛出ArgumentException）");
                }
                Console.WriteLine();
                
                // 测试超出图像范围的矩形
                var outOfBoundsRect = new OpenCvSharp.Rect(90, 90, 20, 20);
                try
                {
                    double result13b = AlgoUtils.CalculateRoiAverageBrightness(testImage, outOfBoundsRect);
                    Console.WriteLine($"测试超出图像范围的矩形: 失败（应抛出异常但未抛出）");
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine($"测试超出图像范围的矩形: 通过（正确抛出ArgumentOutOfRangeException）");
                }
                Console.WriteLine();
                
                // 测试空图像
                using var emptyImage = new OpenCvSharp.Mat();
                var validRect = new OpenCvSharp.Rect(0, 0, 10, 10);
                try
                {
                    double result13c = AlgoUtils.CalculateRoiAverageBrightness(emptyImage, validRect);
                    Console.WriteLine($"测试空图像: 失败（应抛出异常但未抛出）");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"测试空图像: 通过（正确抛出ArgumentException）");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            
            Console.WriteLine("\n所有测试完成！");
            
            // 只有在控制台输入可用时才等待按键
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("按任意键继续...");
                Console.ReadKey();
            }
        }
        
        // 可选：添加辅助测试方法
        private static void TestSpecificScenario()
        {
            // 具体测试逻辑
        }
    }
}
