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
            
            Console.WriteLine("\n所有测试完成！按任意键继续...");
            Console.ReadKey();
        }
        
        // 可选：添加辅助测试方法
        private static void TestSpecificScenario()
        {
            // 具体测试逻辑
        }
    }
}
